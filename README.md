# OfficeAutomata Open Source RPA Exports

This library allows for the creation of different RPA exports. 

## Frameworks

- Windows
- HTML (Chromium)
- Office (Excel, Word, etc)
- Java
- PDF (X/Y Coord, Sections)
- Greenscreen (AS400, X/Y Coord, Line Coord)

## Format

Data from tracking does not use XML as it's put into a common format for the AI. Hence, field value change based on the framework being tracked.

Event Format: 
```
    {
      "WorkflowID": 0,
      "Index": 647,
      "UI_Element": "Six",
      "AutoID_Control": "num6Button;button",
      "UI_Element_Pattern": "InvokePattern;ScrollItemPattern",
      "Pattern_Value": "",
      "X_Path": "Desktop 1;pane/Calculator;window/Calculator;window/;group/NumberPad;group/",
      "Type": "Clicked",
      "Value": "6",
      "Time": "2020-04-22T06:52:35.1530000",
      "AppFile": "",
      "AppFrame_1": "",
      "User": "Jeremiah",
      "Location": "C:\\Program Files\\WindowsApps\\Microsoft.WindowsCalculator_10.1910.0.0_x64__8wekyb3d8bbwe\\Calculator.exe",
      "Application": "Calculator",
      "Data": "",
      "API": "UIAutomation"
    }
```