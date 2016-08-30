using System;
using SlimDX;

namespace MMF.Matricies.Projection
{
    /// <summary>
    ///     Projection matrix interface
    /// </summary>
    public interface IProjectionMatrixProvider
    {
        /// <summary>
        ///     Projection matrix
        /// </summary>
        Matrix ProjectionMatrix { get; }

        /// <summary>
        ///     Viewing angle
        /// </summary>
        float Fovy { get; set; }

        /// <summary>
        ///     Aspect ratio
        /// </summary>
        float AspectRatio { get; set; }

        /// <summary>
        ///     Near clipping plane
        /// </summary>
        float ZNear { get; set; }

        /// <summary>
        ///     Far clipping
        /// </summary>
        float ZFar { get; set; }

        /// <summary>
        ///     To initialize the projection matrix
        /// </summary>
        /// <param name="fovyAngle">Early viewing angle</param>
        /// <param name="aspect">Initial aspect ratio</param>
        /// <param name="znear">Early near clipping plane</param>
        /// <param name="zfar">Early far clipping</param>
        void InitializeProjection(float fovyAngle, float aspect, float znear, float zfar);

        /// <summary>
        ///     To indicate the projection matrix has changed
        /// </summary>
        event EventHandler<ProjectionMatrixChangedEventArgs> ProjectionMatrixChanged;
    }
}