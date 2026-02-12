using HDLabelMaker.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace HDLabelMaker.Services
{
    public enum PrinterMode
    {
        SerialPort,     // COM1, COM2, etc.
        UsbPort,        // USB001, USB002, etc. (raw data)
        WindowsPrinter  // Windows printer queue name
    }

    public class PrintService
    {
        private HDLabelMaker.Models.PrinterSettings _settings;
        private PrinterMode _mode;

        public PrintService(HDLabelMaker.Models.PrinterSettings settings)
        {
            _settings = settings ?? new HDLabelMaker.Models.PrinterSettings();
            _mode = DetectPrinterMode(_settings.Port);
        }

        private PrinterMode DetectPrinterMode(string portName)
        {
            if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                return PrinterMode.SerialPort;
            }
            else if (portName.StartsWith("USB", StringComparison.OrdinalIgnoreCase) ||
                     portName.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
            {
                return PrinterMode.UsbPort;
            }
            else
            {
                return PrinterMode.WindowsPrinter;
            }
        }

        public void UpdateSettings(HDLabelMaker.Models.PrinterSettings settings)
        {
            _settings = settings ?? new HDLabelMaker.Models.PrinterSettings();
            _mode = DetectPrinterMode(_settings.Port);
        }

        public void PrintLabel(LabelTemplate label, int copies = 1)
        {
            if (!label.IsValid)
            {
                throw new FileNotFoundException($"Label file not found: {label.FullPath}");
            }

            try
            {
                switch (_mode)
                {
                    case PrinterMode.SerialPort:
                        PrintViaSerialPort(label, copies);
                        break;
                    case PrinterMode.UsbPort:
                        PrintViaUsbPort(label, copies);
                        break;
                    case PrinterMode.WindowsPrinter:
                        PrintViaWindowsPrinter(label, copies);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to print to {_settings.Port} ({_mode}): {ex.Message}", ex);
            }
        }

        private void PrintViaSerialPort(LabelTemplate label, int copies)
        {
            var (bmpBytes, width, height) = ConvertBmpToPrinterFormat(label.FullPath);

            using (var port = new SerialPort(_settings.Port, 9600, Parity.None, 8, StopBits.One))
            {
                try
                {
                    port.Open();
                    port.WriteTimeout = 5000;

                    for (int i = 0; i < copies; i++)
                    {
                        SendEscPosCommandsToStream(port.BaseStream, bmpBytes, width, height);
                    }

                    port.Write(new byte[] { 0x0A, 0x0A, 0x0A }, 0, 3);
                }
                finally
                {
                    if (port.IsOpen)
                        port.Close();
                }
            }
        }

        private void PrintViaUsbPort(LabelTemplate label, int copies)
        {
            // USB/LPT ports with Windows drivers should use Windows printing
            // Raw port access (\\.\USB001) doesn't work for printer queue ports
            PrintViaWindowsPrinter(label, copies);
        }

        private void PrintViaWindowsPrinter(LabelTemplate label, int copies)
        {
            // Use Windows printing for USB printers with drivers
            // For USB/LPT ports, we need to find the printer name that uses this port
            string printerName = _settings.Port;
            if (_mode == PrinterMode.UsbPort)
            {
                printerName = GetPrinterNameFromPort(_settings.Port);
                if (string.IsNullOrEmpty(printerName))
                {
                    throw new Exception($"No printer found using port {_settings.Port}");
                }
            }

            using (var bmp = PrepareBitmapForPrint(label.FullPath))
            using (var printDoc = new PrintDocument())
            {
                printDoc.PrinterSettings.PrinterName = printerName;
                printDoc.PrinterSettings.Copies = (short)copies;

                // 636x424 = 80mm x 53mm label
                printDoc.DefaultPageSettings.Landscape = false;
                printDoc.DefaultPageSettings.PaperSize = new PaperSize("Custom", 313, 209);

                printDoc.PrintPage += (sender, e) =>
                {
                    e.Graphics.DrawImage(bmp, 0, 0, e.PageBounds.Width, e.PageBounds.Height);
                };

                printDoc.Print();
            }
        }

        private string? GetPrinterNameFromPort(string portName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
                {
                    foreach (ManagementObject printer in searcher.Get())
                    {
                        var printerPort = printer["PortName"]?.ToString() ?? "";
                        if (printerPort.Equals(portName, StringComparison.OrdinalIgnoreCase))
                        {
                            return printer["Name"]?.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to query printer from port: {ex.Message}");
            }
            return null;
        }

        private Bitmap PrepareBitmapForPrint(string filePath)
        {
            using (var original = new Bitmap(filePath))
            {
                // Resize to 53mm tall (424px @ 203 DPI) in the feed direction,
                // maintaining 3:2 aspect ratio: 636x424
                var resized = new Bitmap(original, 636, 424);

                return resized;
            }
        }

        private void InvertBitmap(Bitmap bmp)
        {
            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            try
            {
                int byteCount = Math.Abs(data.Stride) * bmp.Height;
                byte[] pixels = new byte[byteCount];
                Marshal.Copy(data.Scan0, pixels, 0, byteCount);

                for (int i = 0; i < byteCount; i++)
                {
                    pixels[i] = (byte)(255 - pixels[i]);
                }

                Marshal.Copy(pixels, 0, data.Scan0, byteCount);
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        private (byte[] data, int width, int height) ConvertBmpToPrinterFormat(string filePath)
        {
            using (var bmp = PrepareBitmapForPrint(filePath))
            {
                return (ConvertToMonochromeBytes(bmp), bmp.Width, bmp.Height);
            }
        }

        private byte[] ConvertToMonochromeBytes(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            int bytesPerRow = (width + 7) / 8;
            byte[] result = new byte[bytesPerRow * height];

            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            try
            {
                IntPtr ptr = data.Scan0;
                int stride = data.Stride;

                for (int y = 0; y < height; y++)
                {
                    byte[] row = new byte[stride];
                    Marshal.Copy(ptr + (y * stride), row, 0, stride);

                    for (int x = 0; x < width; x++)
                    {
                        int pixelOffset = x * 3;
                        byte b = row[pixelOffset];
                        byte g = row[pixelOffset + 1];
                        byte r = row[pixelOffset + 2];

                        int gray = (r + g + b) / 3;
                        bool isBlack = gray < 128;

                        if (isBlack)
                        {
                            int byteIndex = (y * bytesPerRow) + (x / 8);
                            int bitIndex = 7 - (x % 8);
                            result[byteIndex] |= (byte)(1 << bitIndex);
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return result;
        }

        private void SendEscPosCommandsToStream(Stream stream, byte[] imageData, int width, int height)
        {
            // Initialize printer
            stream.Write(new byte[] { 0x1B, 0x40 }, 0, 2);

            // Set line spacing to 0
            stream.Write(new byte[] { 0x1B, 0x33, 0x00 }, 0, 3);

            // Center alignment
            stream.Write(new byte[] { 0x1B, 0x61, 0x01 }, 0, 3);

            int bytesPerRow = (width + 7) / 8;

            // Print image in 24-dot rows
            for (int row = 0; row < height; row += 24)
            {
                int rowsToPrint = Math.Min(24, height - row);

                // ESC * m nL nH - Select bit-image mode
                byte[] command = new byte[]
                {
                    0x1B, 0x2A, 0x21,
                    (byte)(bytesPerRow & 0xFF),
                    (byte)((bytesPerRow >> 8) & 0xFF)
                };
                stream.Write(command, 0, command.Length);

                // Write image data
                byte[] rowData = new byte[bytesPerRow * rowsToPrint];
                for (int r = 0; r < rowsToPrint; r++)
                {
                    Array.Copy(imageData, (row + r) * bytesPerRow, rowData, r * bytesPerRow, bytesPerRow);
                }
                stream.Write(rowData, 0, rowData.Length);

                // Line feed
                stream.Write(new byte[] { 0x0A }, 0, 1);
            }

            // Reset line spacing
            stream.Write(new byte[] { 0x1B, 0x32 }, 0, 2);

            // Feed 3 lines
            stream.Write(new byte[] { 0x1B, 0x64, 0x03 }, 0, 3);

            // No cut command - supplies are perforated

            stream.Flush();
        }

        public bool TestPrinterConnection()
        {
            try
            {
                switch (_mode)
                {
                    case PrinterMode.SerialPort:
                        return TestSerialPort();
                    case PrinterMode.UsbPort:
                        return TestUsbPort();
                    case PrinterMode.WindowsPrinter:
                        return TestWindowsPrinter();
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TestSerialPort()
        {
            using (var port = new SerialPort(_settings.Port, 9600, Parity.None, 8, StopBits.One))
            {
                port.Open();
                port.Write(new byte[] { 0x10, 0x04, 0x01 }, 0, 3);
                port.ReadTimeout = 1000;
                
                try
                {
                    byte[] response = new byte[1];
                    port.Read(response, 0, 1);
                    return true;
                }
                catch (TimeoutException)
                {
                    return true;
                }
            }
        }

        private bool TestUsbPort()
        {
            // USB/LPT ports with Windows drivers should test via Windows printer API
            // Raw port access (\\.\USB001) doesn't work for printer queue ports
            return TestWindowsPrinter();
        }

        private bool TestWindowsPrinter()
        {
            string printerName = _settings.Port;
            
            // For USB/LPT ports, look up the actual printer name
            if (_mode == PrinterMode.UsbPort)
            {
                printerName = GetPrinterNameFromPort(_settings.Port) ?? "";
                if (string.IsNullOrEmpty(printerName))
                {
                    return false;
                }
            }

            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                if (printer.Equals(printerName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public void FeedPaper(int mm)
        {
            // At 203 DPI, 1mm = 203/25.4 ≈ 8 dots
            int dots = (int)Math.Round(mm * 203.0 / 25.4);

            switch (_mode)
            {
                case PrinterMode.SerialPort:
                    using (var port = new SerialPort(_settings.Port, 9600, Parity.None, 8, StopBits.One))
                    {
                        port.Open();
                        port.WriteTimeout = 5000;
                        // ESC J n - Feed n dots forward
                        port.Write(new byte[] { 0x1B, 0x4A, (byte)Math.Min(dots, 255) }, 0, 3);
                        port.Close();
                    }
                    break;
                case PrinterMode.UsbPort:
                case PrinterMode.WindowsPrinter:
                    string? printerName = _settings.Port;
                    if (_mode == PrinterMode.UsbPort)
                        printerName = GetPrinterNameFromPort(_settings.Port);
                    if (string.IsNullOrEmpty(printerName))
                        throw new Exception($"No printer found on {_settings.Port}");
                    // Print a blank page sized to the desired feed distance
                    int heightHundredths = (int)Math.Round(mm / 25.4 * 100);
                    using (var printDoc = new PrintDocument())
                    {
                        printDoc.PrinterSettings.PrinterName = printerName;
                        printDoc.DefaultPageSettings.PaperSize = new PaperSize("Feed", 100, Math.Max(heightHundredths, 1));
                        printDoc.PrintPage += (sender, e) => { e.HasMorePages = false; };
                        printDoc.Print();
                    }
                    break;
            }
        }

        public string GetPrinterStatus()
        {
            string modeStr = _mode.ToString();
            if (TestPrinterConnection())
            {
                return $"Printer ({modeStr}) on {_settings.Port}: Connected";
            }
            return $"Printer ({modeStr}) on {_settings.Port}: Not connected";
        }
    }
}
