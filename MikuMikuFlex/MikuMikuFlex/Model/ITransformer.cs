using SlimDX;

namespace MMF.Model
{
    public interface ITransformer
    {
        /// <summary>
        ///     Represents a position in world coordinates of the model。
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        ///     Represents the direction pointing in world space model。
        /// </summary>
        Vector3 Foward { get; set; }

        /// <summary>
        ///     Represents the orientation of the above in world space。
        /// </summary>
        Vector3 Top { get; set; }

        /// <summary>
        ///     Represents a revolution in the world-space model。
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        ///     Represents the scale of the model
        /// </summary>
        Vector3 Scale { get; set; }

        /// <summary>
        ///     Indicates the direction in the initial state
        /// </summary>
        Vector3 InitialTop { get; }

        /// <summary>
        ///     Represents the direction you're facing in the initial state。
        /// </summary>
        Vector3 InitialFoward { get; }

        /// <summary>
        ///     Position of the model returns to the initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Model transformation matrix
        /// </summary>
        Matrix LocalTransform { get; }
    }
}