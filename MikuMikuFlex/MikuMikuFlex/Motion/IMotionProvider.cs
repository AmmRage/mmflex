using System;
using MMF.Bone;
using MMF.Morph;

namespace MMF.Motion
{
    public interface IMotionProvider
    {
        /// <summary>
        /// Whether or not the attached
        /// </summary>
        bool IsAttached { get; }

        /// <summary>
        ///     The current frame
        /// </summary>
        float CurrentFrame { get; set; }

        /// <summary>
        ///     Last frame of this motion
        /// </summary>
        int FinalFrame { get;}

        /// <summary>
        /// To attach a motion
        /// </summary>
        /// <param name="bones">Bone matrix</param>
        void AttachMotion(PMXBone[] bones);

        /// <summary>
        ///     Motion play。
        /// </summary>
        /// <param name="frame">The first play frame</param>
        /// <param name="action">After the motion behavior</param>
        void Start(float frame, ActionAfterMotion action);


        /// <summary>
        ///     Stop motion。
        /// </summary>
        void Stop();

        /// <summary>
        /// Advances one frame motion。
        /// </summary>
        /// <param name="fps">Seconds between frames</param>
        /// <param name="elapsedTime">Just how long it took from the last frame[秒]</param>
        /// <param name="morphManager">Morph management class</param>
        void Tick(int fps, float elapsedTime, IMorphManager morphManager);

        /// <summary>
        ///     Indicates that the motion was。
        /// </summary>
        event EventHandler<ActionAfterMotion> MotionFinished;

        event EventHandler<EventArgs> FrameTicked;
    }

    /// <summary>
    ///     Enumeration specifies the behavior of a motion play finish after
    /// </summary>
    public enum ActionAfterMotion
    {
        Nothing,
        Replay
    }
}