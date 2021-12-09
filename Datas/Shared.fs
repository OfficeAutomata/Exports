[<AutoOpen>]
module Datas.Shared

module Time =
    let hours (x:Time) = x.TotalHours
    let mins  (x:Time) = x.TotalMinutes
    let ticks (x:Time) = x.Ticks
    let print (x:Time) = x.ToString "g"
    module From =
        let seconds = Time.FromSeconds
        let minutes = Time.FromMinutes
        let hours   = Time.FromHours

module Date =
    let date  (x:Date) = x.Date
    let ticks (x:Date) = x.Ticks
    let print (f:string) (x:Date) = x.ToString f
    let create x = System.DateTime x

module Event =
    let count (x:Event) = x.count
    let app   (x:Event) = x.app
    let doc   (x:Event) = x.doc
    let time  (x:Event) = x.time
    let user  (x:Event) = x.user
    let kind  (x:Event) = x.kind

module Op =
    let index (x:Op) = x.index
    let flow  (x:Op) = x.flow
    let app   (x:Op) = x.app
    let time  (x:Op) = x.time
    let user  (x:Op) = x.user
    // let links (x:Operation) = x.links

module Proc =
    let id       (x:Proc) = x.id
    let name     (x:Proc) = x.name
    let category (x:Proc) = x.category
    let apps     (x:Proc) = x.Apps
    let users    (x:Proc) = x.Users
    let depts    (x:Proc) = x.Depts
    let cost     (x:Proc) = x.Cost

module Tasks =
    let id       (x:Tasks.Task) = x.id
    let name     (x:Tasks.Task) = x.name
    let category (x:Tasks.Task) = x.category
    let apps     (x:Tasks.Task) = x.Apps
    let users    (x:Tasks.Task) = x.Users
    let depts    (x:Tasks.Task) = x.Depts
    let cost     (x:Tasks.Task) = x.Cost
    let link i s d k = {Tasks.Link.id = i; Tasks.Link.destination = d; Tasks.Link.source = s; Tasks.Link.kind = k}

module Node =
    let id (x:Tasks.Node) = x.task
    let depts (x:Tasks.Node) = x.Task.Depts

module User =
    let toHourly (u:User.User) =
        match u.hourly, u.monthly with
        | x,0 -> float x
        | 0,x -> float x / 160.
        | x,y -> failwithf "Wrong Salary: %i,%i" x y


module Points =
    open Points
    let scatter (d,u,c) v : Scatter = {dept = d; user = u; label = c; value = v; tooltip = None }
    let scatterex k v d r : Scatter = {scatter k v with tooltip = Some {rate = r; duration = d}}
    let row (d,u) v       = {dept = d; user = u; value = v;}
    let app a u y v : App = {user = u; app = a; value = v; y = y}
    let month (t,y,m) v   = {group = t; year = y; month = m; value = v;}
    let gantt d n f t     = {name = n; from = f; into = t; dept = d}
    let dept d v          = {dept = d; value = v}
    let pie  t v          = {value = v; title = t}
    let link (f,t) w      = {from=f; into=t; weight=w}

    module Scatter =
        let dept  (x:Scatter) = x.dept
        let user  (x:Scatter) = x.user
        let label (x:Scatter) = x.label
        let value (x:Scatter) = x.value

    module App =
        let user (x:App) = x.user

    module Row =
        let dept  (x:Row) = x.dept
        let user  (x:Row) = x.user
        let value (x:Row) = x.value

    module Dept =
        let dept (x:Dept) = x.dept

    module Gantt =
        let dept (x:Gantt) = x.dept
        let name (x:Gantt) = x.name
        let from (x:Gantt) = x.from
        let into (x:Gantt) = x.into

    module Pie =
        let value (x:Pie) = x.value
        let title (x:Pie) = x.title


module Unsafe =
    open System
    open Microsoft.FSharp

    // Original: https://stackoverflow.com/a/9009623/2346621
    let rec initializer (x:Type) =
        if   x.IsValueType then
               Activator.CreateInstance x
        elif x = typeof<string> then
               box String.Empty
        elif Reflection.FSharpType.IsRecord x then
               let ctors = x.GetConstructors() |> Array.pick Some
               ctors.GetParameters()
            |> Array.map (fun p -> initializer p.ParameterType)
            |> ctors.Invoke
        elif Reflection.FSharpType.IsTuple x then
               x
            |> Reflection.FSharpType.GetTupleElements
            |> Array.map initializer
            |> pairf x
            |> Reflection.FSharpValue.MakeTuple
        elif Reflection.FSharpType.IsUnion x then
               x
            |> Reflection.FSharpType.GetUnionCases
            |> Array.head
            |> pairf [||]
            |> Reflection.FSharpValue.MakeUnion
        elif x.IsArray then
               x.GetElementType()
            |> pairf 0
            |> Array.CreateInstance
            |> box
        else null

    let init<'a> = initializer typeof<'a> :?> 'a


//--------------------------------------------------------------------------------
// Note: Creation and Subscription should be separated to:
//       - Save mutable reference in OnActivate to call Dispose in OnDeactivate
//       - Wrap in Command instead of Result (otherwise would need to save IDisposable reference from Subscription call into Result)
// Note: Is needed to substitute different implementations on Admin, Demo switches
[<AllowNullLiteral>]
type IServer =
    inherit System.IDisposable
    abstract member Connect :unit   -> unit // This dedicated method is to wrap into Command otherwise would connect right from factory
    abstract member Send    :string -> int seq -> unit