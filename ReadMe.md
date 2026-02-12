# HDLabelMaker - Home Depot Label Printing Application

A Windows desktop application for printing 3" x 2" warning labels on Star TSP100 thermal transfer printers at Home Depot stores.

This is a bullshit little app I made to print labels because Home Depot refused return an unopened generator 11 days after purchase. 

At last count, there were 5 different policies. It's a lot for a consumer to keep up with. Store credit would have been a 'both sides win' way to 
handle this.

Anyway, if you need a generator, there's one in my garage. Unopened. 

## Features

- **Product Search**: Search by SKU, barcode, or product name
- **Label Templates**: Auto-discovers BMP label files from the Labels folder
- **Product Associations**: Link products to specific labels with default print counts
- **ESC/POS Printing**: Direct communication with Star TSP100 printers
- **XML Configuration**: All settings stored in XML format
- **Recent History**: Quick access to recently printed products

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime (or SDK for development)
- Star TSP100 series thermal printer
- USB or Serial connection to printer

## Setup Instructions

### 1. Configure the Printer

1. Install the Star TSP100 printer drivers from [Star Micronics website](https://starmicronics.com)
2. Open the **Configuration Utility TSP100**
3. Change emulation to **ESC/POS Mode**
4. Note the printer port (e.g., `USB001`, `COM3`)

### 2. Create Label Templates

1. Navigate to the `Resources/Labels/` folder
2. Add BMP files with the following specifications:
   - **Dimensions**: 609 x 406 pixels (3" x 2" at 203 DPI)
   - **Format**: Windows BMP (uncompressed)
   - **Color**: Black and white or 24-bit RGB

See `Resources/Labels/README.md` for detailed specifications.

### 3. Configure Product Associations

1. Launch the application
2. Click **"Manage Associations"**
3. Add associations linking SKUs/barcodes to label templates
4. Set default print counts for each product

### 4. Printer Configuration

The application uses `USB001` by default. To change the port:

1. Open the application
2. The config file will be created at:
   `%AppData%\HDLabelMaker\config.xml`
3. Edit the `<Port>` element to match your printer port

Example `config.xml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<AppConfiguration>
  <PrinterSettings>
    <Port>USB001</Port>
    <DPI>203</DPI>
    <LabelWidthInches>3</LabelWidthInches>
    <LabelHeightInches>2</LabelHeightInches>
  </PrinterSettings>
  <ProductAssociations>
    <Association Sku="123456" Barcode="012345678901" 
                 ProductName="Power Drill" 
                 LabelFileName="7day_return_warning.bmp" 
                 DefaultCount="1" />
  </ProductAssociations>
</AppConfiguration>
```

## Building from Source

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Build Commands
```bash
# Restore packages
dotnet restore

# Build debug version
dotnet build

# Build release version
dotnet build --configuration Release

# Publish single-file executable
dotnet publish --configuration Release --self-contained --runtime win-x64
```

## Usage

1. **Search for Product**: Enter SKU, barcode, or product name
2. **Select Label**: Choose from available label templates
3. **Set Count**: Adjust number of labels to print
4. **Print**: Click the orange **PRINT LABEL** button

## Architecture

```
HDLabelMaker/
├── Models/           # Data models (LabelTemplate, ProductAssociation, Config)
├── Services/         # Business logic (ConfigService, LabelDiscoveryService, PrintService)
├── ViewModels/       # MVVM view models (MainViewModel, RelayCommand)
├── Views/            # WPF windows and dialogs
├── Resources/
│   └── Labels/       # BMP label template files
└── ReadMe.md         # This file
```

## Troubleshooting

### Printer Not Responding
1. Check printer power and connection
2. Verify correct port in config.xml
3. Ensure printer is in ESC/POS mode
4. Check Windows Device Manager for the correct port name

### Labels Not Found
1. Verify BMP files are in `Resources/Labels/` folder
2. Check BMP dimensions are exactly 609 x 406 pixels
3. Click **"Refresh Labels"** in the application

### Print Quality Issues
1. Clean the print head
2. Check thermal paper is loaded correctly
3. Ensure BMP files are high contrast (black/white)

## Dependencies

- .NET 8.0
- System.IO.Ports (for serial communication)
- System.Drawing.Common (for BMP processing)
- WPF (Windows Presentation Foundation)

## License

Internal use only - Home Depot

## Support

For technical issues or feature requests, contact the development team.
