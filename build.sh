dotnet build ./src/Itinero -f netstandard1.3
# Waiting for NTS .NET core release for IO.Geo
# dotnet build ./src/Itinero.Geo -f netstandard1.3
dotnet build ./src/Itinero.IO.Osm -f netstandard1.6
# Waiting for NTS .NET core release for IO.Shape
# dotnet build ./src/Itinero.IO.Shape -f netstandard1.3

# Waiting to port to .NET core.
# dotnet build ./test/Itinero.Test -f netstandard1.3
# dotnet build ./test/Itinero.Test.Functional -f netstandard1.3
# dotnet build ./test/Itinero.Test.Runner -f netstandard1.3

# Build samples.
dotnet build ./samples/Sample.Basic -r ubuntu.16.04-x64
dotnet build ./samples/Sample.Basic -r win10-x64
dotnet build ./samples/Sample.Basic -r osx.10.11-x64
dotnet build ./samples/Sample.Matrix -r ubuntu.16.04-x64
dotnet build ./samples/Sample.Matrix -r win10-x64
dotnet build ./samples/Sample.Matrix -r osx.10.11-x64
# Waiting for NTS .NET core release for IO.Shape
# dotnet build ./samples/Sample.Shape -f netstandard1.3
