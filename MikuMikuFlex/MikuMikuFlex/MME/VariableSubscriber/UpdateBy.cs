namespace MMF.MME.VariableSubscriber
{
    /// <summary>
    ///     Variables are what is updated every
    /// </summary>
    public enum UpdateBy
    {
        /// <summary>
        ///     This variable is updated separately for each material
        /// </summary>
        Material,

        /// <summary>
        ///     This variable is updated for each model
        /// </summary>
        Model
    }
}