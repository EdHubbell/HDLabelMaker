# Label Templates

Place your BMP label files in this folder. They will be automatically discovered by the application.

## BMP File Specifications

### Required Format:
- **Resolution:** 203 DPI (dots per inch)
- **Dimensions:** 609 x 406 pixels (3" x 2" at 203 DPI)
- **Color Depth:** 1-bit monochrome (black/white only) or 24-bit RGB
- **Format:** Windows BMP (uncompressed)

### Important Notes:
1. The Star TSP100 prints in black and white only - any color will be converted to grayscale
2. Keep the file size reasonable (under 500KB recommended)
3. Use descriptive filenames (e.g., `7day_return_warning.bmp`)
4. The application will automatically detect new BMP files when you click "Refresh Labels"

### Creating BMP Files:

#### Option 1: Using an Image Editor
- Create a new image: 609 x 406 pixels at 203 DPI
- Design in black and white
- Save as BMP (Windows format, 24-bit or 1-bit)

#### Option 2: Using Another LLM
Provide these exact specifications:
```
Format: BMP (Windows Bitmap)
Dimensions: 609 x 406 pixels
Resolution: 203 DPI
Color Mode: Black and White (1-bit) or Grayscale
Size: 3 inches wide x 2 inches high
Purpose: Thermal transfer label for Star TSP100 printer
Content: [Describe your warning text and layout]
```

### Naming Convention:
Use underscores or spaces in filenames. The application will convert them to readable names:
- `7day_return_warning.bmp` → "7 Day Return Warning"
- `electronics_limited.bmp` → "Electronics Limited"

### Example Labels Needed:
1. **7-Day Return Warning** - General limited return policy
2. **Electronics** - Electronics-specific warning
3. **Tools** - Tools and hardware warning
4. **Appliances** - Appliance-specific warning  
5. **Clearance** - Clearance item warning

After adding BMP files to this folder, click "Refresh Labels" in the application to see them.
