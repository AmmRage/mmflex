using MMF.Model;
using MMF.Morph;

namespace MMF.Bone
{
    public interface ITransformUpdater
    {
        /// <summary>
        /// Including the changes in the amount of bone, allocated to bone
        /// </summary>
        /// <returns>Whether or not or not immediately generate value based on matrix</returns>
        bool UpdateTransform();
    }
}