Get-Command protoc | Foreach-Object { Write-Host "protoc found at: $( $_.Source )" }
Write-Host "PROTO_PATH: $env:PROTO_PATH"

$rootDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
Write-Host "Root Directory: $rootDir"

$contractDir = Join-Path $rootDir "Zylance.Contract"
$uiDir = Join-Path $rootDir "Zylance.UI"
$outDir = Join-Path $uiDir "Src/Generated"

Set-Location $contractDir

# Find all .proto files recursively
$protoFiles = Get-ChildItem -Path . -Filter "*.proto" -Recurse | ForEach-Object { $_.FullName }

if ($protoFiles.Count -eq 0)
{
    Write-Error "No .proto files found in $contractDir"
    exit 1
}

Write-Host "Found $( $protoFiles.Count ) proto file(s):"
$protoFiles | ForEach-Object { Write-Host "  - $_" }

Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $outDir
New-Item -ItemType Directory -Path $outDir

# Run protoc for each proto file
foreach ($protoFile in $protoFiles)
{
    $protoFileName = Split-Path -Leaf $protoFile

    Write-Host "`nCompiling $protoFileName..."

    # Execute protoc with proper PowerShell syntax
    & protoc `
        --proto_path=$env:PROTO_PATH `
        --proto_path=$contractDir `
        --plugin=$contractDir/node_modules/.bin/protoc-gen-ts_proto.cmd `
        --ts_proto_opt=esModuleInterop `
        --ts_proto_opt=env=browser `
        --ts_proto_opt=forceLong=string `
        --ts_proto_opt=outputEncodeMethods=false `
        --ts_proto_opt=outputJsonMethods=true `
        --ts_proto_opt=outputClientImpl=false `
        --ts_proto_opt=nestJs=false `
        --ts_proto_out=$outDir `
        $protoFile

    if ($LASTEXITCODE -ne 0)
    {
        Write-Error "Failed to compile $protoFileName"
        exit $LASTEXITCODE
    }
}

Write-Host "`nSuccessfully compiled all proto files!" -ForegroundColor Green
