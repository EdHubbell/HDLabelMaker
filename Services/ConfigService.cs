using HDLabelMaker.Models;
using System;
using System.IO;
using System.Xml.Serialization;

namespace HDLabelMaker.Services
{
    public class ConfigService
    {
        private readonly string _configDirectory;
        private readonly string _configFilePath;
        private AppConfiguration _cachedConfig;

        public ConfigService()
        {
            _configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HDLabelMaker");
            _configFilePath = Path.Combine(_configDirectory, "config.xml");
            
            EnsureConfigDirectoryExists();
        }

        private void EnsureConfigDirectoryExists()
        {
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }
        }

        public AppConfiguration LoadConfiguration()
        {
            if (_cachedConfig != null)
                return _cachedConfig;

            if (!File.Exists(_configFilePath))
            {
                _cachedConfig = new AppConfiguration();
                SaveConfiguration(_cachedConfig);
                return _cachedConfig;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(AppConfiguration));
                using (var reader = new StreamReader(_configFilePath))
                {
                    _cachedConfig = (AppConfiguration)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
                _cachedConfig = new AppConfiguration();
            }

            return _cachedConfig;
        }

        public void SaveConfiguration(AppConfiguration config)
        {
            try
            {
                EnsureConfigDirectoryExists();
                var serializer = new XmlSerializer(typeof(AppConfiguration));
                using (var writer = new StreamWriter(_configFilePath))
                {
                    serializer.Serialize(writer, config);
                }
                _cachedConfig = config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
                throw;
            }
        }

        public void AddOrUpdateAssociation(ProductAssociation association)
        {
            var config = LoadConfiguration();
            var existing = config.ProductAssociations.Find(
                a => a.Sku.Equals(association.Sku, StringComparison.OrdinalIgnoreCase) ||
                     (!string.IsNullOrEmpty(a.Barcode) && 
                      a.Barcode.Equals(association.Barcode, StringComparison.OrdinalIgnoreCase)));

            if (existing != null)
            {
                config.ProductAssociations.Remove(existing);
            }

            association.LastUsed = DateTime.Now;
            config.ProductAssociations.Add(association);
            SaveConfiguration(config);
        }

        public ProductAssociation FindAssociation(string searchTerm)
        {
            var config = LoadConfiguration();
            return config.ProductAssociations.Find(
                a => a.Sku.Equals(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                     (!string.IsNullOrEmpty(a.Barcode) && 
                      a.Barcode.Equals(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                     (!string.IsNullOrEmpty(a.ProductName) &&
                      a.ProductName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        public void UpdateLastUsed(string sku)
        {
            var config = LoadConfiguration();
            var association = config.ProductAssociations.Find(
                a => a.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
            
            if (association != null)
            {
                association.LastUsed = DateTime.Now;
                SaveConfiguration(config);
            }
        }
    }
}
