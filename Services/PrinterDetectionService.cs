using System.Management;
using System.IO;
using System.IO.Ports;

namespace HDLabelMaker.Services
{
    public class PrinterInfo
    {
        public string Name { get; set; } = "";
        public string PortName { get; set; } = "";
        public string DeviceId { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsStarTsp => Name.Contains("Star", StringComparison.OrdinalIgnoreCase) && 
                                  Name.Contains("TSP", StringComparison.OrdinalIgnoreCase);
    }

    public class PrinterDetectionService
    {
        public static List<PrinterInfo> DetectAvailablePrinters()
        {
            var printers = new List<PrinterInfo>();
            
            try
            {
                // Method 1: Query WMI for installed printers
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
                {
                    foreach (ManagementObject printer in searcher.Get())
                    {
                        var printerInfo = new PrinterInfo
                        {
                            Name = printer["Name"]?.ToString() ?? "Unknown",
                            DeviceId = printer["DeviceID"]?.ToString() ?? "",
                            Status = printer["Status"]?.ToString() ?? "Unknown"
                        };

                        // Try to get port name
                        var portName = printer["PortName"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(portName))
                        {
                            printerInfo.PortName = portName;
                        }

                        printers.Add(printerInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WMI query failed: {ex.Message}");
            }

            // Method 2: Check for available serial ports (common for thermal printers)
            try
            {
                var serialPorts = SerialPort.GetPortNames();
                foreach (var port in serialPorts)
                {
                    // Check if this port is already associated with a printer
                    if (!printers.Any(p => p.PortName.Equals(port, StringComparison.OrdinalIgnoreCase)))
                    {
                        printers.Add(new PrinterInfo
                        {
                            Name = $"Serial Port ({port})",
                            PortName = port,
                            DeviceId = port,
                            Status = "Available"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Serial port detection failed: {ex.Message}");
            }

            return printers.OrderByDescending(p => p.IsStarTsp).ThenBy(p => p.Name).ToList();
        }

        public static PrinterInfo? FindStarTspPrinter()
        {
            var printers = DetectAvailablePrinters();
            return printers.FirstOrDefault(p => p.IsStarTsp);
        }

        public static bool TestPrinterConnection(string portName)
        {
            try
            {
                // Determine port type
                if (portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                {
                    // Serial port
                    using (var port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One))
                    {
                        port.Open();
                        port.WriteTimeout = 1000;
                        port.ReadTimeout = 1000;
                        port.Write(new byte[] { 0x10, 0x04, 0x01 }, 0, 3);
                        return true;
                    }
                }
                else if (portName.StartsWith("USB", StringComparison.OrdinalIgnoreCase) ||
                         portName.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
                {
                    // USB/Parallel port - check if a printer uses this port
                    // Raw port access (\\.\USB001) doesn't work for printer queue ports
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
                    {
                        foreach (ManagementObject printer in searcher.Get())
                        {
                            var printerPort = printer["PortName"]?.ToString() ?? "";
                            if (printerPort.Equals(portName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                else
                {
                    // Windows printer queue - just check if it exists
                    foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                    {
                        if (printer.Equals(portName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
