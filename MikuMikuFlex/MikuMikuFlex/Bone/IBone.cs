using SlimDX;

namespace MMF.Bone
{
    public interface IBone
    {
        /// <summary>
        ///     Parallel movement position vector
        /// </summary>
        Vector3 Translation { get; set; }

        /// <summary>
        ///     Rotation matrix
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        ///     Global pose
        /// </summary>
        Matrix GlobalPose { get; }

        void UpdateGrobalPose();
    }
}