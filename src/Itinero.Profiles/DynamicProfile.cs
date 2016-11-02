using System;
using MoonSharp.Interpreter;
using Itinero.Attributes;

namespace Itinero.Profiles
{
    public class DynamicProfile
    {
        private readonly Script _script;
        private readonly string[] _vehicleTypes;

        public DynamicProfile(string script)
        {
            _script = new Script();
            _script.DoString(script);

            _attributesTable = new Table(_script);
            _resultsTable = new Table(_script);
        }

        private readonly Table _attributesTable;
        private readonly Table _resultsTable;

        public Func<IAttributeCollection, DynamicFactorAndSpeed> GetGetFactorAndSpeed()
        {
            return (attributes) =>
            {
                lock (_script)
                {
                    // build lua table.
                    _attributesTable.Clear();
                    foreach (var attribute in attributes)
                    {
                        _attributesTable.Set(attribute.Key, DynValue.NewString(attribute.Value));
                    }

                    // call factor_and_speed function.
                    _resultsTable.Clear();
                    var function = _script.Globals["factor_and_speed"];
                    _script.Call(function, _attributesTable, _resultsTable);

                    // get the results.
                    var result = new DynamicFactorAndSpeed();
                    float val;
                    if (!_resultsTable.TryGetFloat("factor", out val))
                    {
                        val = 0;
                    }
                    result.Factor = val;

                    return result;
                }
            };
        }
    }
}
