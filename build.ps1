param(
    [string]$GameDirectory = 'C:\Program Files (x86)\Steam\steamapps\common\Car Mechanic Simulator 2018'
)

$ErrorActionPreference = 'Stop'
$managed = Join-Path $GameDirectory 'cms2018_Data\Managed'
$core = Join-Path $GameDirectory 'BepInEx\core'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$output = Join-Path $PSScriptRoot 'EngineBuildChecklist.dll'
$references = @('BepInEx.dll', '0Harmony.dll') | ForEach-Object { Join-Path $core $_ }
$references += @(
    'mscorlib', 'System', 'System.Core', 'Assembly-CSharp-firstpass',
    'UnityEngine', 'UnityEngine.CoreModule', 'UnityEngine.IMGUIModule',
    'UnityEngine.InputModule', 'UnityEngine.TextRenderingModule',
    'UnityEngine.UI', 'UnityEngine.UIModule'
) | ForEach-Object { Join-Path $managed "$_.dll" }
$sources = @(
    'EngineBuildChecklist.cs',
    'EngineBuildChecklist.RadialMenuPatches.cs',
    'EngineBuildChecklist.GameplayInputPatches.cs',
    'EngineBuildChecklist.CursorPatches.cs',
    'EngineBuildChecklist.Panel.cs'
) | ForEach-Object { Join-Path $PSScriptRoot $_ }

foreach ($path in @($compiler) + $references + $sources) {
    if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required file missing: $path" }
}
$compilerArgs = @('/nologo', '/noconfig', '/nostdlib+', '/target:library', '/optimize+', '/warnaserror+', "/out:$output")
$compilerArgs += $references | ForEach-Object { "/reference:$_" }
$compilerArgs += $sources
& $compiler @compilerArgs
if ($LASTEXITCODE -ne 0) { throw "C# compiler exited with code $LASTEXITCODE." }
Write-Output "Built $output"
