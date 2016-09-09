namespace MMF.MME.VariableSubscriber.ControlInfoSubscriber
{
    /// <summary>
    /// Summarizes the control object annotation
    /// </summary>
    class ControlObjectAnnotation
    {
        public ControlObjectAnnotation(TargetObject target, bool isString)
        {
            this.Target = target;
            this.IsString = isString;
        }

        public TargetObject Target { get; private set; }

        public bool IsString { get; private set; }
    }
}
