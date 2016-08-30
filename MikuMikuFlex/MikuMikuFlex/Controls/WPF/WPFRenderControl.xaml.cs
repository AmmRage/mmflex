using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Assimp;
using MMF.DeviceManager;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Size = System.Drawing.Size;

namespace MMF.Controls.WPF
{
    /// <summary>
    ///     Interaction logic for WPFRenderControl.xaml
    /// </summary>
    public partial class WPFRenderControl : UserControl, IDisposable
    {
        public bool KeepAspectRatio = false;

        public WPFRenderControl()
        {
            InitializeComponent();
        }

        public WPFRenderControl(RenderContext context)
        {
            this.RenderContext = context;
            InitializeComponent();
        }

        /// <summary>
        ///     Render context
        /// </summary>
        public RenderContext RenderContext { get; private set; }

        /// <summary>
        ///     Texture rendering context
        /// </summary>
        public TextureTargetContext TextureContext { get; private set; }

        public event EventHandler Render=delegate{};

        /// <summary>
        ///     This controls the world space
        /// </summary>
        public WorldSpace WorldSpace
        {
            get { return this.TextureContext.WorldSpace; }
            set { this.TextureContext.WorldSpace = value; }
        }

        /// <summary>
        ///     Background clear color
        /// </summary>
        public new Color4 Background
        {
            get { return this.TextureContext.BackgroundColor; }
            set { this.TextureContext.BackgroundColor = value; }
        }

        /// <summary>
        /// </summary>
        protected D3DImageContainer ImageContainer { get; private set; }

        public virtual void Dispose()
        {
        }

        protected virtual RenderContext getRenderContext()
        {
            RenderContext returnValue = RenderContext.CreateContext();
            return returnValue;
        }

        protected virtual TextureTargetContext GetTextureTargetContext()
        {
            //Initial size is funny, so for the time being 100,100を与えて、最初にリサイズイベントでリサイズする
            return new WPFTargetTextureContext(this.RenderContext, new Size(100, 100), new SampleDescription(1, 0));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (this.RenderContext == null) this.RenderContext = getRenderContext();
            this.TextureContext = GetTextureTargetContext();
            //TextureContext.BackgroundColor=new Color4(1,0,1,1);
            this.RenderContext.UpdateRequireWorlds.Add(this.TextureContext.WorldSpace);
            this.ImageContainer = new D3DImageContainer();
            this.ImageContainer.IsFrontBufferAvailableChanged += ImageContainer_IsFrontBufferAvailableChanged;
            this.TargetImg.Source = this.ImageContainer;
            this.ImageContainer.SetBackBufferSlimDX(this.TextureContext.RenderTarget);
            this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            BeginRenderingScene();
            //////Test code
            //BasicGrid grid = new BasicGrid();
            //grid.Load(RenderContext);
            //TextureContext.WorldSpace.AddResource(grid);
            //TextureContext.MatrixManager.ViewMatrixManager.CameraPosition = new Vector3(0, 20, -40);
            //TextureContext.MatrixManager.ViewMatrixManager.CameraLookAt = new Vector3(0, 15, 0);
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            this.RenderContext.UpdateRequireWorlds.Remove(this.WorldSpace);

            if (this.ImageContainer != null)
            {
                this.ImageContainer.Dispose();
                this.ImageContainer = null;
            }
            if (this.TextureContext != null)
            {
                this.TextureContext.Dispose();
                this.TextureContext = null;
            }
            Dispose();
        }

        private void BeginRenderingScene()
        {
            if (this.ImageContainer.IsFrontBufferAvailable)
            {
                Texture2D texture = this.TextureContext.RenderTarget;
                this.ImageContainer.SetBackBufferSlimDX(texture);
                CompositionTarget.Rendering += Rendering;
            }
        }

        private void StopRenderingScene()
        {
            CompositionTarget.Rendering -= Rendering;
        }

        private void Rendering(object sender, EventArgs e)
        {
            if (this.TextureContext == null || this.ImageContainer == null) return;
            this.TextureContext.Render();
            this.ImageContainer.InvalidateD3DImage();
            Render(this,new EventArgs());
        }

        private void ImageContainer_IsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.ImageContainer.IsFrontBufferAvailable)
            {
                BeginRenderingScene();
            }
            else
            {
                StopRenderingScene();
            }
        }

        private void WPFRenderControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.TextureContext != null) this.TextureContext.Size = new Size((int) e.NewSize.Width, (int) e.NewSize.Height);
            if (this.ImageContainer != null && this.TextureContext != null)
                this.ImageContainer.SetBackBufferSlimDX(this.TextureContext.RenderTarget);
            if (!this.KeepAspectRatio && this.TextureContext != null)
                this.TextureContext.MatrixManager.ProjectionMatrixManager.AspectRatio =
                    (float) (e.NewSize.Width/e.NewSize.Height);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if(e.LeftButton==MouseButtonState.Pressed) this.TextureContext.HitChecker.IsMouseDown = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.LeftButton == MouseButtonState.Released) this.TextureContext.HitChecker.IsMouseDown = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var pos=e.GetPosition(this);
            this.TextureContext.HitChecker.CheckPoint=new System.Drawing.Point((int) pos.X,(int) pos.Y);
        }
    }
}