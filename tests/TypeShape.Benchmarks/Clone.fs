﻿module TypeShape.Benchmarks.Clone

open System
open BenchmarkDotNet.Attributes

open TypeShape.Clone
open TypeShape.Tests.StagedClone

type Record =
    { A : string ; B : int ; C : bool }

type Union =
    | A of int
    | B of string * int
    | C

[<Struct>]
type SRecord = { x : string ; y : bool }

[<Struct>]
type SUnion = SA of x:int | SB of y:string | SC

type TestType = (struct(Record list list * string list [] * int option [] * Union list * SRecord list * SUnion list))

let baselineCloner : TestType -> TestType =
    fun (struct(v1,v2,v3,v4,v5,v6)) ->

        let cloneRecord (r : Record) =
            { A = r.A ; B = r.B ; C = r.C }

        let cloneUnion (u : Union) =
            match u with
            | A x -> A x
            | B(x,y) -> B(x,y)
            | C -> C

        let v1' = v1 |> List.map (List.map cloneRecord)
        let v2' = v2 |> Array.map (List.map id)
        let v3' = v3 |> Array.map id
        let v4' = v4 |> List.map cloneUnion
        let v5' = v5 |> List.map id
        let v6' = v6 |> List.map id
        struct(v1',v2',v3',v4',v5',v6')

let typeShapeCloner : TestType -> TestType =
    clone<TestType>

//let unquoteStagedCloner : TestType -> TestType =
//    mkStagedCloner<TestType>()

//let compiledStagedCloner : TestType -> TestType =
//    mkCompiledCloner<TestType>()


let testValue : TestType =
    let rs = [ for i in 1 .. 100 -> { A = sprintf "lorem ipsum %d" i ; B = i ; C = i % 2 = 0 } ]
    let ss = [for i in 1 .. 20 -> string i]
    let us = [A 42; B("42", -1); C]
    let srs = [for i in 1 .. 10 -> { x = sprintf "lorem ipsum %d" i; y = i % 2 = 0 }]
    let sus = [SA 42; SB "42" ; SC]
    struct([rs; []], [|ss|], [|for i in 1 .. 20 -> Some i|], us, srs, sus)

[<MemoryDiagnoser>]
type CloneBenchmarks() =
    [<Benchmark(Description = "Baseline Cloner", Baseline = true)>]
    member __.Baseline() = baselineCloner testValue |> ignore
    [<Benchmark(Description = "TypeShape Cloner")>]
    member __.Reflection() = typeShapeCloner testValue |> ignore
    //[<Benchmark(Description = "TypeShape Unquote Staged Cloner")>]
    //member __.Unquote() = unquoteStagedCloner testValue |> ignore
    //[<Benchmark(Description = "TypeShape Compiled Staged Cloner")>]
    //member __.TypeShape() = compiledStagedCloner testValue |> ignore
