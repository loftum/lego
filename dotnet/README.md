# Lego.sln

Solution contains
- console apps
- libraries (device drivers + LCTP library)
- test console apps (for testing hardware)

Some apps are .net classic, some are .net core.
Raspberry pi Zero (ARMv6) does not support .net core, hence apps must be built for .net classic, and run with mono.
Raspberry pi 3 will run .net core apps.

The main apps are LegoCarServer (mono) / LegoCarServer2 (.net core) and SteeringWheel (Xamarin.ios).
SteeringWheel is an iOS app and LegoCarServer / LegoCarServer2 is a server app running on the raspberry pi.

SteeringWheel communicates with LegoCarServer via LCTP (Lego Control Transfer Protocol) over a persistent TCP connection. LCTP is an anemic version of HTTP.

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
e.g.\
`.../lego/dotnet $ ./publish.sh LegoCarServer2 netcore2.0 user@pi`


### .net classic apps
`.../lego/dotnet $ ./deploy.sh <project> user@host`\
e.g.\
`.../lego/dotnet $ ./deploy.sh LegoCarServer user@pi`

### running apps on pi
Sudo is required
.net core:\
`~/LegoCarServer2 $ sudo dotnet LegoCarServer2.dll`\
mono:\
`~/LegoCarServer $ sudo mono LegoCarServer.dll`

## Steering Wheel
Deploying SteeringWheel requires a Mac with Visual Studio for Mac:
- Connect a device, select SteeringWheel > Debug > Your iOS device.
- Press play

## LCTP (Lego Control Transfer Protocol)
Anemic version of HTTP.
### Request message
Messages end with CRLF
`<method> <path> [content]\r\n`\
e.g. `SET motor/speed 50\r\n`

Method can be anything: GET, SET, DELETE etc

### Response message:
`<status code> <message>`\
e.g.
`200 OK`
