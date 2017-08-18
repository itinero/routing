The Route object is usually the result of a route calculation on the [[Router]]. There is just the bare minimum to represent a route accurately but there are some extension methods available on it to work with the Route object easier.

The following usecases need to be supported by the route object:
- A geometry-only route, just for display.
- A route with meta-data like distance/time etc.
- A complete route with all meta-data, streetnames, sidestreets, intersections and support for instructions.

To support all these usecases at the same time the Route object has the following properties:
- ```Shape```: The coordinates representing the shape of the route.
- ```ShapeMeta```: Meta-data about segments of the route.
- ```Stops```: Data about stops along the route.
- ```Branches```: Data about intersections along the route.

A minimum route only needs to have ```Shape``` array set. Any of the others are optional.

### Shape

An array of coordinates, the bare minimum to represent the route on a map.

### ShapeMeta

A collection of **Meta** objects, meta-data about parts of the shape. The segment of the shape the meta-data applies to can be extracted by using the Shape property. 

- ```Shape```: An integer representing the **endlocation** in the shape array this meta-data applies to. The begin location is either 0 or the end of the previous Meta object.
- ```Profile```: The name of the vehicle profile that applies to this segment.
- ```Attributes```: A collection of key-value attributes, could be streetname, oneway tags, highway category for example but can contain any relevant meta-data.
- ```AttributesDirection```: True, means forward, false means backward.
- ```Distance```/```Time```: The distance/time of the segment of the shape this applies to.

### Stops

An array of **Stop** objects, usually a route has only two stops, beginning and end. A stop has:
- ```Shape```: An integer representing the location in the Shape array this stop occurs along the route.
- ```Coordinate```: The location of the stop, this doesn't have to be on the route, it can be a house next to the road.
- ```Attributes```: A collection of key-value attributes, could be name, address for example but can contain any relevant meta-data.
- ```Distance```/```Time```: Two properties that contain the distance/time from the beginning of the route to this stop.

### Branches

A collection of branches that represent any road not taken along the route. This is included to be able to calculate routing instructions based on the route object alone.

- ```Shape```: An integer representing the location in the shape array this branch occurs.
- ```Attributes```: A collection of key-value attributes, could be streetname, oneway tags, highway category for example but can contain any relevant meta-data.
- ```AttributesDirection```: True, means forward, false means backward.

## Example

```json
{
  "Shape": [
    [
      6.208951,
      49.61287
    ],
    [
      6.208951,
      49.61287
    ],
    [
      6.208805,
      49.61286
    ],
    [
      6.208506,
      49.61284
    ],
    [
      6.208352,
      49.61286
    ],
    [
      6.208031,
      49.61293
    ],
    [
      6.207917,
      49.61294
    ]
  ],
  "ShapeMeta": [
    {
      "Shape": 0,
      "Attributes": {
        "profile": "car"
      }
    },
    {
      "Shape": 1,
      "Attributes": {
        "name": "Rue Jean Schaus",
        "highway": "residential",
        "maxspeed": "30",
        "profile": "car",
        "distance": "0.03165637",
        "time": "0.005065019"
      }
    },
    {
      "Shape": 6,
      "Attributes": {
        "name": "Rue Jean Schaus",
        "oneway": "yes",
        "highway": "residential",
        "maxspeed": "30",
        "profile": "car",
        "distance": "76.13165",
        "time": "12.18106"
      }
    }
  ],
  "Stops": [
    {
      "Shape": 0,
      "Coordinates": [
        6.210227,
        49.61304
      ],
      "Attributes": {
        "distance": "0",
        "time": "0"
      }
    },
    {
      "Shape": 6,
      "Coordinates": [
        6.207917,
        49.61294
      ],
      "Attributes": {
        "distance": "76.13165",
        "time": "12.18106"
      }
    }
  ],
  "Branches": [
    {
      "Shape": 1,
      "Coordinates": [
        6.208204,
        49.6138
      ],
      "Attributes": {
        "name": "Op der Houscht",
        "highway": "residential",
        "maxspeed": "30"
      }
    },
    {
      "Shape": 1,
      "Coordinates": [
        6.208677,
        49.6128
      ],
      "Attributes": {
        "name": "Rue Jean Schaus",
        "oneway": "yes",
        "highway": "residential",
        "maxspeed": "30"
      }
    }
  ]
}
```
