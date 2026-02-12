using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace HDLabelMaker.Services
{
    public class LabelGenerator
    {
        public static void Generate7DayReturnLabel(string outputPath)
        {
            int width = 609;
            int height = 406;

            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                // White background
                graphics.Clear(Color.White);

                var sf = new StringFormat { Alignment = StringAlignment.Center };

                // Left side - Large "7" with "DAYS" centered beneath
                int leftZoneWidth = 200;
                using (var bigFont = new Font("Arial Black", 140, FontStyle.Bold))
                using (var daysFont = new Font("Arial Black", 36, FontStyle.Bold))
                {
                    var sevenSize = graphics.MeasureString("7", bigFont);
                    float sevenX = (leftZoneWidth - sevenSize.Width) / 2;
                    graphics.DrawString("7", bigFont, Brushes.Black, sevenX, -10);

                    float daysY = sevenSize.Height - 30;
                    graphics.DrawString("DAYS", daysFont, Brushes.Black,
                        new RectangleF(0, daysY, leftZoneWidth, 50), sf);
                }

                // Right side - Warning text (no lines or boxes)
                using (var titleFont = new Font("Arial", 28, FontStyle.Bold))
                using (var warningFont = new Font("Arial", 18, FontStyle.Bold))
                using (var detailFont = new Font("Arial", 14, FontStyle.Regular))
                {
                    int rightX = 220;
                    int currentY = 40;

                    graphics.DrawString("LIMITED RETURN", titleFont, Brushes.Black, rightX, currentY);
                    currentY += 45;

                    graphics.DrawString("WINDOW", titleFont, Brushes.Black, rightX, currentY);
                    currentY += 60;

                    graphics.DrawString("DO NOT OPEN", warningFont, Brushes.Black, rightX, currentY);
                    currentY += 30;
                    graphics.DrawString("Unless you intend", detailFont, Brushes.Black, rightX, currentY);
                    currentY += 22;
                    graphics.DrawString("to keep this item", detailFont, Brushes.Black, rightX, currentY);
                    currentY += 40;

                    graphics.DrawString("30-day return policy", detailFont, Brushes.Black, rightX, currentY);
                    currentY += 25;
                    graphics.DrawString("DOES NOT APPLY", new Font("Arial", 16, FontStyle.Bold), Brushes.Black, rightX, currentY);
                    currentY += 35;
                    graphics.DrawString("See associate for complete", detailFont, Brushes.Black, rightX, currentY);
                    currentY += 22;
                    graphics.DrawString("return policy details", detailFont, Brushes.Black, rightX, currentY);
                }

                // Save as 1-bit monochrome BMP
                SaveAsMonochromeBmp(bitmap, outputPath);
            }
        }

        private static void SaveAsMonochromeBmp(Bitmap source, string outputPath)
        {
            // Convert to 1-bit monochrome
            int width = source.Width;
            int height = source.Height;
            int stride = (width + 7) / 8;
            byte[] imageData = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = source.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    bool isBlack = gray < 128;

                    if (isBlack)
                    {
                        int byteIndex = (y * stride) + (x / 8);
                        int bitIndex = 7 - (x % 8);
                        imageData[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }
            }

            // Write BMP file header
            using (var fs = new FileStream(outputPath, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                // BMP Header (14 bytes)
                writer.Write((byte)'B');
                writer.Write((byte)'M');
                writer.Write(14 + 40 + 8 + imageData.Length); // File size
                writer.Write(0); // Reserved
                writer.Write(14 + 40 + 8); // Offset to pixel data

                // DIB Header (BITMAPINFOHEADER - 40 bytes)
                writer.Write(40); // Header size
                writer.Write(width);
                writer.Write(height);
                writer.Write((short)1); // Planes
                writer.Write((short)1); // Bits per pixel (1 = monochrome)
                writer.Write(0); // Compression (0 = none)
                writer.Write(imageData.Length);
                writer.Write(203); // X pixels per meter
                writer.Write(203); // Y pixels per meter
                writer.Write(2); // Colors in color table
                writer.Write(0); // Important colors

                // Color table: index 0 = White (default bits), index 1 = Black (set bits)
                writer.Write(0x00FFFFFF); // Index 0: White
                writer.Write(0x00000000); // Index 1: Black

                // Pixel data (bottom-up)
                for (int y = height - 1; y >= 0; y--)
                {
                    byte[] row = new byte[stride];
                    Array.Copy(imageData, y * stride, row, 0, stride);
                    writer.Write(row);

                    // Pad to 4-byte boundary
                    int padding = (4 - (stride % 4)) % 4;
                    for (int p = 0; p < padding; p++)
                        writer.Write((byte)0);
                }
            }
        }

        public static void GenerateSampleLabels(string labelsDirectory)
        {
            if (!Directory.Exists(labelsDirectory))
            {
                Directory.CreateDirectory(labelsDirectory);
            }

            // Copy bundled resource BMPs to the Labels folder
            var resourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Labels");
            if (Directory.Exists(resourceDir))
            {
                foreach (var srcFile in Directory.GetFiles(resourceDir, "*.bmp"))
                {
                    var destFile = Path.Combine(labelsDirectory, Path.GetFileName(srcFile));
                    File.Copy(srcFile, destFile, overwrite: true);
                }
            }

            // Also generate the programmatic sample
            Generate7DayReturnLabel(Path.Combine(labelsDirectory, "7day_return_warning.bmp"));
        }
    }
}
