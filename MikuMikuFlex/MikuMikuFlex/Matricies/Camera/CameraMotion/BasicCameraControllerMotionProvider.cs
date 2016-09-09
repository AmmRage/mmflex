using System.Drawing;
using System.Windows.Forms;
using MMF.Matricies.Projection;
using SlimDX;

namespace MMF.Matricies.Camera.CameraMotion
{
    public class BasicCameraControllerMotionProvider:ICameraMotionProvider
    {
        private Vector3 xAxis=new Vector3(1,0,0);
        private Vector3 yAxis=new Vector3(0,1,0);

        public BasicCameraControllerMotionProvider(Control control,Control wheelRevieveControl,float initialDistance=45f)
        {
            this.distance = initialDistance;
            this.cameraPositionRotation = Quaternion.Identity;
            control.MouseDown += panel_MouseDown;
            control.MouseMove += panel_MouseMove;
            control.MouseUp += panel_MouseUp;
            wheelRevieveControl.MouseWheel += wheelRevieveControl_MouseWheel;
            this.MouseWheelSensibility = 2.0f;
            this.RightButtonRotationSensibility = 0.005f;
            this.MiddleButtonTranslationSensibility = 0.01f;
        }

        void wheelRevieveControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                this.distance -= this.MouseWheelSensibility;
                if (this.distance <= 0)
                    this.distance = 0.0001f;
            }
            else
            {
                this.distance += this.MouseWheelSensibility;
            }
        }

        /// <summary>
        /// Position of the last mouse
        /// </summary>
        private Point LastMousePosition { get; set; }

        /// <summary>
        /// Or right mouse button is pressed
        /// </summary>
        private bool isRightMousePushed { get; set; }

        /// <summary>
        /// Middle mouse button is pressed?
        /// </summary>
        private bool isMiddleMousePushed { get; set; }

        /// <summary>
        /// The gaze of the camera rotation
        /// </summary>
        private Quaternion cameraPositionRotation { get; set; }

        private Vector2 cameraLookatTranslation { get; set; }

        private float distance { get; set; }

        public float MouseWheelSensibility { get; set; }

        public float RightButtonRotationSensibility { get; set; }

        public float MiddleButtonTranslationSensibility { get; set; }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            int x = e.Location.X - this.LastMousePosition.X;
            int y = e.Location.Y - this.LastMousePosition.Y;
            if (this.isRightMousePushed)
            {
//About the amount of rotation log in
                this.cameraPositionRotation *= Quaternion.RotationAxis(this.yAxis, this.RightButtonRotationSensibility*x)*
                                          Quaternion.RotationAxis(this.xAxis, this.RightButtonRotationSensibility*(-y));
                this.cameraPositionRotation.Normalize();
            }
            if (this.isMiddleMousePushed)
            {//Record changes in the amount of
                this.cameraLookatTranslation+=new Vector2(x,y)*this.MiddleButtonTranslationSensibility;
            }
            this.LastMousePosition = e.Location;
        }


        private void panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) this.isRightMousePushed = false;
            if (e.Button == MouseButtons.Middle) this.isMiddleMousePushed = false;
        }

        private void panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) this.isRightMousePushed = true;
            if (e.Button == MouseButtons.Middle) this.isMiddleMousePushed = true;
        }


        void ICameraMotionProvider.UpdateCamera(CameraProvider cp1, IProjectionMatrixProvider proj)
        {
            Vector3 cp2la = Vector3.TransformCoordinate(new Vector3(0, 0, 1),
                Matrix.RotationQuaternion(this.cameraPositionRotation));
            this.xAxis = Vector3.Cross(cp2la, cp1.CameraUpVec);
            this.xAxis.Normalize();
            this.yAxis = Vector3.Cross(this.xAxis, cp2la);
            this.yAxis.Normalize();
            cp1.CameraLookAt += this.xAxis*this.cameraLookatTranslation.X + this.yAxis*this.cameraLookatTranslation.Y;
            cp1.CameraPosition = cp1.CameraLookAt + this.distance*(-cp2la);
            this.cameraLookatTranslation = Vector2.Zero;
        }
    }
}
