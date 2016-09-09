using System;
using SlimDX;

namespace MMF.Model
{
    /// <summary>
    /// Drawable resources interface
    /// </summary>
    public interface IDrawable : IDisposable
    {

        /// <summary>
        ///     To determine whether the listed variables
        /// </summary>
        bool Visibility { get; set; }

        /// <summary>
        /// Variable to hold the file name
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// The number of subsets
        /// </summary>
        int SubsetCount { get; }

        /// <summary>
        /// Number of vertices
        /// </summary>
        int VertexCount { get; }

        /// <summary>
        ///     When you move the model classes
        /// </summary>
        ITransformer Transformer { get; }

        /// <summary>
        ///     When you draw the model call。
        /// </summary>
        void Draw();

        /// <summary>
        ///     Called when the model is updated
        /// </summary>
        void Update();

        /// <summary>
        /// Self shadow color
        /// </summary>
        Vector4 SelfShadowColor { get; set; }

        /// <summary>
        /// Ground shadow color
        /// </summary>
        Vector4 GroundShadowColor { get; set; }
    }
}