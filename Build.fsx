#r "paket:
nuget BlackFox.Fake.BuildTask
nuget BlackFox.CommandLine
nuget NuGet.CommandLine
nuget Fake.BuildServer.TeamFoundation
nuget Fake.Core.Target
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MSBuild
nuget Fake.IO.FileSystem
nuget Fake.IO.Zip
nuget Fake.Runtime
//"
#load ".microcelium/lib/microcelium.fsx"

#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#r "netstandard"
#endif

open BlackFox.CommandLine
open BlackFox.Fake
open Fake.Core
open Fake.BuildServer
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Microcelium.Fake

BuildServer.install [ TeamFoundation.Installer ]

if BuildServer.buildServer = LocalBuild then
  CoreTracing.ensureConsoleListener ()

let version = Version.fromEnvironment ()
let versionstr = Version.toString version

let binDir = Environment.defaultBinDir
let testResultsDir = Environment.defaultTestResultsDir

let srcDir = Path.getFullName "./src" |> Path.normalizeFileName
let shotsDir = testResultsDir @@ "screenshots" |> Path.normalizeFileName

let slnFile = Path.combine srcDir "Microcelium.Testing.sln"

let projects = 
  GlobbingPattern.createFrom srcDir
  ++ "*/Microcelium.Testing.*.csproj"
  -- "*/*.Tests.*"
  |> Seq.toList

let getSuffix (v: Version.Entry) =
  match v.suffix with
  | "" -> None 
  | null -> None
  | _ -> Some v.suffix

let props = 
  [("VersionPrefix", version.prefix)
   ("VersionSuffix", version.suffix)
   ("Version", Version.toString version)]

let optBuildDefault (p: DotNet.BuildOptions) =
  { p with
      Configuration = DotNet.BuildConfiguration.Debug
      MSBuildParams =  
      { p.MSBuildParams with 
          NoWarn = Build.msbNowarn
          Properties = p.MSBuildParams.Properties @ props 
          NoConsoleLogger = false } }

let optPackDefault (p: DotNet.PackOptions) = 
  { p with
      Configuration = DotNet.BuildConfiguration.Debug
      OutputPath = Some binDir
      VersionSuffix = getSuffix version
      MSBuildParams =
        { p.MSBuildParams with
            NoWarn = Build.msbNowarn
            Properties = p.MSBuildParams.Properties @ props
            NodeReuse = false
            DisableInternalBinLog = true
            NoConsoleLogger = false
        }
  }

let PrepareSelenium = BuildTask.create "PrepareSelenium" [] {
  if BuildServer.buildServer <> LocalBuild then
    Process.killAllByName "chrome"
    Process.killAllByName "chrome.exe"

  Process.killAllByName "chromedriver"
  let dirs = !! binDir ++ shotsDir
  dirs |> Seq.iter Directory.ensure
  dirs |> Shell.cleanDirs
}

let Clean = BuildTask.create "Clean" [PrepareSelenium] {
  [binDir; testResultsDir] |> List.iter (fun x -> 
    printfn "cleaning `%s`" x
    Directory.create x
    Shell.cleanDir x
  )

  GlobbingPattern.createFrom srcDir
  ++ "**/bin"
  ++ "**/obj"
  -- "**/node_modules/**"
  -- "**/node_modules"
  |> Shell.cleanDirs
}

let VersionTask = BuildTask.create "VersionTask" [Clean] {
  Trace.logfn "versionMajorMinor: %s" version.raw
  Trace.logfn "versionPrefix:     %s" version.prefix
  Trace.logfn "versionSuffix:     %s" version.suffix
  Trace.logfn "version:           %s" <| Version.toString version
  Trace.logfn "buildServer:       %A" <| Fake.Core.BuildServer.buildServer

  Version.toString version |> Trace.setBuildNumber
}

let Build = BuildTask.create "Build" [VersionTask] {
   DotNet.build optBuildDefault slnFile
}

let Test = BuildTask.create "Test" [Build] {
  Testing.runUnitTests (Testing.UnitTestProjects srcDir) |> ignore
}

BuildTask.create "Package" [Test] {
  projects |> Seq.iter (DotNet.pack optPackDefault)
}

Targets.publishLocal binDir versionstr |> Target.create "ToLocalNuget"
Targets.packageLocal srcDir |> Target.create "ToLocalPackageRepo"

"Build" ==> "ToLocalPackageRepo"

Target.runOrDefault <| if Environment.runPublish then "Package" else "Test"