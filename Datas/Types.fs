[<AutoOpen>]
module Datas.Types
open LinqToDB.Mapping
open System.ComponentModel

type Date = System.DateTime
type Time = System.TimeSpan


module User =
    type Kind =
        | [<MapValue "">]           Undefined = 0
        | [<MapValue "Admin">]      Admin     = 1
        | [<MapValue "User">]       User      = 2
        | [<MapValue "AzureAdmin">] Azure     = 3

    type Permission =
        | [<MapValue "">]      None  = 0
        | [<MapValue "Read">]  Read  = 1
        | [<MapValue "Write">] Write = 2

    [<Table "Users">]
    type User = {
        [<Column "User_ID"; PrimaryKey>] id:string
        [<Column "[User]"; NotNull>] name:string
        [<Column "User_Groups"; NotNull>] dept:string
        [<Column "Title">] title:string
        [<Column "Hourly"; NotNull>] hourly:int
        [<Column "Monthly"; NotNull>] monthly:int
        [<Column "User_Type"; NotNull>] kind:Kind
        [<Column "User_Permission"; NotNull>] permission:Permission }

    let id     (x:User) = x.id
    let name   (x:User) = x.name
    let dept   (x:User) = x.dept
    let empty    :User = {id = null
                          name = null
                          dept = null
                          title = null
                          hourly = -1
                          monthly = -1
                          kind = Kind.Undefined
                          permission = Permission.None}


    module Connection =
        type Status =
            | [<MapValue "Online">]  Online  = 1
            | [<MapValue "Away">]    Away    = 2
            | [<MapValue "Offline">] Offline = 3

        [<Table "Connections">]
        type Connection = {
            [<Column "Status"; NotNull>] status:Status
            [<Column "User"; NotNull>] user:string
            [<Column "Version"; NotNull>] version:string
            [<Column "Time";  NotNull>] time: Date
            [<Column "RAM">] ram: float
            [<Column "CPU">] cpu: float
            [<Column "Disk">] disk: float
            [<Column "Mbps">] mbps: float
            [<Column "NetSpeed">] net: float
            [<Column "Location">] location: string
            [<Column "Latitude">] latitude: float
            [<Column "Longitude">] longitude: float
            [<Association(ThisKey="user", OtherKey="id", CanBeNull=false)>] User:User }

        type Performance = { connection:Connection; count:int } with
            member o.System = (o.connection.cpu + o.connection.ram + o.connection.disk) / 3.
            member o.Network = min (o.connection.mbps / o.connection.net) 1. * 100.

[<Table "IndexedEvents">]
type Event = {
    [<Column "eventIndex"; Identity; PrimaryKey>] index:int
    [<Column "processID">]  proc:int option
    [<Column "workflowID">] flow:int option
    [<Column "eventTime";  NotNull>] time: Date
    [<Column "eventUser"; NotNull>] user: string
    [<Column "eventCount"; NotNull>] count: int
    [<Column "eventApplication"; NotNull>] app: string
    [<Column "eventAppFile"; NotNull>] doc: string
    [<Column "eventType_1"; NotNull>] kind: string
    [<Column "eventValue_0"; NotNull>] valuep: string
    [<Column "eventValue_1"; NotNull>] value:string
    [<Column "eventCell_1"; NotNull>] cell:string
    [<Column "eventAppFrame_1"; NotNull>] frame:string
    [<Column "eventLocation"; NotNull>] location:string
    [<Column "eventContext_2"; NotNull>] context:string
    [<Column "eventAutomation"; NotNull>] automation:int
    [<Column "Duration"; NotNull>] duration:float
    [<Association(ThisKey="user", OtherKey="id", CanBeNull=false)>] User:User.User }


[<Table "Process_IndexedEvents">]
type Op = {
    [<Description "ID";                Column "eventIndex";       PrimaryKey>] index:int
    [<Description "Process ID";        Column "processID";        PrimaryKey>] proc:int
    [<Description "Task ID";           Column "TaskID";           PrimaryKey>] task:int
    [<Description "Workflow ID";       Column "workflowID";       PrimaryKey>] flow:int
    [<Description "Time";              Column "eventTime";        NotNull>]    time:Date
    [<Description "User";              Column "eventUser";        NotNull>]    user:string
    [<Description "Document";          Column "eventAppFile";     NotNull>]    doc: string
    [<Description "Application";       Column "eventApplication"; NotNull>]    app:string
    [<Description "Count";             Column "eventCount";       NotNull>]    count:int
    [<Description "Type";              Column "eventType_1";      NotNull>]    kind: string
    [<Description "Current Value";     Column "eventValue_1";     NotNull>]    value1:string
    [<Description "UI Element Name";   Column "eventCell_1";      NotNull>]    cell1:string
    [<Description "Application Frame"; Column "eventAppFrame_1";  NotNull>]    appFrame1:string
    [<Description "Duration";          Column "Duration";         NotNull>]    duration:float
    [<Column "events_est";   NotNull>] links: string
    [<Column "events_probs"; NotNull>] probs: string
    //[<Association(ThisKey="user", OtherKey="id", CanBeNull=false)>] User:User.User
    // Only for Excel export
    [<Description "AutoID, Control Type "; Column "eventCell_2"; NotNull>] cell2:string
    [<Description "UI Element Pattern"; Column "eventCell_3"; NotNull>] cell3:string
    [<Description "Pattern Value"; Column "eventCell_4"; NotNull>] cell4:string
    [<Description "X-Path"; Column "eventCell_5"; NotNull>] cell5:string
    [<Description "Location"; Column "eventLocation";   NotNull>] location:string
    [<Description "OCR Data"; Column "eventData_2";   NotNull>] image:byte[] } with
    member o.SetValue1 x = {o with value1 = x} // To use from C# factory


