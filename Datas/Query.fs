module Datas.Query
open System.Linq
open FSharp.Linq
open LinqToDB
open System
open Microsoft.Data.SqlClient


//-----------------------------------------------------------------
type IDatabase =
    abstract member Connection: string
    abstract member Credential: SqlCredential

type Database(conn: string, cred: SqlCredential) =
   interface IDatabase with
      member this.Connection  = conn
      member this.Credential  = cred

type Connection(db:IDatabase)=
    inherit Data.DataConnection(
        LinqToDB.Data.DataConnection.GetDataProvider(
            "SqlServer",
            db.Connection),
        new SqlConnection(
            db.Connection,
            Credential = db.Credential))

    member o.Users  = base.GetTable<User.User>()
    member o.Conns  = base.GetTable<User.Connection.Connection>() // .LoadWith(fun x -> upcast x.User)
    member o.Procs  = base.GetTable<Proc>()
    member o.Tasks  = base.GetTable<Tasks.Task>()       // .LoadWith(fun x -> upcast x.Ops)
    member o.Ops    = base.GetTable<Op>()        // .LoadWith(fun x -> upcast x.User)
    member o.Events = base.GetTable<Event>()            // .LoadWith(fun x -> upcast x.User)
    member o.Nodes  = base.GetTable<Tasks.Node>()
    member o.Links  = base.GetTable<Tasks.Link>()

    static member New db =
        try
            new Connection(db)
        with
        | :? LinqToDBException as e when e.Message.Contains "is not defined" ->
                pair "Configuration" "Connection string"
             |> e.Message.Replace
             |> pairf e
             |> LinqToDBException
             |> raise
        | :? Collections.Generic.KeyNotFoundException as e ->
                pair "Wrong SQL Provider" e
             |> LinqToDBException
             |> raise
        | _ ->  reraise()


//-----------------------------------------------------------------
module Tasks =
    let nodes db p =
        use db = Connection.New db
        let xs = db.Nodes
                    .LoadWith(fun x -> x.Proc)
                    .LoadWith(fun x -> x.Task)
                    .Where(fun x -> x.proc = p)
                    // .LoadWith(fun x -> upcast x.Links)
        xs.LeftJoin(
            db.GetTable<Tasks.Link>(),
            (fun n l -> n.task = l.destination || n.task = l.source),            
            (fun n l -> n,l))
        |> List.seq
        |> List.groupf
        |> List.maps (List.snd >> List.ary)
        |> List.map (fun (n,ls) -> {n with Links = ls})

    let tasks db t =
        use db = Connection.New db
        let xs = db.Ops
                    .Where(fun x -> x.task = t)
        xs
        |> List.seq

    let image db t i =
        use db = Connection.New db
        let xs = db.Ops
                    .Where(fun x -> x.task = t && x.index = i)
                    .Select(fun x -> x.image)
        xs
        |> List.seq
        |> Seq.exactlyOne
        
        //let ops = db.Ops.LoadWith(fun x -> x.User)
        //execute <| query {
        //    for x in ops do
        //    where (x.proc = p)}

    let taskSummary db t =
        use db = Connection.New db
        let xs = db.Tasks
                    .Where(fun x -> x.id = t)
                    .Select(fun o -> o.name, o.id)
                    .FirstOrDefault()
        xs

    let allTasks db =
        use db = Connection.New db
        db.Tasks
            .Select(fun x -> x.id, x.name)
        |> List.seq

module Processes =

    let allProcesses db =
        use db = Connection.New db
        db.Procs
            .Select(fun x -> x.id, x.name)
        |> List.seq
