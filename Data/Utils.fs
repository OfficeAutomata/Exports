[<AutoOpen>]
module Utils
//Refactor: Cleanup

type Print = StructuredFormatDisplayAttribute
type Debug = System.Diagnostics.DebuggerDisplayAttribute

module String =
    let is_empty = System.String.IsNullOrWhiteSpace
    let isValid = not << System.String.IsNullOrWhiteSpace
    let split (c:char) (s:string) = s.Split c
    let size (s:string) = s.Length
    let trim (s:string) = s.Trim()
    let chars (s:string) = s.ToCharArray()
    let replace (x:string) (y:string) (s:string) = s.Replace (x,y)
    let remove x (s:string) = s.Remove x
    let contains (x:string) (s:string) = let s1 = s.ToLower()
                                         s1.Contains(x.ToLower())

module Random =
    let float (r:System.Random) = r.NextDouble()

[<AutoOpen>]
module Functions =
    //let flip f x y = f y x
    let repeat f x = f x x
    let nested f t x = f (t x) x

    let apply  f x = f x //Refactor: Rename to applyf
    let applyf x f = f x

    // let uncarry f  x y  = f (x,y)
    // let uncarry3 f  x y z  = f (x,y,z)
    // let carry   f (x,y) = f  x y

    let skip  x _ = x
    let skipf _ x = x
    let skip2 x _ _ = x
    let skip3 x _ _ _ = x

    // let ignore2<'a,'b> (_:'a) (_:'b) = () // To fix warning: The method or function 'ignore2' should not be given explicit type argument(s) because it does not declare its type parameters explicitly
    let ignore3 _ _ _ = ()
    let ignoreArg callback ignored = callback()

    let (>>>) f g x y = g (f x y)

    let condition predicate positive negative =
        if predicate
        then positive
        else negative

    // module Extras
    let inline flip f a b = f b a
    let inline flip3 f a b c = f c a b
    let inline flip4 f a b c d = f d a b c
    let inline curry f a b = f(a,b)
    let inline uncurry f (a,b) = f a b
    let inline curry3 f a b c = f (a, b, c)
    let inline uncurry3 f (a,b,c) = f a b c
    let inline swap (a,b) = (b,a)

// [<AutoOpen>]
module Pair =
    let pair   x y  = x,y
    // let pairs  x y  = struct (x,y)
    let pairf  x y  = flip pair x y
    let swap   (x,y) = y,x
    let dfst   ((x,_),_) = x
    let dsnd   (_,(_,y)) = y
    let map    func (x,y) = func x, func y
    let pass   func = flip (||>) func
    let tofst  func (x,y) = func x, y
    let tofst2 func (x,y) = func x y, y
    let tosnd  func (x,y) = x, func y
    let tosnd2 func (x,y) = x, func x y
    // let tosnd2 func = fst >> func >> tosnd |> repeat
    let ftos     (f,s)   = f s
    let triplefs (f,s,t) = f,s
    let triplest (f,s,t) = s,t
    let tripleft (f,s,t) = f,t


module Triple =
    let create  f s t = f,s,t
    // let creates f s t = struct (f,s,t)
    let fst (f,_,_) = f
    let snd (_,s,_) = s
    let thd (_,_,t) = t
    let tofst func (f,s,t) = func f, s, t
    let tosnd func (f,s,t) = f, func s, t
    let tothd func (f,s,t) = f, s, func t
    let map   func (f,s,t) = func f, func s, func t
    let flat     (f,(s,t)) = f,s,t


module List =
    let one    = List.exactlyOne
    let size   = List.length
    let clone  = List.replicate
    let single = List.singleton
    let chunks = List.chunkBySize
    let group  = List.groupBy
    let empty  = List.isEmpty
    let count  = List.countBy
    let sortd  = List.sortDescending
    let seq    = List.ofSeq
    let ary    = List.toArray

    /// Aliases for Pair
    let sortf  xs = List.sortBy  fst xs
    let sorts  xs = List.sortBy  snd xs
    let sortfd xs = List.sortByDescending fst xs
    let sortsd xs = List.sortByDescending snd xs
    let groupf xs = List.groupBy fst xs
    let groups xs = List.groupBy snd xs
    let inline sumf xs = List.sumBy fst xs
    let inline sums xs = List.sumBy snd xs

    let inline sumy   f xs = List.sumBy  f xs
    let inline sorty  f xs = List.sortBy f xs
    let inline sortyd f xs = List.sortByDescending f xs

    let add    x  = curry List.Cons x
    let addf   x  = curry List.Cons |> flip <| x
    let build  h  = Pair.pair h  >> List.Cons //Ref: uncarry?
    let buildf t  = Pair.pairf t >> List.Cons
    let print  xs = List.iter  (printfn "%A") xs
    let printi xs = List.iteri (printfn "%2i: %A") xs
    let any    xs = not <| List.isEmpty xs
    // let any2   xs = List.exists2 id xs //Rem:?
    let has    xs = flip List.contains xs
    let foldf  f  = flip List.fold f

    /// Map applied to Pair.first
    let mapf f = f |> Pair.tofst |> List.map
    /// Map applied to Pair.second
    let maps f = f |> Pair.tosnd |> List.map
    let dmap  f = List.map >> List.map <| f
    let dmaps f = maps >> maps <| f

    /// Pad list to required width with fill
    let pad width fill xs =
        let d = size xs - width
        if  d >= 0
        then xs
        else xs @ List.replicate -d fill

    /// Remove first ocurrence from list (or nothing without `key`)
    let rec remove k = function
        | x::xs when x = k -> xs
        | x::xs            -> x::remove k xs
        | []               -> []

    /// `List.forall` which returns false for `[]`
    let all predicate = function
        | [] -> false
        | xs -> List.forall predicate xs

    /// Replace all same item to a new one
    let replace k x' = List.map (fun x -> if x = k then x' else x)

    /// Combine each `xs` item with `x`
    let flat (x,xs) = xs |> List.map (Pair.pair x)

    let fst xs = List.map fst xs
    let snd xs = List.map snd xs

    /// Returns `(Head,Tail)`
    let topair = function
        | x::xs -> x,xs
        | []    -> failwith "Empty List"

    //Rem: With Outcome.Model is not needed?
    module Safe =
        let nulled = function
            | null -> []
            | xs   -> List.ofSeq xs

        let aggregate f = function
            | [] -> 0.
            | xs -> f xs


