using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.Matricies.Camera;
using MMF.Model.Shape.Overlay;
using SlimDX;
using SlimDX.DirectInput;

namespace MMF.Model.Controller.ControllerComponent
{
    class RotateRingController:OverlaySilinderShape
    {
        private readonly DragControlManager dragController;


        public EventHandler<RotationChangedEventArgs> OnRotated = delegate { }; 

        private bool lastState;

        private bool lastMouseState;

        private bool isDragging;

        private Point lastPoint;

        public RotateRingController(RenderContext context,ILockableController parent, Vector4 color, Vector4 overlayColor, SilinderShapeDescription desc) : base(context, color, overlayColor, desc)
        {
            this.dragController=new DragControlManager(parent);
        }

        public override void HitTestResult(bool result, bool mouseState, Point mousePosition)
        {

            base.HitTestResult(this.dragController.checkNeedHighlight(result), mouseState, mousePosition);
            this.dragController.checkBegin(result,mouseState,mousePosition);
            if (this.dragController.IsDragging)
            {
                float t = calculateLength(this.dragController.Delta);
                var a = Vector3.TransformNormal(Vector3.UnitY, this.Transformer.LocalTransform);
                a.Normalize();
                this.OnRotated(this,new RotationChangedEventArgs(a,t));
            }
            this.dragController.checkEnd(result,mouseState,mousePosition);
        }

        private float calculateLength(Vector2 delta)//the amount of displacement of the screen coordinates of the mouse at the Delta
        {
            CameraProvider cp = this.RenderContext.MatrixManager.ViewMatrixManager;
            Vector3 transformedAxis = Vector3.TransformNormal(Vector3.UnitY,//UnitY=(0,1,0)
                this.Transformer.LocalTransform*cp.ViewMatrix);//Cylinder seen from a camera Center-axis vector。
                                                     //Transformer.Transformはモデルのローカル変形行列,
                                                     //CP.ViewMatrixはカメラのビュー変換行列
            Vector3 cp2la =cp.CameraLookAt-cp.CameraPosition;//Pull the camera from the camera's gaze, the eye vector
            //Z vector in depth on the screen
            cp2la.Normalize();//Normalization
            Vector3 transformUnit = Vector3.Cross(Vector3.UnitZ, transformedAxis);
            //Because it is seen from a camera (0,0,1)とシリンダの中心軸ベクトルの外積によって求まるベクトルが
            //As when to raise or lower the value for this cylinder direction。
            transformUnit.Normalize(); //Normalization

            Vector3 xUnit = Vector3.Cross(Vector3.UnitZ, Vector3.TransformNormal(cp.CameraUpVec,cp.ViewMatrix));//On the camera's direction vector and vector eye outside the product seeking,
                                                                 //With x-axis of the screen on the current camera position in 3D space
                                                                 //Ask for or represented in any vector
            xUnit.Normalize();//Normalization
            Vector3 yUnit = Vector3.Cross(xUnit, Vector3.UnitZ);//xUnit and cp2la y axis more on-screen in 3D space which vector
                                                        //Seek be transferred?。|xUnit|=|cp2la|=1のため、正規化は不要
            Vector3 deltaInDim3 = xUnit*delta.X + yUnit*delta.Y;//Find vectors on the space vector of mouse 3 CCG。
            return Vector3.Dot(deltaInDim3, transformUnit);//In the mouse move and direction vectors for the top and bottom of the cylinder value
                                                          //Include how much you ask for within to multiply the amount of displacement relative to this。
        }

        public class RotationChangedEventArgs:EventArgs
        {
            private float length;

            private Vector3 axis;

            public RotationChangedEventArgs(Vector3 axis, float length)
            {
                this.axis = axis;
                this.length = length;
            }

            public Vector3 Axis
            {
                get { return this.axis; }
            }

            public float Length
            {
                get { return this.length; }
            }
        }
    }
}
