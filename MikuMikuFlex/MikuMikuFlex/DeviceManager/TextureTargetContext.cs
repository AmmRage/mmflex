using System;
using System.Drawing;
using System.Windows.Forms;
using MMF.Matricies;
using MMF.Matricies.Camera;
using MMF.Matricies.Projection;
using MMF.Matricies.World;
using MMF.Motion;
using MMF.Utility;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;

namespace MMF.DeviceManager
{
    public class TextureTargetContext : TargetContextBase
    {
        private bool disposed = false;

        private FPSCounter fpsCounter;

        private SampleDescription sampleDesc = new SampleDescription(1, 0);
        /// <summary>
        /// Multimedia sample Gets or sets．
        /// As much as possible，指定された値に近い設定を採用します．
        /// </summary>
        public SampleDescription SampleDesc
        {
            get { return this.sampleDesc; }
            set
            {
                var device = this.Context.DeviceManager.Device;
                Format format = getRenderTargetTexture2DDescription().Format;
                int count = value.Count;
                do
                {
                    int msql = device.CheckMultisampleQualityLevels(format, count);
                    if (msql > 0)
                    {
                        int quality = Math.Min(msql - 1, value.Quality);
                        this.sampleDesc = new SampleDescription(count, quality);
                        break;
                    }

                    // マルチサンプル数がサポートされない場合
                    count--;
                } while (count > 0);
                if(this.size.Width>0&& this.size.Height>0)
                ResetTargets();
            }
        }

        private Size size;
        /// <summary>
        /// Sets or retrieves the size of the render target．
        /// </summary>
        public Size Size
        {
            get { return this.size; }
            set
            {
                if (this.size != value&&value.Width>0&&value.Height>0)
                {
                    this.size = value;
                    ResetTargets();
                }
            }
        }

        /// <summary>
        /// Reset the render target．
        /// </summary>
        private void ResetTargets()
        {
            // ターゲットを破棄
            DisposeTargetViews();
            if(this.depthTarget!=null&&!this.depthTarget.Disposed) this.depthTarget.Dispose();
            if(this.renderTarget!=null&&!this.renderTarget.Disposed) this.renderTarget.Dispose();
            Device device = this.Context.DeviceManager.Device;
            //Initialization of the depth stencil Buffa
            //Initializing render targets
            this.renderTarget = new Texture2D(device, getRenderTargetTexture2DDescription());
            this.RenderTargetView = new RenderTargetView(device, this.RenderTarget);
            //Initialization of the depth stencil Buffa
            this.depthTarget = new Texture2D(device, getDepthBufferTexture2DDescription());
            this.DepthTargetView = new DepthStencilView(device, this.DepthTarget);
            this.HitChecker.Resize(this.Size);
            SetViewport();
        }

        private Texture2D renderTarget;
        /// <summary>
        /// Render ターゲットを取得します．
        /// </summary>
        public Texture2D RenderTarget
        {
            get { return this.renderTarget; }
        }


        private Texture2D depthTarget;
        public TexturedBufferHitChecker HitChecker;

        /// <summary>
        /// Gets the depth target.
        /// </summary>
        public Texture2D DepthTarget
        {
            get { return this.depthTarget; }
        }

        public TextureTargetContext(RenderContext context, Size size, SampleDescription sampleDesc)
            : this(context,
            new MatrixManager(new BasicWorldMatrixProvider(), new BasicCamera(new Vector3(0, 20, -200), new Vector3(0, 3, 0), new Vector3(0, 1, 0)), new BasicProjectionMatrixProvider())
            , size, sampleDesc)
        {
            this.MatrixManager.ProjectionMatrixManager.InitializeProjection((float)Math.PI / 4f, (float)size.Width / size.Height, 1, 2000);
        }

        public TextureTargetContext(RenderContext context, MatrixManager matrixManager, Size size, SampleDescription sampleDesc):base(context)
        {
            this.HitChecker = new TexturedBufferHitChecker(context, this);
            context.Timer=new MotionTimer(context);
            Device device = context.DeviceManager.Device;

            // サイズを設定（ターゲットは初期化しない）
            this.size = size;
            // マルチサンプルの設定（ターゲットも初期化する）
            this.SampleDesc = sampleDesc;

            // その他設定
            this.MatrixManager = matrixManager;
            this.WorldSpace = new WorldSpace(context);
            this.fpsCounter=new FPSCounter();
            SetViewport();
            this.HitChecker=new TexturedBufferHitChecker(this.Context,this);
        }
        ~TextureTargetContext()
        {
            Dispose(false);
        }
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Context = null;
                }

                if (this.WorldSpace != null)
                {
                    this.WorldSpace.Dispose();
                    this.WorldSpace = null;
                }

                DisposeTargetViews();
                if (this.RenderTarget != null && !this.RenderTarget.Disposed)
                    this.RenderTarget.Dispose();
                if (this.DepthTarget != null && !this.DepthTarget.Disposed)
                    this.DepthTarget.Dispose();
                this.HitChecker.Dispose();
                this.disposed = true;
            }
        }

        ///// <summary>
        ///// Gets a render target settings．
        ///// </summary>
        protected virtual Texture2DDescription getRenderTargetTexture2DDescription()
        {
            return new Texture2DDescription
            {
                Width = this.Size.Width,
                Height = this.Size.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = this.SampleDesc,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
        }

        /// <summary>
        /// Gets the setting of the depth stencil Buffa。
        /// </summary>
        protected virtual Texture2DDescription getDepthBufferTexture2DDescription()
        {
            return new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D32_Float,
                Width = this.Size.Width,
                Height = this.Size.Height,
                MipLevels = 1,
                SampleDescription = this.SampleDesc
            };
        }

        /// <summary>
        /// Gets or sets the matrix manager.
        /// </summary>
        /// <value>
        /// The matrix manager.
        /// </value>
        public override MatrixManager MatrixManager { get; set; }

        /// <summary>
        /// Properties that control the behavior of the camera motion
        /// </summary>
        public override ICameraMotionProvider CameraMotionProvider { get; set; }

        /// <summary>
        /// World space is tied to this screen
        /// </summary>
        public override WorldSpace WorldSpace { get; set; }

        public override void SetViewport()
        {
            this.Context.DeviceManager.Context.Rasterizer.SetViewports(getViewport());
        }

        public void MoveCameraByCameraMotionProvider()
        {
            if (this.CameraMotionProvider == null)
                return;

            this.CameraMotionProvider.UpdateCamera(this.MatrixManager.ViewMatrixManager, this.MatrixManager.ProjectionMatrixManager);
        }

        /// <summary>
        /// Gets the contents of the viewport。
        /// </summary>
        /// <returns>To set the viewport</returns>
        protected virtual Viewport getViewport()
        {
            return new Viewport
            {
                Width = this.Size.Width,
                Height = this.Size.Height,
                MaxZ = 1
            };
        }

        /// <summary>
        /// Sets or retrieves the background color．
        /// </summary>
        public Color4 BackgroundColor { get; set; }

        /// <summary>
        /// WorldSpaceThe content draws．
        /// </summary>
        public void Render()
        {
            if(this.WorldSpace==null|| this.WorldSpace.IsDisposed)return;
            this.Context.SetRenderScreen(this);
            this.Context.ClearScreenTarget(this.BackgroundColor);
            this.Context.Timer.TickUpdater();
            this.fpsCounter.CountFrame();
            MoveCameraByCameraMotionProvider();
            this.WorldSpace.DrawAllResources(this.HitChecker);
            this.Context.DeviceManager.Context.Flush();

        }
    }
}