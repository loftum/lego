# Lego.sln

Solution contains
- console apps
- libraries (device drivers + LCTP library)
- test console apps (for testing hardware)

Some apps are .net classic, some are .net core.
Raspberry pi Zero (ARMv6) does not support .net core, hence apps must be built for .net classic, and run with mono.
Raspberry pi 3 will run .net core apps.

## Prerequisites
- .net core
- mono / .NET
- Xamarin.ios (for steering wheel app)
- preferably Visual Studio with Xamarin.ios support / Visual Studio for Mac
- Visual Studio Code also works

## Build
### Visual Studio
- Open Lego.sln in Visual Studio
- (restore nuget packages)
- Build

### Command line
`$ dotnet restore`\
`$ dotnet build`

## Deploy to raspberry pi
### .net core apps
`.../lego/dotnet $ ./publish.sh <project> <framework> user@host`\
E.g.\
`.../lego/dotnet $ ./publish.sh LegoCarServer2 netcore2.0 user@pi`


### .net classic apps
`.../lego/dotnet $ ./deploy.sh <project> user@host`\
E.g.\
`.../lego/dotnet $ ./deploy.sh LegoCarServer user@pi`
