using System.Windows.Forms;
using MMF.Motion;
using SlimDX;

namespace MMF.DeviceManager
{
    /// <summary>
    /// Primarily a class on a Panel, such as a mouse movement
    /// MMEFor effects
    /// </summary>
    public class PanelObserver
    {
        public Vector2 MousePosition { get; private set; }

        public Vector4 LeftMouseDown { get; private set; }

        public Vector4 MiddleMouseDown { get; private set; }

        public Vector4 RightMouseDown { get; private set; }

        /// <summary>
        /// Monitor controls
        /// </summary>
        private Control RelatedControl { get; set; }

        /// <summary>
        /// MMEThe mouse function is enabled or not
        /// </summary>
        public bool IsMMEMouseEnable { get; set; }

        public PanelObserver(Control control)
        {
            this.RelatedControl = control;
            this.IsMMEMouseEnable = false;
            control.MouseMove += MouseHandler;
            control.MouseDown += MouseHandler;
        }

        void MouseHandler(object sender, MouseEventArgs e)
        {
            if (this.IsMMEMouseEnable)
            {
                float x = 0, y = 0;
                float leftT= this.LeftMouseDown.W, middleT= this.MiddleMouseDown.W, rightT= this.RightMouseDown.W;
                float leftP = 0f, middleP = 0f, rightP = 0f;
                if (e.X!=0)
                {
                    x = (float)e.X*2f/(float) this.RelatedControl.Width - 1f;
                }
                if (e.Y != 0)
                {
                    y = (float)e.Y * 2f / (float) this.RelatedControl.Height - 1f;
                }
                this.MousePosition = new Vector2(x, y);
                if (e.Button.HasFlag(MouseButtons.Left))
                {
                    leftT = MotionTimer.stopWatch.ElapsedMilliseconds/1000f;
                    leftP = 1;
                }
                if (e.Button.HasFlag(MouseButtons.Middle))
                {
                    middleT = MotionTimer.stopWatch.ElapsedMilliseconds/1000f;
                    middleP = 1;
                }
                if (e.Button.HasFlag(MouseButtons.Right))
                {
                    rightT = MotionTimer.stopWatch.ElapsedMilliseconds/1000f;
                    rightP = 1;
                }
                this.LeftMouseDown=new Vector4(x,y,leftP,leftT);
                this.MiddleMouseDown = new Vector4(x, y, middleP, middleT);
                this.RightMouseDown = new Vector4(x, y, rightP, rightT);
            }
        }
    }
}
