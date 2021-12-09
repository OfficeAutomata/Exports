namespace Exports

module RobotFramework =
    open System
    open Datas
    open Utils
    open System.Text.RegularExpressions

    let HasCreditCardNumber (input: string) =
        let regex = @"\b4[0-9]{12}(?:[0-9]{3})?\b|\b5[1-5][0-9]{14}\b|\b3[47][0-9]{13}\b|\b3(?:0[0-5]|[68][0-9])[0-9]{11}\b|\b6(?:011|5[0-9]{2})[0-9]{12}\b|\b(?:2131|1800|35\d{3})\d{11}\b"
    //        ^(?:4[0-9]{12}(?:[0-9]{3})?          # Visa
    //         |  5[1-5][0-9]{14}                  # MasterCard
    //         |  3[47][0-9]{13}                   # American Express
    //         |  3(?:0[0-5]|[68][0-9])[0-9]{11}   # Diners Club
    //         |  6(?:011|5[0-9]{2})[0-9]{12}      # Discover
    //         |  (?:2131|1800|35\d{3})\d{11}      # JCB
    //        )$
        let matches = Regex.Matches(input, regex)
        if (matches.Count > 0) then true
        else false
    
    let HasCVVNumber (input: string) =
        let regex = @"/^[0-9]{3,4}$/"
        let matches = Regex.Matches(input, regex)
        if (matches.Count > 0) then true
        else false
    
    let HasSSNNumber (input: string) =
        let regex = @"^(?!219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}-(?!00)\d{2}-(?!0{4})\d{4}$"
        let matches = Regex.Matches(input, regex)
        if (matches.Count > 0) then true
        else false
    
    let (|Int|_|) str =
       match System.Int32.TryParse(str) with
       | (true,int) -> Some(int)
       | _ -> None
    
    // create an active pattern
    let (|Bool|_|) str =
       match System.Boolean.TryParse(str) with
       | (true,bool) -> Some(bool)
       | _ -> None
    
    let (|DateTime|_|) str =
       match System.DateTime.TryParse(str) with
       | (true,date) -> Some(date)
       | _ -> None
    
    let (|Double|_|) str =
       match System.Double.TryParse(str) with
       | (true,double) -> Some(double)
       | _ -> None
    
    let (|Decimal|_|) str =
       match System.Decimal.TryParse(str) with
       | (true,decimal) -> Some(decimal)
       | _ -> None
    
    let (|CreditCard|_|) str =
       let res = HasCreditCardNumber str
       if res then Some(res)
       else None       
    
    let (|CVV|_|) str =
       let res = HasCVVNumber str
       if res then Some(res)
       else None
    
    let (|SSN|_|) str =
       let res = HasSSNNumber str
       if res then Some(res)
       else None
    
    let inline data_cleaner (input: string) =
            try
                match input with
                | "" -> ""
                | x when x.StartsWith("$") -> "Currency"
                | x when x.StartsWith("%") -> "Percentage" 
                | x when x.[x.Length-1] = '%' -> "Percentage" 
                | CreditCard x -> "Credit Card"
                | CVV x -> "CVV"
                | Bool x -> "Boolean"
                | Int x -> "Number"
                | DateTime x -> "DateTime"
                | Double x -> "Number"
                | Decimal x -> "Number"
                | SSN x -> "SSN"
                | _ -> "" 
            with
            | _ -> ""
    
    let uriCreate (input:string) =
        let out = ref null
        let chk = Uri.TryCreate(input, UriKind.Absolute, out)
        if chk then out.Value.Host else input
        
    let urlDomain url = 
        try
            match url with
            | "" -> "_.com"
            | x when not (x.StartsWith("http")) ->  uriCreate ("http://" + url)
            | _ -> uriCreate url
        with
        | exn -> printfn "%A" exn
                 url    
    
    let webSelector (pie: Process_IndexedEvent) =
        let cell3 = pie.eventCell_3.Split ';'
                        |> Array.map (fun x -> let s1 = x.Split([|':'|],2)
                                               let s2 = if s1.Length > 1 then s1.[1] else ""
                                               s1.[0],s2)
                        |> Map.ofArray
        let mutable value = pie.eventCell_5
        match cell3 with
        | x when cell3.TryGetValue("data-test-id", &value) -> "data-test-id" + ":" + value.Trim()
        | x when cell3.TryGetValue("id", &value) -> "id" + ":" + value.Trim()
        | x when cell3.TryGetValue("name", &value) -> "name" + ":" + value.Trim()
        | x when cell3.TryGetValue("title", &value) -> "title" + ":" + value.Trim()
        | x when cell3.TryGetValue("class", &value) -> "class" + ":" + value.Trim()
        | _ ->  //printfn "%A" pie.eventCell_3
                "xpath" + ":" + pie.eventCell_5
    
    let appfilechk (input : Process_IndexedEvent)  =
        match input.eventApplication with
        | "chrome" -> urlDomain input.eventAppFile
        | "Chrome OA" -> urlDomain input.eventAppFile
        | "Edge" -> urlDomain input.eventAppFile
        | "Firefox" -> urlDomain input.eventAppFile
        | x when input.eventAppFile = "" -> input.eventApplication
        | _ -> input.eventAppFile
    
    let sep = "\t"
    
    let eventDescription_Text (input : Process_IndexedEvent) =
        let cell = if input.eventCell_1 = "" then input.eventCell_2 else input.eventCell_1
        "Send Keys To Input" + sep + input.eventValue_1 + "\twith_enter=False"
    
    let eventDescription_Enter (input : Process_IndexedEvent) =
        let cell = if input.eventCell_1 = "" then input.eventCell_2 else input.eventCell_1
        "Send Keys To Input" + sep + input.eventValue_1 + "\twith_enter=True"
    
    let eventDescription_Click (input : Process_IndexedEvent) =
        let autoid = input.eventCell_2.[..input.eventCell_2.IndexOf(";")]
        let cell = if input.eventCell_1 = "" then autoid else input.eventCell_1
        "Mouse Click" + sep + "id:" + cell
        //match input.eventCell_1 with
        //| "" -> 
        //| _ -> $"{input.eventType_1} on {cell} in {input.eventAppFile}"
    
    let eventDescription_WebClick (prev: Process_IndexedEvent) (input : Process_IndexedEvent) =
        let type1 = input.eventType_1.Replace("INPUT:","")
        let selector = webSelector input
        //String.Format("{0}\t{1}", "Click Button",selector) |> printfn "%A"
        match input.eventCell_1 with
        | x when input.eventCell_3.EndsWith("selectdrop") ->
                "Click Element" + sep + input.eventCell_2
        | x when input.eventCell_3.Contains("dropdown") ->
                "Click Element" + sep + input.eventCell_2
        | x when input.eventCell_3.Contains("calendar") ->
                "Click Element" + sep + input.eventCell_2
        | x when type1 = "submit" ->
                "Submit Form" + sep + input.eventCell_2
        | x when type1 = "checkbox" ->
                "Select Checkbox" + sep + input.eventCell_2
        | x when type1 = "file" ->
                "Click Button" + sep + selector
        | x when type1 = "button" ->
                "Click Button" + sep + selector
        | x when prev.eventAppFrame_1 <> input.eventAppFrame_1 ->         
                "Go To\t" + input.eventAppFrame_1
        | _ ->  "Click Button" + sep + selector
    
    let eventDescription_WebText (prev: Process_IndexedEvent) (input : Process_IndexedEvent) =
    //| "INPUT:password" -> eventDescription_Text input
    //| "INPUT:text" -> eventDescription_Text input
        let type1 = input.eventType_1.Replace("INPUT:","")
        let selector = webSelector input
        match input.eventCell_1 with
        | x when input.eventCell_3.EndsWith("selectdrop") ->
                "Click Element" + sep + selector
        | x when input.eventCell_3.Contains("dropdown") ->
                "Click Element" + sep + selector
        | _ -> "Input Text" + sep + selector + sep + input.eventValue_1
    
    let eventDescription_Copy (input : Process_IndexedEvent) prefix =
        let cell = if input.eventCell_1 = "" then input.eventCell_2 else input.eventCell_1
        "Copy to clipboard" + sep + "id:" + cell
    
    let eventDescription_General (input : Process_IndexedEvent) prefix =
        let cell = if input.eventCell_1 = "" then input.eventCell_2 else input.eventCell_1
        "Copy to clipboard" + sep + "id:" + cell
    
    let eventDescription_Excel (input : Process_IndexedEvent) =
        let cell = if input.eventCell_1 = "" then input.eventCell_2 else input.eventCell_1
        "Apply" + sep + input.eventType_1 + " in" + cell
    
    let eventDescription_Key (input : Process_IndexedEvent) =
        let cell = if input.eventCell_1 = "" then input.eventCell_2 else input.eventCell_1
        "Send Keys To Input" + sep + input.eventType_1 + "\twith_enter=False"
    
    //if url change in chrome then navigate instead of click
    //if internal then select
    //if Input:Password or type password then 
    //if Input:
    //insert "open application x"
    
    
    let createEventDescription (input0 : Process_IndexedEvent*Process_IndexedEvent) =
        let config = AppDataConfig()
        let prev, input = input0
        let cell2 = 
            if input.eventCell_2.Contains("$") then
                input.eventCell_2.[input.eventCell_2.LastIndexOf("$")+1..]
            else input.eventCell_2
        let cell1 = 
            match input.eventCell_1 with
            | "" -> cell2 
            | x when input.eventCell_1.Length > 50 -> input.eventCell_1.Substring(0,30)
            | x when input.eventCell_1.Contains("$") -> input.eventCell_1.[input.eventCell_1.LastIndexOf("$")+1..]
            | _ -> input.eventCell_1
            //let t1 = pie.eventCell_1.Split(' ') 
            //if t1.Length > 3 then t1 |> fun x -> x.
        //let appfile = appfilechk pie
        let value = try config.Decrypt input.eventValue_1 with _ -> input.eventValue_1
        let input = {input with eventCell_1 = cell1; eventCell_2 = cell2; eventValue_1 = value}
        match input.eventType_1 with
        | x when input.eventContext_4 = "Logic" -> input.eventContext_1
        | "Alt Hotkey" -> eventDescription_Key input
        | "Auto Fill" -> eventDescription_Excel input
        | "Borders" -> eventDescription_Excel input
        | "Cell Color,Cell Tint And Shade,Cell Color Index"
        | "Clear" -> eventDescription_Copy input "in"
        | "Clicked" ->  match input.eventApplication with
                        | "chrome" -> eventDescription_WebClick prev input 
                        | "Chrome OA" -> eventDescription_WebClick prev input
                        | "Edge" -> eventDescription_WebClick prev input
                        | "Firefox" -> eventDescription_WebClick prev input
                        | _ -> eventDescription_Click input
        | "Column Width" -> eventDescription_Excel input
        | "Convert Text to Table" -> eventDescription_Excel input
        | "Copied Cells" -> eventDescription_General input "from"
        | "Copy" -> eventDescription_Copy input "from"
        | "Ctrl Hotkey" -> eventDescription_Key input
        | "Currency" -> eventDescription_Text input
        | "Custom Sort" -> eventDescription_Excel input
        | "Cut" -> eventDescription_Copy input "from"
        | "Cut Cells" -> eventDescription_General input "from"
        | "DateTime" -> eventDescription_Text input
        | "Decimal" -> eventDescription_Text input
        | "Delete" -> eventDescription_Copy input "in"
        | "Delete Column(s)" -> eventDescription_Excel input
        | "Delete Row(s)" -> eventDescription_Excel input
        | "DoubleClicked" -> eventDescription_Click input
        | "Edit Note" -> eventDescription_Text input
        | "Empty" -> eventDescription_Text input
        | "Enter" -> eventDescription_Enter input
        | "Entry" -> eventDescription_Text input
        | "F1" -> eventDescription_Key input
        | "F10" -> eventDescription_Key input
        | "F12" -> eventDescription_Key input
        | "F2" -> eventDescription_Key input
        | "F3" -> eventDescription_Key input
        | "F4" -> eventDescription_Key input
        | "F5" -> eventDescription_Key input
        | "F6" -> eventDescription_Key input
        | "F7" -> eventDescription_Key input
        | "F8" -> eventDescription_Key input
        | "F9" -> eventDescription_Key input
        | "Filter" -> eventDescription_Excel input
        | "Focus" -> eventDescription_Click input
        | "Font Bold" -> eventDescription_Excel input
        | "Font Italic" -> eventDescription_Excel input
        | "Font Name" -> eventDescription_Excel input
        | "Font Size" -> eventDescription_Excel input
        | "Format Cells" -> eventDescription_Excel input
        | "Formula" -> eventDescription_Excel input
        | "Horizontal Alignment" -> eventDescription_Excel input
        | "Hover" -> eventDescription_Click input
        | "INPUT:button" -> eventDescription_WebClick prev input 
        | "INPUT:checkbox" -> eventDescription_WebClick prev input 
        | "INPUT:file" -> eventDescription_WebClick prev input 
        | "INPUT:password" -> eventDescription_WebText prev input
        | "INPUT:submit" -> eventDescription_WebClick prev input 
        | "INPUT:text" -> eventDescription_WebText prev input
        | "INPUT:hidden" -> eventDescription_WebText prev input
        | "INPUT:radio" -> eventDescription_WebClick prev input 
        | "Insert Cells" -> eventDescription_Excel input
        | "Insert Column(s)" -> eventDescription_Excel input
        | "Insert Row(s)" -> eventDescription_Excel input
        | "Insert Rows" -> eventDescription_Excel input
        | "Merge Cells" -> eventDescription_Excel input
        | "Number" -> eventDescription_Text input
        | "Password" -> eventDescription_Text input
        | "Paste" -> eventDescription_Copy input "to"
        | "Paste Special" -> eventDescription_Copy input "to"
        | "Paste Values" -> eventDescription_Copy input "to"
        | "Pivot" -> eventDescription_Excel input
        | "Pivot Table Update" -> eventDescription_Excel input
        | "PivotTable Refresh" -> eventDescription_Excel input
        | "Replace" -> eventDescription_Excel input
        | "Resize Object" -> eventDescription_Click input
        | "Row Height" -> eventDescription_Excel input
        | "Shift Down" -> eventDescription_Excel input
        | "Shift Up" -> eventDescription_Excel input
        | "Sort Ascending" -> eventDescription_Excel input
        | "SSN" -> eventDescription_Text input
        | "Sum" -> eventDescription_Excel input
        | "Tab" -> eventDescription_Key input
        | "Text" -> eventDescription_Text input
        | "Undo" -> eventDescription_Excel input
        | "Values" -> eventDescription_Text input
        | "Workbook Saved" -> eventDescription_Excel input
        | "Worksheet Name" -> eventDescription_Excel input
        | _ -> eventDescription_Text input

    let openApp app =
        "\tOpen application\t" + app

    let openFile file =
        "\tOpen file\t" + file

    let openBrowser url =
        "\tOpen Chrome browser\turl:" + url

    let composeGroup (pieAry: Process_IndexedEvent[]) =
        pieAry
        |> Array.groupBy (fun x -> x.eventApplication )
        |> Array.map (fun (app,pie) -> 
            pie 
            |> Array.sortBy (fun x -> x.eventTime)
            |> Array.map (fun x -> "\t" + x.eventContext_1)
            |> fun x -> match app with
                        | "Excel" -> Array.append [|openApp app; openFile pie.[0].eventAppFile|] x 
                        | "Word" -> Array.append [|openApp app; openFile pie.[0].eventAppFile|] x 
                        | "Adobe" -> Array.append [|openApp app; openFile pie.[0].eventAppFile|] x 
                        | "chrome" -> Array.append [|openBrowser pie.[0].eventAppFile|] x 
                        | "Chrome OA" -> Array.append [|openBrowser pie.[0].eventAppFile|] x 
                        | _ -> Array.append [|openApp app;|] x 
            //|> Array.append [|System.Environment.NewLine|]
            )
        |> Array.concat

    let robotHeader (name: string) =
        [|"*** Settings ***";
        "Documentation" + sep + name;
        "Library" + sep + sep + "RPA.Browser.Selenium";
        "Library" + sep + sep + "RPA.HTTP";
        "Library" + sep + sep + "RPA.Excel.Files";
        "Library" + sep + sep + "RPA.Excel.Application";
        "Library" + sep + sep + "RPA.Desktop.Windows";
        "\n";
        "*** Keywords ***"|]

    let robotFooter (ary: string[]) = 
        ary |> Array.map (fun x -> "\t" + x)
            |> Array.append [|"\n"; "*** Tasks ***"|]

    let composeRobotEvents (pieAry: Process_IndexedEvent[]) =
        pieAry
        |> Array.pairwise
        |> Array.map (fun (x,y) ->  
            let context = createEventDescription (x,y) //|> fun x -> Regex.Replace(x, @"\s+", " ");
            {x with eventContext_1 = context;})
        |> Array.groupBy (fun x -> x.eventContext_2 )
        |> Array.map (fun (key,pie) -> let pie2 = composeGroup pie 
                                       Array.append [|System.Environment.NewLine;key.Trim();|] pie2
                                       ) 
        |> Array.concat
        
    let composeRobot (tid:int) =
        let task = SQL_Task.Sql_TaskSelect_TaskID tid |> Sql_func |> Array.head
        let pie = Sql_PIE_Select_Task tid |> Sql_func
        let head = robotHeader task.Task_Name        
        if pie.IsSome then 
            let ary = pie.Value |> Array.map (fun x -> x.eventContext_2.Trim()) |> Array.distinct
            let foot = robotFooter ary
            composeRobotEvents pie.Value
            |> Array.append head
            |> fun x -> Array.append x foot
            |> String.concat "\n"
        else ""


    //let saveRobot name (text: string[]) =
    //    let dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    //    let name = robotHeader name
    //    let text2 = Array.append name text
    //    File.WriteAllLines (dir + @"\test.robot", text2) |> ignore
    //    //File.WriteAllText (dir + @"\test.robot", text2) |> ignore