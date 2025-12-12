dotnet tool uninstall -g gitsith
dotnet pack c:\Source\Nucular\git-sith-command\GitSith.csproj -c Release
dotnet tool install -g --add-source c:\Source\Nucular\git-sith-command\bin\Release GitSith