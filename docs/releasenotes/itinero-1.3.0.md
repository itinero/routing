Itinero 1.3.0 Release Notes
---------------------------

New features:

- Added the option to extract part of the network: https://github.com/itinero/routing/wiki/RouterDb#extracting-an-area
- Added the option to write a routerdb as a shapefile: https://github.com/itinero/routing/wiki/Shapefiles#routerdb-to-shapefile
- Added the option to keep original node id's and to keep meta-data on vertices: https://github.com/itinero/routing/wiki/Development-Plan:--Meta-data-on-vertices

Bugfixes:

- Fixed weight matrix calculation issues related to oneways: https://github.com/itinero/routing/issues/116
- Fixed hilbert search sometimes missing closest vertex. Thanks @ironromeo

### 1.1.1 -> 1.2.0

- Fixed maxspeed normalization issue.
- Implemented support for nested relations by allowing multiple passes over relations if requested.
- Implemented support for nested cycle route relations in the default bicycle profile.
- Fixed directed weight matrix issue related to resolved points on oneway segments.

### 1.1.0 -> 1.1.1

- Fixed issue with directed weight matrix being unable to handle new contraction hierarchies.
- Fixed issue with directed edge-based routes not returning single-edge routes. This was a breaking change in behaviour.

### Itinero 1.1.0

This update fixes the issues with the edge-based routing experience in v1.0. On top of that there are some more enhancements for network simplification and several bugfixes related to resolving and restriction handling.

Extra functionality:

- Calculate directed routes, arrive and depart on an edge in a fixed direction.
- An extension method to optimize a network by removing obsolete vertices.
- Added support for simplification of edge geometries.

Behaviour changes:

- Contraction performance improvements.
- RouterDb's are bigger after contraction when using advanced restrictions.
- Performances improvements when loading raw OSM data (thanks @airbreather).
- Allow inserting a non-portable array implementation (thanks @airbreather).
- Fixed bollard nodes restriction handling.
- Added a more advanced network builder with support for single-vertex restrictions.
- By default OSM restrictions are now for motor_vehicles.

Bugfixes:

- Fixed NUnit test runner (thanks @airbreather).
- Fixed issue in hilbert search cause resolve to fail in rare cases.
