using System;
using MMF.Model;
using SlimDX;

namespace MMF.Matricies.World
{
    public interface IBasicWorldMatrixProvider
    {
        Matrix getWorldMatrix(Matrix localMatrix);
    }

    /// <summary>
    ///     Typical world matrix management class
    /// </summary>
    public class BasicWorldMatrixProvider : IWorldMatrixProvider
    {
        /// <summary>
        ///     Rotation value
        /// </summary>
        private Quaternion rotation;

        /// <summary>
        ///     Larger values
        /// </summary>
        private Vector3 scaling;

        /// <summary>
        ///     Parallel displacement
        /// </summary>
        private Vector3 translation;

        public BasicWorldMatrixProvider()
        {
            this.scaling = new Vector3(1.0f, 1.0f, 1.0f);
            this.rotation = Quaternion.Identity;
            this.translation = Vector3.Zero;
        }

        public Vector3 Scaling
        {
            get { return this.scaling; }
            set
            {
                this.scaling = value;
                NotifyWorldMatrixChanged(new WorldMatrixChangedEventArgs(ChangedWorldMatrixValueType.Scaling));
            }
        }

        public Quaternion Rotation
        {
            get { return this.rotation; }
            set
            {
                this.rotation = value;
                NotifyWorldMatrixChanged(new WorldMatrixChangedEventArgs(ChangedWorldMatrixValueType.Rotation));
            }
        }

        public Vector3 Translation
        {
            get { return this.translation; }
            set
            {
                this.translation = value;
                NotifyWorldMatrixChanged(new WorldMatrixChangedEventArgs(ChangedWorldMatrixValueType.Translation));
            }
        }

        public Matrix getWorldMatrix(Vector3 scalingLocal, Quaternion rotationLocal, Vector3 translationLocal)
        {
            Vector3 scaling = new Vector3(this.Scaling.X*scalingLocal.X, this.Scaling.Y*scalingLocal.Y, this.Scaling.Z*scalingLocal.Z);
            return Matrix.Scaling(scaling)*Matrix.RotationQuaternion(rotationLocal*this.Rotation)*
                   Matrix.Translation(translationLocal + this.Translation);
        }

        public Matrix getWorldMatrix(Matrix localMatrix)
        {
            return Matrix.Scaling(this.scaling)*Matrix.RotationQuaternion(this.rotation)*Matrix.Translation(this.translation)*localMatrix;
        }

        public event EventHandler<WorldMatrixChangedEventArgs> WorldMatrixChanged;


        public Matrix getWorldMatrix(IDrawable drawable)
        {
            return getWorldMatrix(drawable.Transformer.LocalTransform);
        }

        private void NotifyWorldMatrixChanged(WorldMatrixChangedEventArgs arg)
        {
            if (WorldMatrixChanged != null) WorldMatrixChanged(this, arg);
        }
    }
}