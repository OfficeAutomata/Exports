module Export.Excel
open System
open System.IO
open OfficeOpenXml
open Datas.Shared
open Datas.Types

//Ref: remove Op type
//      columns
//      description attribute
//Note: 0 based worksheets for .NET Core

let excel file ops =
    let log  = NLog.LogManager.GetLogger("Export.Excel")
    let package =
        log.Info ("Export Excel path: " + file)
        if  File.Exists file then File.Delete file
        new ExcelPackage(FileInfo file)
    ops
    |> List.group Op.flow
    |> List.sortf
    |> List.iter (fun (w, ops) ->
        let ws = sprintf "Workflow %d " w |> package.Workbook.Worksheets.Add
        ws.Column(2).Style.Numberformat.Format  <- "yyyy-mm-dd h:mm"
        let columns = ["id";"time";"doc";"app";"kind";"value1";"cell1";"cell2";"cell3";"cell4";"cell5";"appFrame1";"location";"image"]
        let props =
            let name (x:Reflection.PropertyInfo) = x.Name
            (typeof<Op>).GetProperties()
         |> Array.filter (name >> List.has columns)
         |> Seq.cast
         |> Seq.toArray
        ws.Cells.LoadFromCollection(
            ops,
            true,
            Table.TableStyles.Medium1,
            Reflection.BindingFlags.Default,
            props) |> ignore
        ws.Cells.AutoFitColumns())
    package.Save()