//#define VSG_DEBUG//Remove the commented out because Direct2D uses when using the VS2012 graphical debugger and error,。BasicGraphicDeviceManagerも同様にする。

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MMF.DeviceManager;
using MMF.Sprite;
using SlimDX.DXGI;

namespace MMF.Controls.Forms
{
    /// <summary>
    ///     RenderForm in the case of utilizing Direct2D / DirectWrite
    /// </summary>
    public partial class D2DSupportedRenderForm : RenderForm
    {
        private bool IsInitialized;

#if VSG_DEBUG
#else
        /// <summary>
        /// DUse 2D sprites
        /// </summary>
        public D2DSpriteBatch SpriteBatch { get; private set; }
#endif

        public D2DSupportedRenderForm()
        {
            InitializeComponent();
        }

        public D2DSupportedRenderForm(RenderContext context) : base(context)
        {
        }

        public D2DSupportedRenderForm(BasicGraphicDeviceManager deviceManager) : base(deviceManager)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            #region Design mode when processing

            if (this.DesignMode)
            {
                var label = new Label();
                label.Text = "D2DSuppurtedRenderForm\n* It can not be drawn in the design mode. \n * window of size, such as the title is also can be changed from the design view.";
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Font = new Font("Arial", 16);
                this.Controls.Add(label);
                return;
            }

            #endregion

            base.OnLoad(e);
#if VSG_DEBUG
#else
            this.RenderContext.SetRenderScreen(this.ScreenContext);
            this.SpriteBatch = new D2DSpriteBatch(this.RenderContext);
#endif
            ClientSizeChanged += D2DSupportedRenderForm_ClientSizeChanged;
            this.IsInitialized = true;
        }

        private void D2DSupportedRenderForm_ClientSizeChanged(object sender, EventArgs e)
        {
            if (this.DesignMode) return;
#if VSG_DEBUG
#else
            if (this.SpriteBatch != null)
            {
                this.RenderContext.SetRenderScreen(this.ScreenContext);
                this.SpriteBatch.Resize();
            }
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        public override void Render()
        {
            if (!this.IsInitialized || !this.Visible) return;
            this.RenderContext.SetRenderScreen(this.ScreenContext);
            this.ScreenContext.MoveCameraByCameraMotionProvider();
            ClearViews();
            this.RenderContext.Timer.TickUpdater();
            this.FpsCounter.CountFrame();
            this.ScreenContext.WorldSpace.DrawAllResources(this.ScreenContext.HitChekcer);
#if VSG_DEBUG
#else
            this.SpriteBatch.Begin();
            RenderSprite();
            this.SpriteBatch.End();
#endif
            this.ScreenContext.SwapChain.Present(0, PresentFlags.None);
            OnPresented();
            this.Text = "FPS: " + this.FpsCounter.FPS;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
#if VSG_DEBUG

#else
            if (this.SpriteBatch != null) this.SpriteBatch.Dispose();
#endif
        }

        protected virtual void RenderSprite()
        {
        }
    }
}