using HDLabelMaker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HDLabelMaker.Services
{
    public class LabelDiscoveryService
    {
        private readonly string _labelsDirectory;
        private readonly string _appDirectory;

        public LabelDiscoveryService()
        {
            _appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _labelsDirectory = Path.Combine(_appDirectory, "Labels");
            EnsureLabelsDirectoryExists();
            DeployBundledLabels();
        }

        private void EnsureLabelsDirectoryExists()
        {
            if (!Directory.Exists(_labelsDirectory))
            {
                Directory.CreateDirectory(_labelsDirectory);
            }
        }

        private void DeployBundledLabels()
        {
            var resourceDir = Path.Combine(_appDirectory, "Resources", "Labels");
            if (!Directory.Exists(resourceDir))
                return;

            foreach (var srcFile in Directory.GetFiles(resourceDir, "*.bmp"))
            {
                var destFile = Path.Combine(_labelsDirectory, Path.GetFileName(srcFile));
                if (!File.Exists(destFile) ||
                    File.GetLastWriteTimeUtc(srcFile) > File.GetLastWriteTimeUtc(destFile))
                {
                    File.Copy(srcFile, destFile, overwrite: true);
                }
            }
        }

        public List<LabelTemplate> DiscoverLabels()
        {
            var labels = new List<LabelTemplate>();

            if (!Directory.Exists(_labelsDirectory))
            {
                return labels;
            }

            var bmpFiles = Directory.GetFiles(_labelsDirectory, "*.bmp")
                .OrderBy(f => f)
                .ToList();

            foreach (var filePath in bmpFiles)
            {
                var fileName = Path.GetFileName(filePath);
                var displayName = GenerateDisplayName(fileName);

                var label = new LabelTemplate
                {
                    FileName = fileName,
                    DisplayName = displayName,
                    FullPath = filePath,
                    Width = 609,
                    Height = 406
                };

                labels.Add(label);
            }

            return labels;
        }

        private string GenerateDisplayName(string fileName)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var parts = nameWithoutExtension.Split('_', '-', ' ');
            var capitalizedParts = parts.Select(p => 
                string.IsNullOrEmpty(p) ? p : char.ToUpper(p[0]) + p.Substring(1).ToLower());
            return string.Join(" ", capitalizedParts);
        }

        public string GetLabelsDirectoryPath()
        {
            return _labelsDirectory;
        }

        public LabelTemplate GetLabelByFileName(string fileName)
        {
            var labels = DiscoverLabels();
            return labels.FirstOrDefault(l => 
                l.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        public bool ValidateLabelFile(string filePath, out string errorMessage)
        {
            errorMessage = null;

            if (!File.Exists(filePath))
            {
                errorMessage = "File does not exist.";
                return false;
            }

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var header = new byte[54];
                    fs.Read(header, 0, 54);

                    if (header[0] != 'B' || header[1] != 'M')
                    {
                        errorMessage = "File is not a valid BMP file.";
                        return false;
                    }

                    int width = BitConverter.ToInt32(header, 18);
                    int height = BitConverter.ToInt32(header, 22);
                    short bpp = BitConverter.ToInt16(header, 28);

                    if (width != 609 || height != 406)
                    {
                        errorMessage = $"Invalid dimensions: {width}x{height}. Expected: 609x406 pixels (3\"x2\" at 203 DPI).";
                        return false;
                    }

                    if (bpp != 1 && bpp != 24 && bpp != 32)
                    {
                        errorMessage = $"Unsupported bit depth: {bpp}. Use 1-bit monochrome or 24-bit RGB.";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error reading file: {ex.Message}";
                return false;
            }
        }
    }
}
