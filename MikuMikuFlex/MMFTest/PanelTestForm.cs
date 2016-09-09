using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MMF.Grid;
using MMF.Matricies.Camera.CameraMotion;
using SlimDX;

namespace CGTest
{
    public partial class PanelTestForm : Form
    {
        public PanelTestForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.leftTop.Initialize();
            BasicGrid grid=new BasicGrid();
            grid.Load(this.leftTop.RenderContext);
            this.leftTop.WorldSpace.AddResource(grid);
            this.rightTop.Initialize(this.leftTop.RenderContext);
            this.rightBottom.Initialize(this.leftTop.RenderContext);
            this.leftBottom.Initialize(this.leftTop.RenderContext);
            this.rightTop.WorldSpace = this.leftTop.WorldSpace;
            this.rightBottom.WorldSpace = this.leftTop.WorldSpace;
            this.leftBottom.WorldSpace = this.leftTop.WorldSpace;
            this.leftTop.ScreenContext.CameraMotionProvider=new BasicCameraControllerMotionProvider(this.leftTop,this);
            this.rightTop.ScreenContext.CameraMotionProvider=new SideCameraMotionProvider(this.leftTop.ScreenContext.CameraMotionProvider,Quaternion.RotationAxis(new Vector3(0,1,0),(float) (Math.PI) ));
            this.leftBottom.ScreenContext.CameraMotionProvider = new SideCameraMotionProvider(this.leftTop.ScreenContext.CameraMotionProvider, Quaternion.RotationAxis(new Vector3(0, 1, 0), (float)(Math.PI / 2)));
            this.rightBottom.ScreenContext.CameraMotionProvider = new SideCameraMotionProvider(this.leftTop.ScreenContext.CameraMotionProvider, Quaternion.RotationAxis(new Vector3(0, 1, 0), -(float)(Math.PI / 2)));
            ControlForm form=new ControlForm(this.leftTop.RenderContext, this.leftTop.ScreenContext, this.leftTop.ScreenContext);
            form.Show();
        }

        public void Render()
        {
            this.leftTop.Render();
            this.rightTop.Render();
            this.rightBottom.Render();
            this.leftBottom.Render();
        }
    }
}
