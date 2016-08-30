using System;
using SlimDX;
using SlimDX.Direct3D11;

namespace MMF.Matricies.Camera
{
    /// <summary>
    ///     Camera control base class
    /// </summary>
    public abstract class CameraProvider
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="cameraPos">The initial camera position</param>
        /// <param name="lookAtPos">The initial camera position</param>
        /// <param name="upVec">Early on the camera's direction vector</param>
        public CameraProvider(Vector3 cameraPos, Vector3 lookAtPos, Vector3 upVec)
        {
            this.CameraPosition = cameraPos;
            this.CameraLookAt = lookAtPos;
            this.CameraUpVec = upVec;
        }

        /// <summary>
        ///     Camera coordinates
        /// </summary>
        public abstract Vector3 CameraPosition { get; set; }

        /// <summary>
        ///     Camera gazing point coordinates
        /// </summary>
        public abstract Vector3 CameraLookAt { get; set; }

        /// <summary>
        ///     On the camera's direction vector
        /// </summary>
        public abstract Vector3 CameraUpVec { get; set; }

        /// <summary>
        ///     The view matrix
        /// </summary>
        public abstract Matrix ViewMatrix { get; }

        /// <summary>
        ///     To register effect
        /// </summary>
        /// <param name="effect"></param>
        public virtual void SubscribeToEffect(Effect effect)
        {
            effect.GetVariableBySemantic("CAMERAPOSITION").AsVector().Set(this.CameraPosition);
        }

        /// <summary>
        ///     To notify the view matrix has been updated
        /// </summary>
        public abstract event EventHandler<CameraMatrixChangedEventArgs> CameraMatrixChanged;
    }
}