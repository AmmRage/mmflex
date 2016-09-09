using System.Collections.Generic;
using MMF.MME.VariableSubscriber;

namespace MMF.MME
{
    /// <summary>
    ///     To register according to the semantics of the effects for the class dictionary
    /// </summary>
    public class EffectSubscriberDictionary : Dictionary<string, SubscriberBase>
    {
        /// <summary>
        ///     Add the key semantics
        /// </summary>
        /// <param name="subs">SubscriberBase register</param>
        public void Add(SubscriberBase subs)
        {
            base.Add(subs.Semantics, subs);
        }
    }
}