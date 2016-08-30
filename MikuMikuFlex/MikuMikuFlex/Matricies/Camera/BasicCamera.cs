using System;
using SlimDX;
using SlimDX.Direct3D11;

namespace MMF.Matricies.Camera
{
    /// <summary>
    ///     A standard camera control class
    /// </summary>
    public class BasicCamera : CameraProvider
    {
        private Vector3 cameraLookAt = Vector3.Zero;
        private Vector3 cameraPosition = Vector3.Zero;
        private Vector3 cameraUpVec = Vector3.Zero;
        private Matrix viewMatrix = Matrix.Identity;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="cameraPos">Camera default coordinates</param>
        /// <param name="lookAtPos">Camera initial view coordinates</param>
        /// <param name="upVec">Early on the camera's direction vector</param>
        public BasicCamera(Vector3 cameraPos, Vector3 lookAtPos, Vector3 upVec)
            : base(cameraPos, lookAtPos, upVec)
        {
        }

        /// <summary>
        ///     The view matrix is calculated from a member of this class
        /// </summary>
        public override Matrix ViewMatrix
        {
            get { return this.viewMatrix; }
        }


        /// <summary>
        ///     Camera position
        /// </summary>
        public override Vector3 CameraPosition
        {
            get { return this.cameraPosition; }
            set
            {
                this.cameraPosition = value;
                UpdateCamera();
                NotifyCameraMatrixChanged(CameraMatrixChangedVariableType.Position);
            }
        }


        /// <summary>
        ///     Gaze of the camera
        /// </summary>
        public override Vector3 CameraLookAt
        {
            get { return this.cameraLookAt; }
            set
            {
                this.cameraLookAt = value;
                UpdateCamera();
                NotifyCameraMatrixChanged(CameraMatrixChangedVariableType.LookAt);
            }
        }


        /// <summary>
        ///     On the camera direction
        /// </summary>
        public override Vector3 CameraUpVec
        {
            get { return this.cameraUpVec; }
            set
            {
                this.cameraUpVec = value;
                UpdateCamera();
                NotifyCameraMatrixChanged(CameraMatrixChangedVariableType.Up);
            }
        }

        /// <summary>
        ///     To update the view matrix
        /// </summary>
        private void UpdateCamera()
        {
            this.viewMatrix = Matrix.LookAtLH(this.cameraPosition, this.cameraLookAt, this.cameraUpVec);
        }

        /// <summary>
        ///     Register a view matrix effect
        /// </summary>
        /// <param name="effect"></param>
        public override void SubscribeToEffect(Effect effect)
        {
            effect.GetVariableBySemantic("CAMERAPOSITION").AsVector().Set(this.cameraPosition);
        }

        /// <summary>
        ///     Indicates the camera's matrix has changed
        /// </summary>
        /// <param name="type">Changes</param>
        private void NotifyCameraMatrixChanged(CameraMatrixChangedVariableType type)
        {
            if (CameraMatrixChanged != null) CameraMatrixChanged(this, new CameraMatrixChangedEventArgs(type));
        }

        /// <summary>
        ///     Will be notified when the view matrix change
        /// </summary>
        public override event EventHandler<CameraMatrixChangedEventArgs> CameraMatrixChanged;
    }
}