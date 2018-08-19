#!/usr/bin/env bash
dotnet build ./src/Itinero -f netstandard1.3
dotnet build ./src/Itinero.IO.Osm -f netstandard1.6
dotnet build ./src/Itinero -f netstandard2.0
dotnet build ./src/Itinero.IO.Osm -f netstandard2.0

dotnet build ./test/Itinero.Test
dotnet build ./test/Itinero.Test.Functional

# these are .net core 2.0 projects.
dotnet build ./samples/Sample.Basic
dotnet build ./samples/Sample.Matrix
dotnet build ./samples/Sample.Elevation
