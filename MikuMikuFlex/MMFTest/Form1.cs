#define VSG_DEBUG
#region

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CGTest.Properties;
using MMF;
using MMF.Controls.Forms;
using MMF.DeviceManager;
using MMF.Grid;
using MMF.Matricies.Camera.CameraMotion;
using MMF.Model.Controller;
using MMF.Model.Shape;
using MMF.Model.Shape.Overlay;
using MMF.Sprite;
using MMF.Sprite.D2D;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DirectWrite;
using SlimDX.DXGI;
using Brush = SlimDX.Direct2D.Brush;

#endregion

namespace CGTest
{
    public partial class Form1 : D2DSupportedRenderForm
    {
        public static TransformController Controller;

        // private Brush brush;
        //private D2DSpriteTextformat _format;
        //private IDynamicTexture _tex2;
        //private ShaderResourceView _resourceView;
        private float _t = 0;
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.WorldSpace.AddDrawableGroup(new ControllerDrawableGroup(1,"Controller", this.RenderContext));
            this.KeyPreview = true;
            //BasicGrid grid=new BasicGrid();
            //grid.Load(RenderContext);
            Controller=new TransformController(this.RenderContext, this.ScreenContext.HitChekcer);
            this.WorldSpace.AddResource(Controller,"Controller");

            //If (!String.IsNullOrEmpty(Settings.Default.InitLoadModel))
            //{
            //    MMDModel model = MMDModelWithPhysics.OpenLoad(Settings.Default.InitLoadModel, RenderContext);
            //    WorldSpace.AddResource(model);
            //    if (!String.IsNullOrEmpty(Settings.Default.InitLoadMotion))
            //    {
            //        model.MotionManager.ApplyMotion(model.MotionManager.AddMotionFromFile(Settings.Default.InitLoadMotion, false));
            //    }
            //}
            //World spaces that need to be updated and added to UpdateRequireWorlds must be b。
            //PlaneBoard bill = new PlaneBoard(RenderContext, resourceView, new Vector2(800, 800));
            //WorldSpace.AddResource(bill);
            //Bill.Transformer.Position = new Vector3(0, 0, 20);

            //format = SpriteBatch.CreateTextformat("Meiriyo", 30, FontWeight.ExtraBold);
            //brush = SpriteBatch.CreateRadialGradientBrush(g,
            //    new RadialGradientBrushProperties() {CenterPoint = new PointF(100,100),GradientOriginOffset = new PointF(0,0),HorizontalRadius = 100f,VerticalRadius = 200f}).Brush;
            //brush = SpriteBatch.CreateSolidColorBrush(Color.Aquamarine);

            this.ScreenContext.CameraMotionProvider = new BasicCameraControllerMotionProvider(this, this);
            
            BasicGrid gird = new BasicGrid {Visibility = true};
            gird.Load(this.RenderContext);

            this.ScreenContext.WorldSpace.AddResource(gird);
            //be sure to add child window into the world of texture。
            ControlForm controlForm = new ControlForm(this.RenderContext, this.ScreenContext, null)
            {
                Size = this.Size,
                StartPosition = this.StartPosition,
            };
            controlForm.Show(this);

            //OpenFileDialog ofd = new OpenFileDialog();
            //If (ofd.ShowDialog() == DialogResult.OK)
            //{
            //    VMDCameraMotionProvider camMotion=VMDCameraMotionProvider.OpenFile(ofd.FileName);
            //    ScreenContext.CameraMotionProvider = camMotion;
            //    camMotion.Start();
            //}
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = false;
        }

        protected override void RenderSprite()
        {
            
        }

        public override void OnUpdated()
        {
            base.OnUpdated();
        }

        ///// <summary>
        /////     Sprite drawing here
        ///// </summary>
        //protected override void RenderSprite()
        //{

        //    //SpriteBatch.DWRenderTarget.FillRectangle(brush,SpriteBatch.FillRectangle);
        //    //SpriteBatch.DWRenderTarget.DrawText(
        //    //    string.Format("FPS:{0}\n\nBrush Test!!", FpsCounter.FPS.ToString("####.#")), format,
        //    //    SpriteBatch.FillRectangle, brush);
        //}

        protected override void OnPresented()
        {
            base.OnPresented();
            this.RenderContext.LightManager.Position = new Vector3((float)Math.Cos(this._t),0, (float)Math.Sin(this._t)) * 10f;
            this._t += 0.0001f;
            //If (form.Visible)form.Render();
        }
    }
}