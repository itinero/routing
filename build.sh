dotnet build ./src/Itinero -f netstandard1.3
dotnet build ./src/Itinero.IO.Osm -f netstandard1.6

# Waiting to port to .NET core.
dotnet build ./test/Itinero.Test
dotnet build ./test/Itinero.Test.Functional

# Build samples.
dotnet build ./samples/Sample.Basic -r ubuntu.16.04-x64
dotnet build ./samples/Sample.Basic -r win10-x64
dotnet build ./samples/Sample.Basic -r osx.10.11-x64
dotnet build ./samples/Sample.Matrix -r ubuntu.16.04-x64
dotnet build ./samples/Sample.Matrix -r win10-x64
dotnet build ./samples/Sample.Matrix -r osx.10.11-x64
