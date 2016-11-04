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
    /// To render form-based class
    /// </summary>
    public partial class RenderForm : Form
    {
        private bool IsInitialized;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RenderForm" /> class.
        /// </summary>
        public RenderForm()
        {
            InitializeComponent();
            //this.BackgroundColor = new Vector3(0.2f, 0.4f, 0.8f);
            this.BackgroundColor = new Vector3(0.0f, 0.0f, 0.0f);
        }

        /// <summary>
        ///     If you have a render context
        /// </summary>
        /// <param name="context"></param>
        public RenderForm(RenderContext context) : this()
        {
            this.RenderContext = context;
        }

        /// <summary>
        ///     If you want to customize the device creation
        /// </summary>
        /// <param name="deviceManager"></param>
        public RenderForm(BasicGraphicDeviceManager deviceManager) : this()
        {
            this.RenderContext = new RenderContext(deviceManager);            
        }

        /// <summary>
        /// Render context was used to initialize the form
        /// </summary>
        public RenderContext RenderContext { get; private set; }

        /// <summary>
        ///     Context of the screen
        /// </summary>
        public ScreenContext ScreenContext { get; private set; }

        /// <summary>
        ///     Draw for world space
        /// </summary>
        public WorldSpace WorldSpace
        {
            get
            {
                if (this.DesignMode) return null;
                return this.ScreenContext.WorldSpace;
            }
            set
            {
                if (this.DesignMode) return;
                if (value == null)
                {
                    throw new InvalidOperationException("It can not be null in world space");
                }
                this.ScreenContext.WorldSpace = value;
            }
        }

        /// <summary>
        ///     FPSMeasurement class
        /// </summary>
        public FPSCounter FpsCounter { get; private set; }

        /// <summary>
        ///     Clear color render target
        /// </summary>
        public Vector3 BackgroundColor { get; set; }

        /// <summary>
        ///     Draws by paint loop?
        ///     MessagePump.Run If you can not call the Render method by, to run the loop in the form If true
        /// </summary>
        public bool DoOnPaintLoop { get; set; }

        /// <summary>
        ///     <see cref="E:System.Windows.Forms.Form.Load" /> It raises an event。
        /// </summary>
        /// <param name="e">Event And stores the data<see cref="T:System.EventArgs" />。</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            #region Design mode when processing

            if (this.DesignMode)
            {
                var label = new Label();
                label.Text = "RenderForm\n* It can not be drawn in the design mode. \n* window of size, such as the title is also can be changed from the design view.";
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Font = new Font("Meiriyo", 30);
                this.Controls.Add(label);
                return;
            }

            #endregion

            if (this.RenderContext == null)
            {
                this.RenderContext = new RenderContext();
                this.ScreenContext = this.RenderContext.Initialize(this);
                this.RenderContext.UpdateRequireWorlds.Add(this.WorldSpace);
            }
            else //If you already have a RenderContext
            {
                this.ScreenContext = this.RenderContext.CreateScreenContext(this);
            }
            this.FpsCounter = new FPSCounter();
            this.FpsCounter.Start();
            ClientSizeChanged += RenderForm_ClientSizeChanged;
            this.IsInitialized = true;
        }

        /// <summary>
        ///     Handles the ClientSizeChanged event of the RenderForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void RenderForm_ClientSizeChanged(object sender, EventArgs e)
        {
            if (this.ScreenContext != null && this.RenderContext.DeviceManager != null)
            {
                this.ScreenContext.Resize();
                this.ScreenContext.MatrixManager.ProjectionMatrixManager.AspectRatio = (float) this.Width/this.Height;
            }
        }

        public virtual void OnUpdated()
        {
        }

        /// <summary>
        ///     Draws the background of the control。
        /// </summary>
        /// <param name="e">Event And stores the data <see cref="T:System.Windows.Forms.PaintEventArgs" />。</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.DesignMode) base.OnPaintBackground(e);
        }

        /// <summary>
        ///     DoOnPaintLoop=true For the time of
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.RenderContext != null && this.DoOnPaintLoop && !this.DesignMode)
            {
                Render();
                Invalidate();
            }
        }

        /// <summary>
        ///     Clears the screen
        /// </summary>
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
            this.ScreenContext.SetPanelObserver();
            this.ScreenContext.MoveCameraByCameraMotionProvider();
            this.RenderContext.Timer.TickUpdater();
            this.FpsCounter.CountFrame();
            ClearViews();
            this.ScreenContext.WorldSpace.DrawAllResources(this.ScreenContext.HitChekcer);
            this.ScreenContext.SwapChain.Present(0, PresentFlags.None);
            OnPresented();

            this.Text = "FPS: " + this.FpsCounter.FPS;
        }

        protected virtual void OnPresented()
        {
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);

            if (this.DesignMode) return;
            this.RenderContext.ScreenContexts.Remove(this);
            this.ScreenContext.Dispose();
            if (this.RenderContext.ScreenContexts.Count == 0) this.RenderContext.Dispose();
        }
    }
}