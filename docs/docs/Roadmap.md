This is an overview of features that are either in development or on the roadmap:

## Version 1.1

*This version has been released: https://github.com/itinero/routing/blob/master/docs/releasenotes/itinero-1.1.0.md*

This contains some ideas on some non-breaking extensions on top of v1.0.

- Add a weight matrix calculation algorithm that calculates edge->edge weights excluding the source-target edge weights. *DONE*
  - Check to see if the current edge-based matrix algorithm can be replaced by this more general case. *DONE*
- And several other issues and ideas: [Milestone for 1.1](https://github.com/itinero/routing/milestone/3) *DONE*

## Version 1.2

*This version has been released: https://github.com/itinero/routing/blob/master/docs/releasenotes/itinero-1.2.0.md*

- Fixed maxspeed normalization issue.
- Implemented support for nested relations by allowing multiple passes over relations if requested.
- Implemented support for nested cycle route relations in the default bicycle profile.
- Fixed directed weight matrix issue related to resolved points on oneway segments.

## Version 1.3

- Meta-data on vertices: More details [here](https://github.com/itinero/routing/wiki/Development-Plan:--Meta-data-on-vertices).
- A way to extract parts of the network and save them as a new routerDb.
- Add extension method to directly load data from overpass by polygon.
- Use the meta-data on vertices to paste networks back together by keeping node id's.

## Version 2.0 and beyond

- Fix issue with float's not being good enough: https://github.com/itinero/routing/issues/120
- Move to netstandard2.0 *only*.
- Move to .NET core for the functional test project: development should be possible on any platform.
- [PLANNED](https://github.com/itinero/routing/tree/features/constraints): Constrained routing: Routing with constraints like weight limits or vehicle size (width and height).
- [PLANNED] Destination-only access: Handle access constraints where there is destination only access, also when using contracted graphs.
- Better CH: https://github.com/itinero/routing/issues/92
- Reevaluate Lua profiles: https://github.com/itinero/routing/issues/102
- Figure out a way to better detect islands and route under all conditions: https://github.com/itinero/routing/issues/104
- Support for dynamic weights per edge, for example to handle floating car data: https://github.com/itinero/routing/issues/103
- Look at the filesize of the routerDb's: https://github.com/itinero/routing/issues/105
- Elevation profiles: Output height profiles of calculated routes.
- Elevation-aware routing: Routing that takes into account elevation. Think avoiding steep hills for bicycles.
- No-go areas or maximum speeds based on areas: https://github.com/itinero/routing/issues/19
- Alternative routes.
- Map matching.
- Reevaluate lua for instruction generation of fix performance issues.
- Remove nullable booleans to describe directional information.
- Remove this fix: https://github.com/itinero/routing/issues/108
- cleanup weight handlers, let them die!
- cleanup old way of handling restrictions.
- cleanup using bool? for directions and replace by Dir structure.
- cleanup old way of doing edge-based routing.
- Consider implementing support for multi-level route relations: https://github.com/itinero/routing/wiki/Development-plan:-Handle-multi-level-route-relations.#process-relations-in-multiple-passes
- Move OSM specific parsing to the OSM namespace: IAttributeCollectionExtension

## General ideas

A collection of general ideas that may or may not end up on the final roadmap.

#### Add a more efficient data structure to represent restrictions

#### Add extension method to turn a routerpoint into GeoJSON

Just return geojson with all details included:

- Edge with start and end vertex.
- The location on the network.
- The original location.