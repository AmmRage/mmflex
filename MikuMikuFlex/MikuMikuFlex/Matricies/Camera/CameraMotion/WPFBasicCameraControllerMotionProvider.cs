using System.Drawing;
using System.Windows.Controls;
using System.Windows.Input;
using MMF.Matricies.Projection;
using SlimDX;

namespace MMF.Matricies.Camera.CameraMotion
{
    public class WPFBasicCameraControllerMotionProvider:ICameraMotionProvider
    {
        public bool isLocked;

        private Vector3 xAxis=new Vector3(1,0,0);
        private Vector3 yAxis=new Vector3(0,1,0);
        private Control control;

        public WPFBasicCameraControllerMotionProvider(Control control,float initialDistance=45f)
        {
            this.distance = initialDistance;
            this.cameraPositionRotation = Quaternion.Identity;
            this.control = control;
            control.MouseDown += panel_MouseDown;
            control.MouseMove += panel_MouseMove;
            control.MouseUp += panel_MouseUp;
            control.MouseWheel += wheelRevieveControl_MouseWheel;
            this.MouseWheelSensibility = 2.0f;
            this.RightButtonRotationSensibility = 5f;
            this.MiddleButtonTranslationSensibility = 1f;
        }

        void wheelRevieveControl_MouseWheel(object sender, MouseWheelEventArgs mouseWheelEventArgs)
        {
            if(this.isLocked)return;
            if (mouseWheelEventArgs.Delta > 0)
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
        protected Quaternion cameraPositionRotation { get; set; }

        protected Vector2 cameraLookatTranslation { get; set; }

        protected Vector3 cameraLookatTranslationOfWorld { get; set; }

        protected float distance { get; set; }

        public float MouseWheelSensibility { get; set; }

        public float RightButtonRotationSensibility { get; set; }

        public float MiddleButtonTranslationSensibility { get; set; }

        private void panel_MouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (this.isLocked) return;
            int x = (int) (mouseEventArgs.GetPosition(this.control).X - this.LastMousePosition.X);
            int y = (int) (mouseEventArgs.GetPosition(this.control).Y - this.LastMousePosition.Y);
            if (this.isRightMousePushed)
            {
//About the amount of rotation log in
                this.cameraPositionRotation *= Quaternion.RotationAxis(this.yAxis, this.RightButtonRotationSensibility/1000f*x)*
                                          Quaternion.RotationAxis(this.xAxis, this.RightButtonRotationSensibility/1000f*(-y));
                this.cameraPositionRotation.Normalize();
            }
            if (this.isMiddleMousePushed)
            {//Record changes in the amount of
                this.cameraLookatTranslation+=new Vector2(x,y)*this.MiddleButtonTranslationSensibility/100f;
            }
            this.LastMousePosition = new Point((int) mouseEventArgs.GetPosition(this.control).X, (int) mouseEventArgs.GetPosition(this.control).Y);
        }


        private void panel_MouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.RightButton==MouseButtonState.Released) this.isRightMousePushed = false;
            if (mouseButtonEventArgs.MiddleButton==MouseButtonState.Released) this.isMiddleMousePushed = false;
        }

        private void panel_MouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.RightButton == MouseButtonState.Pressed) this.isRightMousePushed = true;
            if (mouseButtonEventArgs.MiddleButton == MouseButtonState.Pressed) this.isMiddleMousePushed = true;
        }


        public virtual void UpdateCamera(CameraProvider cp1, IProjectionMatrixProvider proj)
        {
            Vector3 cp2la = Vector3.TransformCoordinate(new Vector3(0, 0, 1),
                Matrix.RotationQuaternion(this.cameraPositionRotation));
            this.xAxis = Vector3.Cross(cp2la, cp1.CameraUpVec);
            this.xAxis.Normalize();
            this.yAxis = Vector3.Cross(this.xAxis, cp2la);
            this.yAxis.Normalize();
            cp1.CameraLookAt += this.xAxis*this.cameraLookatTranslation.X + this.yAxis*this.cameraLookatTranslation.Y;
            cp1.CameraLookAt += this.cameraLookatTranslationOfWorld;
            cp1.CameraPosition = cp1.CameraLookAt + this.distance*(-cp2la);
            this.cameraLookatTranslation = Vector2.Zero;
            this.cameraLookatTranslationOfWorld = Vector3.Zero;
        }
    }
}
