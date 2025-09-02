@echo off
REM ========================================
REM GenProto.bat - PreBuild/PostBuild helper
REM %1 = repo root (SolutionDir)
REM %2 = relative output folder (from repo root)
REM %3 = type flag (1=PreBuild, 0=PostBuild)
REM ========================================

REM Save current directory
SET curPath=%cd%
echo Current directory: %curPath%

REM Set root path (repo root)
SET rootPath=%~1
REM Remove trailing backslash if present
IF "%rootPath:~-1%"=="\" SET rootPath=%rootPath:~0,-1%
echo Root directory (repo root): %rootPath%

REM Compute absolute output path
SET outputPath=%rootPath%\%~2
REM Resolve ".." in path
FOR %%I IN ("%outputPath%") DO SET outputPath=%%~fI
echo Output path: %outputPath%

REM Create output directory if it doesn't exist
IF NOT EXIST "%outputPath%" (
    mkdir "%outputPath%"
    echo Created output directory: %outputPath%
)

REM Change to Common\Protocol folder for protoc
FOR %%I IN ("%rootPath%\..\Common\Protocol") DO SET protoPath=%%~fI
IF NOT EXIST "%protoPath%" (
    echo ERROR: Common\Protocol folder not found at %protoPath%
    PAUSE
    EXIT /B 1
)
CD "%protoPath%"
echo Changed to Common\Protocol directory: %cd%

REM Run protoc.exe
REM You must set the full path to protoc.exe if not in PATH
SET protocPath=protoc.exe
IF NOT EXIST "%protocPath%" (
    echo ERROR: protoc.exe not found at %protocPath%
    PAUSE
    EXIT /B 1
)
echo Running protoc.exe to generate C# code...
protoc.exe -I=./ --csharp_out=%outputPath% ./Protocol.proto ./Enum.proto ./Struct.proto
IF ERRORLEVEL 1 (
    echo Error occurred during protoc execution. Pausing for inspection...
    PAUSE
)

echo protoc execution completed successfully.

REM Run PacketGenerator.exe
SET packetGenPath=%rootPath%\..\Tools\PacketGenerator\bin\PacketGenerator.exe
IF NOT EXIST "%packetGenPath%" (
    echo ERROR: PacketGenerator.exe not found at %packetGenPath%
    PAUSE
    EXIT /B 1
)
echo Starting PacketGenerator.exe with output path: %outputPath% and type: %3...
START "" "%packetGenPath%" -o "%outputPath%" -t %3

REM Return to original directory
CD "%curPath%"
echo Returned to original directory: %curPath%

EXIT
