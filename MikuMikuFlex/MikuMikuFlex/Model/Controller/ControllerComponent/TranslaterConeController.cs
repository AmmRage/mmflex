using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.Matricies.Camera;
using MMF.Model.Shape.Overlay;
using SlimDX;

namespace MMF.Model.Controller.ControllerComponent
{
    class TranslaterConeController:OverlayConeShape
    {
        private DragControlManager dragController;

        public event EventHandler<TranslatedEventArgs> OnTranslated=delegate{}; 

        public TranslaterConeController(RenderContext context,ILockableController locker, Vector4 color, Vector4 overlayColor) : base(context, color, overlayColor)
        {
            this.dragController=new DragControlManager(locker);
        }

        public override void HitTestResult(bool result, bool mouseState, Point mousePosition)
        {
            base.HitTestResult(this.dragController.checkNeedHighlight(result), mouseState, mousePosition);
            this.dragController.checkBegin(result, mouseState, mousePosition);
            if (this.dragController.IsDragging)
            {
                checkTranslation();
            }
            this.dragController.checkEnd(result, mouseState, mousePosition);
        }

        private void checkTranslation()
        {
            Vector2 delta = this.dragController.Delta;
            CameraProvider cp = this.RenderContext.MatrixManager.ViewMatrixManager;
            Vector3 transformedAxis = Vector3.TransformNormal(Vector3.UnitY,//UnitY=(0,1,0)
                this.Transformer.LocalTransform * cp.ViewMatrix);//Central axis vector cone from the camera。
            //Transformer.Transformはモデルのローカル変形行列,
            //CP.ViewMatrixはカメラのビュー変換行列
            transformedAxis.Normalize();
            Vector3 cp2la = cp.CameraLookAt - cp.CameraPosition;//Pull the camera from the camera's gaze, the eye vector
            //Z vector in depth on the screen
            cp2la.Normalize();//Normalization

            Vector3 xUnit = Vector3.Cross(Vector3.UnitZ, Vector3.TransformNormal(cp.CameraUpVec, cp.ViewMatrix));//On the camera's direction vector and vector eye outside the product seeking,
            //With x-axis of the screen on the current camera position in 3D space
            //Ask for or represented in any vector
            xUnit.Normalize();//Normalization
            Vector3 yUnit = Vector3.Cross(xUnit, Vector3.UnitZ);//xUnit and cp2la y axis more on-screen in 3D space which vector
            //Seek be transferred?。|xUnit|=|cp2la|=1のため、正規化は不要
            Vector3 deltaInDim3 = xUnit * delta.X + yUnit * delta.Y;//Find vectors on the space vector of mouse 3 CCG。
            float dist = -Vector3.Dot(deltaInDim3, transformedAxis)/10f;
            OnTranslated(this,new TranslatedEventArgs(dist*Vector3.TransformNormal(Vector3.UnitY, this.Transformer.LocalTransform)));
        }

        public class TranslatedEventArgs : EventArgs
        {
            private Vector3 translation;

            public TranslatedEventArgs(Vector3 translation)
            {
                this.translation = translation;
            }

            public Vector3 Translation
            {
                get { return this.translation; }
            }
        }
    }
}
