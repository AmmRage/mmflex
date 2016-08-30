using System;
using System.Drawing;
using System.Windows.Forms;
using MMF.DeviceManager;
using MMF.Utility;
using SlimDX;
using SlimDX.DXGI;

namespace MMF.Controls.Forms
{
    /// <summary>
    ///     Base class for rendering views
    /// </summary>
    public class RenderControl : UserControl
    {
        /// <summary>
        ///     The rendering context
        ///     In various places, such as model needed
        /// </summary>
        public RenderContext RenderContext { get; private set; }

        public ScreenContext ScreenContext { get; private set; }

        public WorldSpace WorldSpace
        {
            get { return this.ScreenContext.WorldSpace; }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("It can not be null in world space.");
                }
                this.ScreenContext.WorldSpace = value;
            }
        }

        /// <summary>
        ///     The background color
        /// </summary>
        public Color3 BackgroundColor { get; set; }

        /// <summary>
        ///     FPSCounter
        /// </summary>
        public FPSCounter FpsCounter { get; private set; }

        /// <summary>
        ///     Determination of whether or not initialized
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        ///     The initialization process
        /// </summary>
        /// <param name="context"></param>
        public virtual void Initialize(RenderContext context = null)
        {
            if (context == null)
            {
                this.RenderContext = new RenderContext();
                this.ScreenContext = this.RenderContext.Initialize(this);
                this.RenderContext.UpdateRequireWorlds.Add(this.WorldSpace);
            }
            else
            {
                this.RenderContext = context;
                this.ScreenContext = context.CreateScreenContext(this);
            }
            this.FpsCounter = new FPSCounter();
            this.FpsCounter.Start();
            this.IsInitialized = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.DesignMode)
            {
                var label = new Label();
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Text =
                    string.Format(
                        "{0} \n * Design mode can not be drawn. Please call the Initialize method in the OnLoad of \n * Always Form. \n * If you want to use the RenderContext utilized in another control, please give to the first argument of Initialize",
                        GetType());
                this.Controls.Add(label);
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (!this.DesignMode && this.ScreenContext != null)
            {
                this.ScreenContext.Resize();
                this.ScreenContext.MatrixManager.ProjectionMatrixManager.AspectRatio = (float)this.Width / this.Height;
            }
        }


        protected virtual void ClearViews()
        {
            this.RenderContext.ClearScreenTarget(new Color4(this.BackgroundColor));
        }

        /// <summary>
        ///     Rendering
        /// </summary>
        public virtual void Render()
        {
            if (!this.IsInitialized || !this.Visible) return;
            this.RenderContext.SetRenderScreen(this.ScreenContext);
            this.ScreenContext.MoveCameraByCameraMotionProvider();
            this.RenderContext.Timer.TickUpdater();
            this.FpsCounter.CountFrame();
            ClearViews();
            this.ScreenContext.WorldSpace.DrawAllResources(this.ScreenContext.HitChekcer);
            this.ScreenContext.SwapChain.Present(0, PresentFlags.None);
        }

        /// <summary>
        ///     <see cref="T:System.Windows.Forms.Control" /> And to release the unmanaged resources that child control is using. In options, managed resources will also release.
        /// </summary>
        /// <param name="disposing">true to release both Managed and unmanaged resources. When to release only unmanaged resources false.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (this.RenderContext != null) this.RenderContext.Dispose();
            this.RenderContext.ScreenContexts.Remove(this);
            this.ScreenContext.Dispose();
            if (this.RenderContext.ScreenContexts.Count == 0) this.RenderContext.Dispose();
        }
    }
}