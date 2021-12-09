// Learn more about F# at http://fsharp.org

open System
open Datas
open Datas.Types
open Datas.Types.Tasks
open Datas.Query
open System.Xml
open System.Xml.Linq
open Export

let conn1 = @"Server=tcp:officeautomata-data.database.windows.net,1433; Initial Catalog=officeAutomata-Test-Server; MultipleActiveResultSets=False; Encrypt=True; TrustServerCertificate=False; Connection Timeout=120;"
let conn = @"Data Source=.\SQLEXPRESS;Initial Catalog=OA_Database;Persist Security Info=False;MultipleActiveResultSets=False;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True"

let credential = (new System.Net.NetworkCredential("", "officeautomat@12")).SecurePassword;
credential.MakeReadOnly();
let livec = new Microsoft.Data.SqlClient.SqlCredential("Test_User", credential);
let db = Database(conn,livec)
   
let task = {
    id = 1;
    proc = Nullable 5;
    name = "task_name";
    kind = Kind.Activity;
    from = "from";
    into = "into";
    category = "category";
    first = DateTime.Now;
    recent = DateTime.Now;
    total = 5L;
    average = 5L;
    potential = 5.0;
    frequency = "";
    steps = 5.0;
    confidence = 5.0;
    volume = 1;
    variants = 1;
    Users=20;
    Apps=10;
    Depts="";
    Cost=10.0;
    Roi=10.0;
    Ops=Array.empty
}

type System.Xml.Linq.XElement with
    member x.ToXmlElement() = 
        let doc = new XmlDocument()
        doc.Load(x.CreateReader())
        doc.DocumentElement
    member x.InnerXml() =
        let reader = x.CreateReader()
        let t1 = reader.MoveToContent()
        reader.ReadInnerXml()

let test () =
    try
        let xmlDocument = new XmlDocument()
        let xDeclaration = new XDeclaration("1.0", "utf-8", "yes");
        let xDoc = new XDocument(xDeclaration);
        let ret = Tasks.nodes db 2
                    |> Export.BPMN.convert
                    //|> Seq.map (fun x -> xDoc.Add(x))// x.InnerXml())

        xDoc.Add(ret)
        xmlDocument.Load(xDoc.CreateReader())
        xmlDocument.InnerXml |> printfn "%A" 
        xmlDocument.Save(@"C:\Users\Jeremiah\Desktop\test.bpmn")
        
    with exn -> printfn "%A" exn

let test2 () =
    try
        let ret = Tasks.tasks db 2
        //Export.Excel.excel (@"C:\Users\Jeremiah\Desktop\test1.xlsx") ret
        //Export.Word.word (@"C:\Users\Jeremiah\Desktop\test2.docx") ret
        Export.PDF.pdf (@"C:\Users\Jeremiah\Desktop\test2.pdf") "test" 1 ret
        
    with exn -> printfn "%A" exn

[<EntryPoint>]
let main argv =
    printfn "Ready"
    test()
    //let testData = [{id = 1; proc = 2; task = 3; }]
    Console.ReadLine() |> ignore    
    0