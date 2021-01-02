#r "paket:
nuget JetBrains.dotCover.CommandLineTools
nuget AWSSDK.S3
nuget Fake.BuildServer.TeamCity
nuget Fake.Core.Xml
nuget Fake.Core.Target
nuget Fake.Core.Trace
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.NuGet
nuget Fake.IO.FileSystem
nuget Fake.IO.Zip
nuget Fake.Runtime 
//"
#load ".microcelium/lib/Microcelium.fsx"
#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.BuildServer
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Microcelium.Fake

BuildServer.install [ TeamCity.Installer ]
CoreTracing.ensureConsoleListener ()

(* read EnvVar 
  let myEnvVar = Util.environVarOrDefault ["myEnvVarKey1"; "myEnvVarKey2"] "default value"
*)

(* setting the version
  let version = Version.fromVersionIni ()   //looks for a .\Version.ini file
  let version = Version.fromFile "filepath" //looks for a file @ "filepath"
*)

let version = Version.from "1.2" //parses from param
let versionparts = Version.parts version
let versionstr = Version.toString version

let srcDir = Path.getFullName "./src"
let binDir = Path.getFullName "./bin"
let shotsDir = Path.getFullName "./screenshots"
let selenDir = Path.getFullName "./selenium"

let project = "Microcelium.Testing"
let tests = seq { yield (srcDir, Default) }

(* override the default Build because it adds adapters that we don't want
    for this specific build, e.g. we're building projects to help with 
    unit and selenium tests *)
Target.create "Build" (fun _ ->
   DotNet.build (fun p ->
     { p with
         Configuration = DotNet.BuildConfiguration.Debug
         MSBuildParams = 
           { p.MSBuildParams with 
               NodeReuse = false
               NoWarn = Build.msbNowarn
               Properties = Build.msbProperties versionparts 
               BinaryLoggers = Some []
               FileLoggers = Some []
               DistributedLoggers = Some []
               Loggers = Some [] 
               DisableInternalBinLog = true  
           }
     }) (srcDir)
)

Target.create "PrepareSelenium" <| Targets.prepareSelenium binDir shotsDir
Target.create "Clean" <| Targets.clean srcDir binDir
Target.create "Version" <| Targets.version version
Target.create "Test" <| Targets.test tests project binDir
Target.create "Publish" <| Targets.publish binDir

(* about the only part that needs customized *)
Target.create "Package" (fun _ -> 
  !! (srcDir @@ "*/*.csproj")
  -- (srcDir @@ "*/*.Tests.csproj")
  -- (srcDir @@ "*.ignore/**")
  |> Seq.map System.IO.Path.GetFileNameWithoutExtension
  |> Seq.iter (fun p -> Build.packageNuget srcDir p versionparts binDir) 
)

Target.create "ToLocalNuget"  <| Targets.publishLocal binDir versionstr

(* `NuGetCachePath` EnvVar should be set to your Nuget Packages Install dir already, but 
    `TargetVersion` should be set prior to running build.bat :
    set TargetVersion=1.14 *)
Target.create "ToLocalPackageRepo" <| Targets.packageLocal srcDir

"PrepareSelenium"
  ==> "Clean" 
  ==> "Version"
  ==> "Build"
  ==> "Test"
  ==> "Package"
  =?> ("Publish", Environment.runPublish)
  
Target.runOrDefault <| if Environment.runPublish then "Publish" else "Test"