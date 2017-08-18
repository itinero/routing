**Itinero** is a library for .NET/Mono to calculate routes in a road network. By default the routing network is based on OpenStreetMap (OSM) but it's possible to load any road network. The most important features:

- Calculating routes from A->B.
- Calculating distance/time matrices between a set of locations.
- Processing OSM-data into a routable network.
- Processing data from shapefiles into a routable network.
- Generating turn-by-turn instructions.
- Memory-mapped data storage for routing on mobile devices and lower-resource environments.

Several algorithms are implemented for different scenarios: Dijkstra, A*, and Contraction Hierarchies.
