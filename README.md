#  Itinero

![Build status](http://build.osmsharp.com/app/rest/builds/buildType:(id:Itinero_RoutingDevelop)/statusIcon)
[![Visit our website](https://img.shields.io/badge/website-itinero.tech-020031.svg) ](http://www.itinero.tech/)
[![GPL licensed](https://img.shields.io/badge/license-GPLv2-blue.svg)](https://github.com/itinero/routing/blob/develop/LICENSE.md)

- Itinero: [![NuGet](https://img.shields.io/nuget-Itinero/v/Itinero.svg?style=flat)](http://www.nuget.org/profiles/Itinero)  
- Itinero.Geo: [![NuGet](https://img.shields.io/nuget/v/Itinero.Geo.svg?style=flat)](http://www.nuget.org/profiles/Itinero.Geo)  
- Itinero.IO.Osm: [![NuGet](https://img.shields.io/nuget/v/Itinero.IO.Osm.svg?style=flat)](http://www.nuget.org/profiles/Itinero.IO.Osm)
- Itinero.IO.Shape: [![NuGet](https://img.shields.io/nuget/v/Itinero.IO.Shape.svg?style=flat)](http://www.nuget.org/profiles/Itinero.IO.Shape)


**Itinero** is a routing project for .NET/Mono to calculate routes in a road network. By default the routing network is based on OpenStreetMap (OSM) but it's possible to load any road network. The most important features:

- Calculating routes from A->B.
- Calculating distance/time matrices between a set of locations.
- Processing OSM-data into a routable network.
- Generating turn-by-turn instructions.
- Routing on mobile devices and lower-resource environments.

### Documentation & Samples

Check the [wiki](https://github.com/itinero/routing/wiki) for documentation and sample code. Don't hesitate to ask questions using an [issue](https://github.com/itinero/routing/issues) or request more documentation on any topic you need.

### Other projects

- [routing](https://github.com/itinero/routing): The core routing project, used by all other projects.
- [idp](https://github.com/itinero/idp): The data processing project, a CLI tool to process data into routerdb's.
- [routing-api](https://github.com/itinero/routing-api): A routing server that can load routerdb's and accept routing requests over HTTP.
