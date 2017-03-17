dotnet build ./src/Itinero
dotnet build ./src/Itinero.Geo
dotnet build ./src/Itinero.IO.Osm
dotnet build ./src/Itinero.IO.Shape

dotnet build ./test/Itinero.Test
dotnet build ./test/Itinero.Test.Functional
dotnet build ./test/Itinero.Test.Runner

dotnet build ./samples/Sample.Basic -r ubuntu.16.04-x64
dotnet build ./samples/Sample.Basic -r win10-x64
dotnet build ./samples/Sample.Basic -r osx.10.11-x64
dotnet build ./samples/Sample.Matrix -r ubuntu.16.04-x64
dotnet build ./samples/Sample.Matrix -r win10-x64
dotnet build ./samples/Sample.Matrix -r osx.10.11-x64