type ProcKind =
    | [<MapValue "">]           None       = 0
    | [<MapValue "Essential">]  Essential  = 1
    | [<MapValue "Management">] Management = 2
    | [<MapValue "Support">]    Support    = 3

[<Table "Processes">]
type Proc = {
    [<Column "ProcessID"; PrimaryKey>] id:int
    [<Column "Process_Name"; NotNull>] name: string
    //[<Column "Type"; NotNull>] kind: ProcKind // not present in db
    [<Column "Source"; NotNull>] from: string
    [<Column "Destination"; NotNull>] into: string
    [<Column "Catagory"; NotNull>] category: string
    [<Column "First_Detected"; NotNull>] first: Date
    [<Column "Most_Recent"; NotNull>] recent: Date
    [<Column "Total_Time"; NotNull>] total: int64
    [<Column "Avg_Time"; NotNull>] average: int64
    [<Column "Automation_Potential"; NotNull>] potential: float
    [<Column "Frequency"; NotNull>] frequency: string
    [<Column "Steps"; NotNull>] steps: float
    [<Column "Confidence"; NotNull>] confidence: float
    [<Column "Volume"; NotNull>] volume: int
    [<Column "Variations"; NotNull>] variants: int
    Users: int
    Apps:  int
    Depts: int
    Cost:  float
    Roi:   float }


module Tasks =
    type Kind =
        | [<MapValue "">]           None       = 0
        | [<MapValue "Event">]      Event      = 1
        | [<MapValue "Activity">]   Activity   = 2
        | [<MapValue "Gateway">]    Gateway    = 3
        | [<MapValue "Message">]    Message    = 4
        | [<MapValue "Data">]       Data       = 5
        | [<MapValue "Task">]       Task       = 6
        | [<MapValue "Subprocess">] Subprocess = 7

    type LinkKind =
        | [<MapValue "Sequence">]    Sequence    = 1
        | [<MapValue "Message">]     Message     = 2
        | [<MapValue "Association">] Association = 3

    // [<Print "{name}, {category}, {depts}, {users}">]
    [<Table "Tasks">]
    type Task = {
        [<Column "TaskID"; PrimaryKey>] id:int
        [<Column "ProcessID"; Nullable>] proc:int System.Nullable
        [<Column "Task_Name"; NotNull>] name: string
        [<Column "Type"; NotNull>] kind: Kind
        [<Column "Source"; NotNull>] from: string
        [<Column "Destination"; NotNull>] into: string
        [<Column "Catagory"; NotNull>] category: string
        [<Column "First_Detected"; NotNull>] first: Date
        [<Column "Most_Recent"; NotNull>] recent: Date
        [<Column "Total_Time"; NotNull>] total: int64
        [<Column "Avg_Time"; NotNull>] average: int64
        [<Column "Automation_Potential"; NotNull>] potential: float
        [<Column "Frequency"; NotNull>] frequency: string
        [<Column "Steps"; NotNull>] steps: float
        [<Column "Confidence"; NotNull>] confidence: float
        [<Column "Volume"; NotNull>] volume: int
        [<Column "Variations"; NotNull>] variants: int
        [<Association(ThisKey="id", OtherKey="proc", CanBeNull=false)>] Ops:Op[]
        Users: int
        Apps:  int
        Depts: string
        Cost:  float
        Roi:   float }

    [<Table "Tasks">]
    type Node = {
        //[<Column "Id"; PrimaryKey; Identity>] id:int
        [<Column "ProcessID"; NotNull>] proc:int
        [<Column "TaskID"; NotNull>] task:int 
        [<Association(ThisKey="proc", OtherKey="id", CanBeNull=false)>] Proc:Proc
        [<Association(ThisKey="task", OtherKey="id", CanBeNull=false)>] Task:Task
        [<Association(ThisKey="id", OtherKey="destination", CanBeNull=false)>] Links:Link[] }
    and
        [<Table "Links">]
        Link = {
        [<Column "Id"; PrimaryKey>] id:int
        [<Column "Type"; NotNull>] kind:LinkKind
        [<Column "Source"; NotNull>] source:int
        [<Column "Destination"; NotNull>] destination:int
        // [<Association(ThisKey="source",      OtherKey="id", CanBeNull=false)>] Source:Node
        // [<Association(ThisKey="destination", OtherKey="id", CanBeNull=false)>] Destination:Node
         }


module Points =
    type Tooltip = { rate:float; duration:float }
    type Row     = { dept:string; user:string; value:float }
    type App     = { user:string; app:string; value:float; y:int }
    type Month   = { group:string; year:int; month:int; value:float } // group is for task or proc
    type Dept    = { dept:string; value:float }
    type Pie     = { title:string; value:float }
    type Link    = { from:string; into:string; weight:int }

    type Scatter = {
        dept:string
        user:string
        label:string
        value:float
        tooltip:Tooltip option }

    type Gantt = {
        name: string
        dept: string
        from: System.DateTime
        into: System.DateTime }


module Window =
    type Period =
        | All
        | Daily
        | Weekly
        | Monthly

    type Type = { period:Period; from:Date; into:Date }


module Whitelist =
    type Kind =
        | [<MapValue "Application">] Application = 0
        | [<MapValue "Website">]     Website     = 1

    [<Table "Whitelist">]
    type Record = {
        [<Column "ApplicationType"; PrimaryKey>] kind:Kind
        [<Column "Value";           PrimaryKey>] name:string }
    let record k n = {kind = k; name = n}
    let kind x     = x.kind