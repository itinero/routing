The router class functions as a façade for most if not all routing requests. The router class will decide what routing flavour or algorithm is used for a specific purpose and based on what's available in the router db being used. The main methods/extensions methods that are available on router are:

- `RouterPoint Resolve(...)`: Resolves a point and throws an exception when it fails.
- `Result<RouterPoint> TryResolve(...)`: Tries to resolve a point and returns an error object when it fails.
- `Route Calculate(...)`: Calculates a route and throws an exception when it fails.
- `Result<Route> TryCalculate(...)`: Tries to calculate a route and returns an error object when it fails.
- `float CalculateWeight(...)`: Calculates the weight of a route and throws an exception when it fails.
- `Result<float> TryCalculate(...)`: Tries to calculate a weight of a route and returns an error object when it fails.
- `float[][] Calculate(...)`: Calculates n x m routes and throws an exception if any fails.
- `Result<float[][]> TryCalculate(...)`: Tries to calculate n x m routes and returns an error object when it fails.
- `float[][] CalculateWeight(...)`: Calculates many-to-many n x m weights and throws an exception if any fails.
- `Result<float[][]> TryCalculateWeight(...)`: Tries to calculate n x m weights and returns an error object when it fails.
- `bool CheckConnectivity(...)`: Checks for connectivity.
- `Result<bool> TryCheckConnectivity(...)`: Tries to check for connectivity.
