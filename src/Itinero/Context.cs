namespace Itinero
{
    /// <summary>
    /// Holds static dependencies used throughout Itinero.
    /// </summary>
    /// <remarks>
    /// Default implementations are all portable, but they may be overridden by
    /// the application to provide optimized non-portable variants.
    /// </remarks>
    public static class Context
    {
        /// <summary>
        /// The <see cref="IArrayFactory"/> used to create large arrays.
        /// </summary>
        public static IArrayFactory ArrayFactory { get; set; } = new DefaultArrayFactory();
    }
}
