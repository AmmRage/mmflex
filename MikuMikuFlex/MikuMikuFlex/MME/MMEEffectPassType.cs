namespace MMF.MME
{
    /// <summary>
    ///     MMEThe path type
    ///     MME specification 158 near the reference for more details
    /// </summary>
    public enum MMEEffectPassType
    {
        /// <summary>
        ///     object_ss
        /// </summary>
        Object_SelfShadow,

        /// <summary>
        ///     object
        /// </summary>
        Object,

        /// <summary>
        ///     zplot
        /// </summary>
        ZPlot,

        /// <summary>
        ///     shadow
        /// </summary>
        Shadow,

        /// <summary>
        ///     edge(PMDOnly)
        /// </summary>
        Edge
    }
}