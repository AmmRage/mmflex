using System;
using SlimDX;

namespace MMF.Matricies.Projection
{
    /// <summary>
    ///     A general projection matrix management class
    /// </summary>
    public class BasicProjectionMatrixProvider : IProjectionMatrixProvider
    {
        private float aspectRatio = 1.618f;
        private float fovy;
        private Matrix projectionMatrix = Matrix.Identity;
        private float zFar;
        private float zNear;

        /// <summary>
        ///     Projection matrix
        /// </summary>
        public Matrix ProjectionMatrix
        {
            get { return this.projectionMatrix; }
        }

        /// <summary>
        ///     Viewing angle (in radians)
        /// </summary>
        public float Fovy
        {
            get { return this.fovy; }
            set
            {
                this.fovy = value;
                UpdateProjection();
                NotifyProjectMatrixChanged(ProjectionMatrixChangedVariableType.Fovy);
            }
        }

        /// <summary>
        ///     Aspect ratio
        /// </summary>
        public float AspectRatio
        {
            get { return this.aspectRatio; }
            set
            {
                this.aspectRatio = value;
                UpdateProjection();
                NotifyProjectMatrixChanged(ProjectionMatrixChangedVariableType.AspectRatio);
            }
        }

        /// <summary>
        ///     Near clipping plane
        /// </summary>
        public float ZNear
        {
            get { return this.zNear; }
            set
            {
                this.zNear = value;
                UpdateProjection();
                NotifyProjectMatrixChanged(ProjectionMatrixChangedVariableType.ZNear);
            }
        }

        /// <summary>
        ///     Far clipping
        /// </summary>
        public float ZFar
        {
            get { return this.zFar; }
            set
            {
                this.zFar = value;
                UpdateProjection();
                NotifyProjectMatrixChanged(ProjectionMatrixChangedVariableType.ZFar);
            }
        }

        /// <summary>
        ///     To initialize the projection matrix
        /// </summary>
        /// <param name="fovyAngle">Early viewing angle</param>
        /// <param name="aspect">Initial aspect ratio</param>
        /// <param name="znear">Early near clipping plane</param>
        /// <param name="zfar">Early far clipping</param>
        public void InitializeProjection(float fovyAngle, float aspect, float znear, float zfar)
        {
            this.fovy = fovyAngle;
            this.aspectRatio = aspect;
            this.zNear = znear;
            this.zFar = zfar;
            UpdateProjection();
        }

        public event EventHandler<ProjectionMatrixChangedEventArgs> ProjectionMatrixChanged;

        /// <summary>
        ///     To update the projection matrix
        /// </summary>
        private void UpdateProjection()
        {
            this.projectionMatrix = Matrix.PerspectiveFovLH(this.fovy, this.aspectRatio, this.zNear, this.zFar);
        }

        /// <summary>
        ///     To indicate the projection matrix has changed
        /// </summary>
        /// <param name="type">Parameters changed</param>
        private void NotifyProjectMatrixChanged(ProjectionMatrixChangedVariableType type)
        {
            if (ProjectionMatrixChanged != null)
                ProjectionMatrixChanged(this, new ProjectionMatrixChangedEventArgs(type));
        }
    }
}