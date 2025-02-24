#!powershell

Write-Host "Formatting code"
dotnet format
Write-Host "Done formatting code"
Write-Host "Running tests"

dotnet test

Write-Host "Testing complete. Exit code: $LASTEXITCODE"

exit $LASTEXITCODE
