$ErrorActionPreference = "stop"
$PSScriptRoot = "$PSScriptRoot/../src"
& 'C:\Program Files\Unity\Editor\Unity.exe' -quit -batchmode -logFile stdout.log -projectPath $PSScriptRoot -executeMethod BuildTool.BuildGame | Write-Output