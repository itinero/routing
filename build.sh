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

# Waiting to port to .NET core.
# dotnet build ./samples/Sample.Basic -f netstandard1.3
# dotnet build ./samples/Sample.Matrix -f netstandard1.3
# dotnet build ./samples/Sample.Shape -f netstandard1.3
