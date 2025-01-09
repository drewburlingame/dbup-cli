## testing the dotnet tool locally

to setup a local test, specify the version and run the script.
```shell
$version=3.0.0-beta
dotnet pack ./src/dbup-cli/dbup-cli.csproj --configuration Release /p:PackageVersion=$version
dotnet tool uninstall dbup-cli-2 --global
dotnet tool install dbup-cli-2 --global --version $version --add-source ./src/dbup-cli/nupkg
```

to restore from nuget
```shell
dotnet tool uninstall dbup-cli-2 --global
dotnet tool install dbup-cli-2 --global
```