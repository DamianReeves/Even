// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"
open Fake

open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO
#if MONO
#else
#load "packages/SourceLink.Fake/tools/Fake.fsx"
open SourceLink
#endif

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override


Target "All" DoNothing

// Default target
Target "Default" (fun _ ->
    trace "Hello World from FAKE"
)

// start build
RunTargetOrDefault "All"