type 'T List with
    member o.Size = o.Length

type 'T System.Collections.Generic.IEnumerable with
    member o.Size = Seq.length o

type 'T ``[]`` with
    member o.Size = o.Length

module Seq =
    let one  = Seq.exactlyOne
    let size = Seq.length

    let equal a b =
        if   size a <> size b
        then false
        else Seq.forall2 (=) a b

module Option =
    let defarg = defaultArg
    let ofbox x =
        match obj.ReferenceEquals(x,null) with
        | true  -> None
        | false -> Some x

    // let box value = match value with None -> null | Some x -> box x
    // let unbox value = match value with null -> None | _ -> Some (value :?> 'T)


module Ref =
    let assign (m:_ byref) x = m <- x


let crash = failwithf
let isnan = System.Double.IsNaN
let isinf = System.Double.IsInfinity

let isNotNull x = not <| isNull x //Ref: Eliminate
let notNull   x = not <| isNull x

let print    a    = printfn "%A" a; a
let prints s a    = printfn "%s     : %A" s a; a
let printst s t a = printfn "%i | %s: %A" s t a; a

let dfst    = Pair.dfst
let dsnd    = Pair.dsnd
let pair    = Pair.pair
// let pairs   = Pair.pairs
let pairf   = Pair.pairf
let triple  = Triple.create
// let triples = Triple.creates
let ( ** )  = pair

/// Applies `func` to Table of groupped data
// let tricky func =
//        List.maps (fst >> func |> List.map)
//     >> List.map  (applyf >> List.map |> Pair.tosnd2)


let def<'T> = Unchecked.defaultof<'T>


[<AutoOpen>]
module Extras =
    let inline flip f a b = f b a
    let inline flip3 f a b c = f c a b
    let inline flip4 f a b c d = f d a b c
    let inline curry f a b = f(a,b)
    let inline uncurry f (a,b) = f a b
    let inline curry3 f a b c = f (a, b, c)
    let inline uncurry3 f (a,b,c) = f a b c
    let inline swap (a,b) = (b,a)

    let inline quad   a b c d = a,b,c,d
    let inline tuple4 a b c d = a,b,c,d
    let inline tuple5 a b c d e = a,b,c,d,e
    let inline tuple6 a b c d e f = a,b,c,d,e,f

    /// Fixed point combinator.
    let rec fix f x = f (fix f) x
    let rec fix2 f x y = f (fix2 f) x y
    let rec fix3 f x y z = f (fix3 f) x y z

    /// Sequencing operator like Haskell's ($). Has better precedence than (<|) due to the first character used in the symbol.
    let (^) = (<|)

    /// Bottom value
    let undefined<'T> : 'T = raise (System.NotImplementedException("result was implemented as undefined"))

    /// Given a value, apply a function to it, ignore the result, then return the original value.
    let inline tee fn x = fn x |> ignore; x

    /// Custom operator for `tee`: Given a value, apply a function to it, ignore the result, then return the original value.
    let inline (|>!) x fn = tee fn x

    /// Rethrows an exception. This can be used even outside of try-with block. The exception object (stacktrace, etc) is not changed.
    let reraise' (e:exn) : 'T = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e).Throw() ; undefined
    // http://thorarin.net/blog/post/2013/02/21/Preserving-Stack-Trace.aspx
    // https://stackoverflow.com/questions/7168801/how-to-use-reraise-in-async-workflows-in-f

    /// Rethrows an exception, but bebore that applies a function on it. This can be used even outside of try-with block. The exception object (stacktrace, etc) is not changed.
    let reraiseWith (f : exn -> unit) (e:exn) : 'T = f e ; reraise' e

    let inline toOption x = match x with
                            | true, v -> Some v
                            | _       -> None

    let inline tryWith f x = f x |> toOption