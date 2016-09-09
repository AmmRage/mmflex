using MMF.Bone;

namespace MMF.Motion
{
    public interface IMovable
    {
        /// <summary>
        /// Want to move the bones
        /// </summary>
        ISkinningProvider Skinning { get; }

        /// <summary>
        /// Want to run Promotion Manager
        /// </summary>
        IMotionManager MotionManager { get; }

        /// <summary>
        /// Used when assigning per-frame motion of a moving target。
        /// </summary>
        void ApplyMove();
    }
}
