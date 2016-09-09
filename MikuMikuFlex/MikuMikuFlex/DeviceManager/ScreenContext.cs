using System.Windows.Forms;
using MMF.Matricies;
using MMF.Matricies.Camera;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;

namespace MMF.DeviceManager
{
    public class ScreenContext:TargetContextBase
    {
        public ScreenContext(Control owner,RenderContext context,MatrixManager manager):base(context)
        {
            Device device = context.DeviceManager.Device;
            SampleDescription sampleDesc = new SampleDescription(4, 0); //zy 1
            this.SwapChain = new SwapChain(context.DeviceManager.Factory, device, getSwapChainDescription(owner, sampleDesc));
            //Set rasterizer
            //Initialization of the depth stencil Buffa
            using (Texture2D depthBuffer = new Texture2D(device, getDepthBufferTexture2DDescription(owner, sampleDesc)))
            {
                this.DepthTargetView = new DepthStencilView(device, depthBuffer);
            }
            //Initializing render targets
            using (Texture2D renderTexture = Resource.FromSwapChain<Texture2D>(this.SwapChain, 0))
            {
                this.RenderTargetView = new RenderTargetView(device, renderTexture);
            }
            this.WorldSpace=new WorldSpace(context);
            this.BindedControl = owner;
            this.MatrixManager = manager;
            this.PanelObserver=new PanelObserver(owner);
            SetViewport();
            this.HitChekcer=new TexturedBufferHitChecker(this.Context,this);
            this.HitChekcer.Resize(owner.ClientSize);
            owner.MouseMove += owner_MouseMove;
            owner.MouseDown += owner_MouseDown;
            owner.MouseUp += owner_MouseUp;
        }

        void owner_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Left)
                this.HitChekcer.IsMouseDown = false;
        }

        void owner_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button==MouseButtons.Left)
                this.HitChekcer.IsMouseDown = true;
        }

        void owner_MouseMove(object sender, MouseEventArgs e)
        {
            this.HitChekcer.CheckPoint = e.Location;
        }

        public TexturedBufferHitChecker HitChekcer;

        /// <summary>
        /// Gets or sets the matrix manager.
        /// </summary>
        /// <value>
        /// The matrix manager.
        /// </value>
        public override MatrixManager MatrixManager { get; set; }

        /// <summary>
        /// Gets the swap chain.
        /// </summary>
        /// <value>
        /// The swap chain.
        /// </value>
        public SwapChain SwapChain { get; private set; }

        /// <summary>
        /// Gets the binded control.
        /// </summary>
        /// <value>
        /// The binded control.
        /// </value>
        public Control BindedControl { get; private set; }


        /// <summary>
        /// Properties that control the behavior of the camera motion
        /// </summary>
        public override ICameraMotionProvider CameraMotionProvider { get; set; }


        /// <summary>
        /// World space is tied to this screen
        /// </summary>
        public override WorldSpace WorldSpace { get; set; }

        /// <summary>
        /// To monitor the user control class
        /// </summary>
        public PanelObserver PanelObserver { get; private set; }

        public override void SetViewport()
        {
            this.Context.DeviceManager.Context.Rasterizer.SetViewports(getViewport());
        }

        public void SetPanelObserver()
        {
            this.Context.CurrentPanelObserver = this.PanelObserver;
        }

        public void Resize()
        {
            if (this.BindedControl.ClientSize.Width == 0 || this.BindedControl.ClientSize.Height == 0) return; //To none at this time so come just for a moment when form becomes a floating device, make。
            DisposeTargetViews();
            SwapChainDescription desc = this.SwapChain.Description;
            this.SwapChain.ResizeBuffers(desc.BufferCount, this.BindedControl.ClientSize.Width, this.BindedControl.ClientSize.Height, desc.ModeDescription.Format,
                desc.Flags);
            //Initialization of the depth stencil Buffa
            using (Texture2D depthBuffer = new Texture2D(this.Context.DeviceManager.Device, getDepthBufferTexture2DDescription(this.BindedControl, desc.SampleDescription)))
            {
                this.DepthTargetView = new DepthStencilView(this.Context.DeviceManager.Device, depthBuffer);
            }
            //Initializing render targets
            using (Texture2D renderTexture = Resource.FromSwapChain<Texture2D>(this.SwapChain, 0))
            {
                this.RenderTargetView = new RenderTargetView(this.Context.DeviceManager.Device, renderTexture);
            }
            this.HitChekcer.Resize(this.BindedControl.ClientSize);
        }

        public void MoveCameraByCameraMotionProvider()
        {
            if(this.CameraMotionProvider==null)return;
            this.CameraMotionProvider.UpdateCamera(this.MatrixManager.ViewMatrixManager, this.MatrixManager.ProjectionMatrixManager);
        }

        /// <summary>
        ///     Get the swap chain set。
        ///     Override if you want to swap chain set。
        /// </summary>
        /// <param name="control">Reference to the control to adapt</param>
        /// <returns>The swap chain set</returns>
        protected virtual SwapChainDescription getSwapChainDescription(Control control, SampleDescription sampDesc)
        {
            return new SwapChainDescription
            {
                BufferCount = 2,
                Flags = SwapChainFlags.AllowModeSwitch,
                IsWindowed = true,
                ModeDescription = new ModeDescription
                {
                    Format = Format.R8G8B8A8_UNorm,
                    Height = control.ClientSize.Height,
                    Width = control.ClientSize.Width,
                    RefreshRate = new Rational(60, 1)
                },
                OutputHandle = control.Handle,
                SampleDescription = sampDesc,
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
        }

        /// <summary>
        ///     Gets the setting of the depth stencil Buffa。
        ///     Override if you want to change the setting of the depth stencil Buffa。
        /// </summary>
        /// <param name="control">A reference to the controls applied</param>
        /// <returns>Setting of Texture2D for depth stencil Buffa</returns>
        protected virtual Texture2DDescription getDepthBufferTexture2DDescription(Control control, SampleDescription desc)
        {
            return new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D32_Float,
                Width = control.ClientSize.Width,
                Height = control.ClientSize.Height,
                MipLevels = 1,
                SampleDescription = desc
            };
        }

        /// <summary>
        ///     Gets the contents of the viewport。
        /// </summary>
        /// <param name="control">A reference to the controls applied</param>
        /// <returns>To set the viewport</returns>
        protected virtual Viewport getViewport()
        {
            return new Viewport
            {
                Width = this.BindedControl.Width,
                Height = this.BindedControl.Height,
                MaxZ = 1
            };
        }

        public override void Dispose()
        {
            if (this.WorldSpace!=null)
            {
                this.WorldSpace.Dispose();
            }
            if (this.RenderTargetView != null && !this.RenderTargetView.Disposed) this.RenderTargetView.Dispose();
            if (this.DepthTargetView != null && !this.DepthTargetView.Disposed) this.DepthTargetView.Dispose();
            if (this.SwapChain != null && !this.SwapChain.Disposed) this.SwapChain.Dispose();
            this.HitChekcer.Dispose();
        }
    }
}
