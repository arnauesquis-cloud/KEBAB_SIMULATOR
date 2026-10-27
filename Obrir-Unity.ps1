$ErrorActionPreference = 'Stop'
$editorPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe'
if (-not (Test-Path -LiteralPath $editorPath)) {
    throw 'Obre Unity Hub i afegeix aquesta carpeta com a projecte amb Unity 6000.3.24f1.'
}
$projectPath = $PSScriptRoot
$editorArguments = '-projectPath "' + $projectPath + '" -openfile "' + (Join-Path $projectPath 'Assets\Kebab\Scenes\Restaurant.unity') + '"'
# Interactive editor requested by the person running this launcher.
Start-Process -FilePath $editorPath -ArgumentList $editorArguments
