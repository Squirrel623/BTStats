# Clean
if (Test-Path .\BTStatsCore\wwwroot) {
  Remove-Item .\BTStatsCore\wwwroot\* -Recurse
}
if (Test-Path .\BTStatsCore\bin\Release\PublishOutput) {
  Remove-Item .\BTStatsCore\bin\Release\PublishOutput\* -Recurse
}
if (Test-Path .\ReleaseDocker\app) {
  Remove-Item .\ReleaseDocker\app -Recurse
}

# First, build assets
Set-Location -Path .\BTStatsCore\wwwsrc
$env:BABEL_ENV = "production"
$env:NODE_ENV = "production"
yarn release

# Next, publish dotnet
Set-Location -Path ..
dotnet restore
dotnet publish -c Release -o .\bin\Release\PublishOutput

# Copy files to stats docker repo
Copy-Item -Path .\bin\Release\PublishOutput -Destination ..\ReleaseDocker\app -Recurse -Container

Set-Location ..\ReleaseDocker
Remove-Item .\app\wwwsrc -Recurse