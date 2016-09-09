using System.Collections.Generic;
using MMF.Motion;

namespace MMF.Morph
{
    /// <summary>
    /// Morph managed class interface
    /// </summary>
    public interface IMorphManager
    {
        /// <summary>
        /// Motion morph from
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="morphs"></param>
        void ApplyMorphProgress(float frame, IEnumerable<MorphMotion> morphs);

        /// <summary>
        /// Sets the frame of the morph
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="morphName"></param>
        void ApplyMorphProgress(float frame, string morphName);

        /// <summary>
        /// When a frame is reset (call at the time of Update)
        /// </summary>
        void UpdateFrame();


        float getMorphProgress(string morphName);
    }
}