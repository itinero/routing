#  Itinero

[![Build status](http://build.itinero.tech:8080/app/rest/builds/buildType:(id:Itinero_RoutingDevelop)/statusIcon)](https://build.itinero.tech/viewType.html?buildTypeId=Itinero_RoutingDevelop)
[![Join the chat at https://gitter.im/Itinero/Lobby](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Itinero/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Visit our website](https://img.shields.io/badge/website-itinero.tech-020031.svg) ](http://www.itinero.tech/)
[![Apache 2.0 licensed](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://github.com/itinero/routing/blob/develop/LICENSE.md)

- Itinero: [![NuGet Badge](https://buildstats.info/nuget/Itinero)](https://www.nuget.org/packages/Itinero/)
- Itinero.Geo: [![NuGet Badge](https://buildstats.info/nuget/Itinero.Geo)](https://www.nuget.org/packages/Itinero.Geo/)
- Itinero.IO.Osm: [![NuGet Badge](https://buildstats.info/nuget/Itinero.IO.Osm)](https://www.nuget.org/packages/Itinero.IO.Osm/)
- Itinero.IO.Shape: [![NuGet Badge](https://buildstats.info/nuget/Itinero.IO.Shape)](https://www.nuget.org/packages/Itinero.IO.Shape/)

**Itinero** is a routing project for .NET/Mono to calculate routes in a road network. By default the routing network is based on OpenStreetMap (OSM) but it's possible to load any road network. The most important features:

- Calculating routes from A->B.
- Calculating distance/time matrices between a set of locations.
- Processing OSM-data into a routable network.
- Generating turn-by-turn instructions.
- Routing on mobile devices and lower-resource environments.

### Documentation & Samples

Check the [documentation website](http://docs.itinero.tech/docs/index.html) for documentation and sample code. Don't hesitate to ask questions using an [issue](https://github.com/itinero/routing/issues) or request more documentation on any topic you need.

### Other projects

- [routing](https://github.com/itinero/routing): The core routing project, used by all other projects.
- [geo](https://github.com/itinero/geo): A collection of extensions to Itinero to work with NTS.
- [idp](https://github.com/itinero/idp): The data processing project, a CLI tool to process data into routerdb's.
- [routing-api](https://github.com/itinero/routing-api): A routing server that can load routerdb's and accept routing requests over HTTP.

### Contributions

Contributions are **always** appreciated, especially if they match what's on our [roadmap](http://docs.itinero.tech/docs/itinero/development/index.html) or it fixes a [bug](https://github.com/itinero/routing/issues). Also check the [contribution guidelines](https://github.com/itinero/routing/issues) before planning anything big. Thanks to past contributions by @Chaz6, @bertt, @jbelien, @pietervdvn, @ironromeo, @jerryfaust and @airbreather. 

If you like routing, OpenStreetMap & Itinero [get in touch](http://www.itinero.tech/#contact), we may have a job for you!
