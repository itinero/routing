using Itinero.Profiles.Lua.Interop;
//using Itinero.Profiles.Lua.Platforms;

namespace Itinero.Profiles.Lua
{
	/// <summary>
	/// Class containing script global options, that is options which cannot be customized per-script.
	/// <see cref="Script.GlobalOptions"/>
	/// </summary>
	public class ScriptGlobalOptions
	{
		internal ScriptGlobalOptions()
		{
			CustomConverters = new CustomConvertersCollection();
		}

		/// <summary>
		/// Gets or sets the custom converters.
		/// </summary>
		public CustomConvertersCollection CustomConverters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether interpreter exceptions should be 
        /// re-thrown as nested exceptions.
        /// </summary>
        public bool RethrowExceptionNested { get; set; }
	}
}
