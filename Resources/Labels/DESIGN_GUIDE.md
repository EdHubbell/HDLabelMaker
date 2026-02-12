# First Label Design: 7-Day Return Warning

## Label Created Successfully!

✅ **File:** `Resources/Labels/7day_return_warning.bmp`
✅ **Size:** 32KB
✅ **Dimensions:** 609 x 406 pixels (3" x 2" at 203 DPI)
✅ **Format:** 1-bit monochrome BMP

---

## Design Layout

```
+----------------------------------------------------------+
|  +------+  |                                             |
|  |  7   |  |  LIMITED RETURN                             |
|  |      |  |  WINDOW                                     |
|  +------+  |                                             |
|  | DAY  |  |  +-------------------------------------+   |
|  +------+  |  |  DO NOT OPEN                          |   |
|            |  |  Unless you intend                    |   |
|            |  |  to keep this item                    |   |
|            |  +-------------------------------------+   |
|            |                                             |
|            |  30-day return policy                       |
|            |  DOES NOT APPLY                             |
|            |  See associate for complete                 |
|            |  return policy details                      |
|            |                                             |
+----------------------------------------------------------+
```

---

## Font Choices

The label uses fonts similar to Home Depot's branding:

- **Left Side (7 DAY):** Arial Black Bold
  - "7" at 72pt
  - "DAY" at 48pt
  
- **Right Side:** Arial family
  - Headers: Arial Bold 28pt
  - Warning box: Arial Bold 18pt
  - Body text: Arial 14-16pt

Home Depot's actual brand font is a custom design, but **Arial Black** provides a similar bold, industrial look that works well for thermal printing.

---

## Key Design Elements

1. **Bold Border:** 4px black border around entire label
2. **Separator Line:** Vertical line dividing left and right sections
3. **Warning Box:** Highlighted box for "DO NOT OPEN" message
4. **Emphasis:** Key phrases in bold/caps
5. **Bottom Accent:** Thick black line at bottom (represents Home Depot orange in 1-bit)

---

## How to Modify This Design

### Option 1: Edit the Code
Edit `Services/LabelGenerator.cs` and adjust:
- Font sizes (lines with `new Font(...)`)
- Text content (`graphics.DrawString(...)` calls)
- Layout positions (X, Y coordinates)
- Box dimensions and borders

### Option 2: Use External Tools
Create a new BMP using:
- **Adobe Photoshop:** Create 609x406px @ 203 DPI, save as BMP
- **GIMP:** Same specs, export as BMP
- **Paint.NET:** Good free option for Windows
- **Canva:** Online tool, export at correct resolution

### Option 3: Use Another LLM
Give them these specs:
```
Create a 3" x 2" warning label BMP (609x406px at 203 DPI)
Format: 1-bit monochrome
Style: Bold industrial, Home Depot inspired

Layout:
- LEFT: Large "7" with "DAY" below it (Arial Black bold)
- RIGHT: 
  * Header: "LIMITED RETURN WINDOW" (bold)
  * Box: "DO NOT OPEN - Unless you intend to keep this item"
  * Text: "30-day return policy DOES NOT APPLY"
  * Footer: "See associate for complete return policy details"

Colors: Black and white only (thermal printer)
Border: Thick black border around entire label
```

---

## Remaining Label Designs Needed

You mentioned needing 5 total designs. Here are suggestions for the other 4:

### 2. Electronics Warning
```
ELECTRONICS
    
SEALED FOR
YOUR PROTECTION

Opening voids
return eligibility

See associate
for details
```

### 3. Tools Warning
```
POWER TOOLS

SAFETY SEAL
INTACT

Do not open
unless keeping

Defective items
only - No returns
after opening
```

### 4. Appliances Warning
```
APPLIANCES

DELIVERY REQUIRED

Do not remove
from store

Delivery team
will unbox and
inspect on arrival

See associate
for scheduling
```

### 5. Clearance Warning
```
CLEARANCE
    
FINAL SALE

No returns
No exchanges
No exceptions

All sales final
See receipt for
detailed terms
```

---

## Tips for Best Results

1. **High Contrast:** Use pure black (#000000) and white (#FFFFFF) only
2. **Bold Fonts:** Thin fonts may not print well on thermal paper
3. **Test Print:** Always test on actual printer before production use
4. **Font Size:** Minimum 12pt recommended for readability
5. **Margins:** Keep text at least 10px from edges

---

## Generate More Labels

Click **"Generate Sample"** button in the main app to recreate this label, or modify `LabelGenerator.cs` to create additional designs programmatically.
