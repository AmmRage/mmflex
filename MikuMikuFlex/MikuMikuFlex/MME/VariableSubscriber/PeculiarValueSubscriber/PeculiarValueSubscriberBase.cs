using SlimDX.Direct3D11;

namespace MMF.MME.VariableSubscriber.PeculiarValueSubscriber
{
    /// <summary>
    ///     Registration of the special parameter base class
    /// </summary>
    public abstract class PeculiarValueSubscriberBase
    {
        /// <summary>
        ///     Variable names are recognized
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     Variable type
        /// </summary>
        public abstract VariableType Type { get; }

        /// <summary>
        ///     To register a value to a variable
        /// </summary>
        /// <param name="subscribeTo">Register variables</param>
        /// <param name="argument">Arguments to use to register</param>
        public abstract void Subscribe(EffectVariable subscribeTo, SubscribeArgument argument);
    }
}