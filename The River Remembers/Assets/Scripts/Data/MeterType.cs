namespace ConNuoc.Data
{
    /// <summary>
    /// The three systems-map meters from the GDD.
    /// Floodwater itself is NOT a meter — it's shown visually on the map (tile/water state),
    /// driven by SeasonManager, not tracked here.
    /// </summary>
    public enum MeterType
    {
        Ecology,
        Trust,
        Resources
    }
}
