using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace HDLabelMaker.Models
{
    [Serializable]
    public class AppConfiguration
    {
        [XmlElement("PrinterSettings")]
        public PrinterSettings PrinterSettings { get; set; } = new PrinterSettings();

        [XmlArray("ProductAssociations")]
        [XmlArrayItem("Association")]
        public List<ProductAssociation> ProductAssociations { get; set; } = new List<ProductAssociation>();

        [XmlArray("RecentProducts")]
        [XmlArrayItem("Product")]
        public List<string> RecentProducts { get; set; } = new List<string>();
    }

    [Serializable]
    public class PrinterSettings
    {
        [XmlElement("Port")]
        public string Port { get; set; } = "USB001";

        [XmlElement("DPI")]
        public int DPI { get; set; } = 203;

        [XmlElement("LabelWidthInches")]
        public double LabelWidthInches { get; set; } = 3.0;

        [XmlElement("LabelHeightInches")]
        public double LabelHeightInches { get; set; } = 2.0;
    }

    [Serializable]
    public class ProductAssociation
    {
        [XmlAttribute("Sku")]
        public string Sku { get; set; } = "";

        [XmlAttribute("Barcode")]
        public string Barcode { get; set; } = "";

        [XmlAttribute("ProductName")]
        public string ProductName { get; set; } = "";

        [XmlAttribute("LabelFileName")]
        public string LabelFileName { get; set; } = "";

        [XmlAttribute("DefaultCount")]
        public int DefaultCount { get; set; } = 1;

        [XmlAttribute("LastUsed")]
        public DateTime LastUsed { get; set; } = DateTime.MinValue;
    }

    [Serializable]
    public class LabelTemplate
    {
        [XmlAttribute("FileName")]
        public string FileName { get; set; } = "";

        [XmlAttribute("DisplayName")]
        public string DisplayName { get; set; } = "";

        [XmlAttribute("Width")]
        public int Width { get; set; } = 609;

        [XmlAttribute("Height")]
        public int Height { get; set; } = 406;

        [XmlAttribute("FullPath")]
        public string FullPath { get; set; } = "";

        [XmlIgnore]
        public bool IsValid => System.IO.File.Exists(FullPath);
    }
}
