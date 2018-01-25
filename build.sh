dotnet build ./src/Itinero -f netstandard1.3
dotnet build ./src/Itinero.IO.Osm -f netstandard1.6

# Waiting to port to .NET core.
dotnet build ./test/Itinero.Test
dotnet build ./test/Itinero.Test.Functional

# Build samples.
dotnet build ./samples/Sample.Basic
dotnet build ./samples/Sample.Matrix
dotnet build ./samples/Sample.Elevation
