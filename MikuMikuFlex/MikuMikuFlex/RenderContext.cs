using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MMF.DeviceManager;
using MMF.Light;
using MMF.Matricies;
using MMF.Matricies.Camera;
using MMF.Matricies.Projection;
using MMF.Matricies.World;
using MMF.Model;
using MMF.Motion;
using SlimDX;
using SlimDX.Direct2D;
using SlimDX.Direct3D11;
using FactoryType = SlimDX.DirectWrite.FactoryType;
using FillMode = SlimDX.Direct3D11.FillMode;

namespace MMF
{
    /// <summary>
    /// Contains various information about the rendering。
    /// This reference can be necessary when the 3D drawing lot
    /// </summary>
    public class RenderContext : IDisposable
    {
        public event EventHandler Update = delegate { };

        /// <summary>
        /// Without specifying a control, create a RenderContext
        /// </summary>
        /// <param name="deviceManager">To use Device Manager．</param>
        /// <returns>RenderContext created</returns>
        public static RenderContext CreateContext(IDeviceManager deviceManager = null)
        {
            RenderContext context = new RenderContext
            {
                DeviceManager = deviceManager
            };
            context.Initialize();
            return context;
        }

        /// <summary>
        /// An associative array of control and ScreenContext
        /// </summary>
        private Dictionary<Control, ScreenContext> screenContexts = new Dictionary<Control, ScreenContext>();

        /// <summary>
        /// Required world update
        /// Heavy processing, so should be removed when not needed
        /// </summary>
        public List<WorldSpace> UpdateRequireWorlds = new List<WorldSpace>();

        /// <summary>
        /// Currently running rendering in the render thread context
        /// </summary>
        public ITargetContext CurrentTargetContext { get; private set; }

        /// <summary>
        /// Light management class (in progress)
        /// </summary>
        public LightMatrixManager LightManager;

        /// <summary>
        /// Gets the current instance using Device Manager．
        /// </summary>
        public IDeviceManager DeviceManager { get; private set; }

        /// <summary>
        /// Whether or not the destruction of the Device Manager in this instance．
        /// </summary>
        private bool disposeDeviceManager = false;

        /// <summary>
        ///     MMEClass to manage the compliance matrix
        /// </summary>
        public MatrixManager MatrixManager
        {
            get { return this.CurrentTargetContext.MatrixManager; }
        }

        /// <summary>
        ///     Timer
        /// </summary>
        public MotionTimer Timer;

        internal List<IDisposable> Disposables = new List<IDisposable>();

        /// <summary>
        /// Current ClearSetColor value
        /// </summary>
        public Color4 CurrentClearColor { get; set; }

        /// <summary>
        /// Current ClearSetDepth value
        /// </summary>
        public float CurrentClearDepth { get; set; }

        /// <summary>
        /// Current RenderColorTarget0~7の値
        /// </summary>
        public RenderTargetView[] CurrentRenderColorTargets = new RenderTargetView[8];

        /// <summary>
        /// Current RenderDepthStencilTarget value
        /// </summary>
        public DepthStencilView CurrentRenderDepthStencilTarget;

        /// <summary>
        /// ScreenContextBe null and does not become a target
        /// </summary>
        public PanelObserver CurrentPanelObserver;

        /// <summary>
        /// For single-sided drawing rastalizastate
        /// </summary>
        public RasterizerState CullingRasterizerState { get; private set; }

        /// <summary>
        /// Rastalizastate for double-sided drawing
        /// </summary>
        public RasterizerState NonCullingRasterizerState { get; private set; }

        /// <summary>
        /// DirectFactory of 2D
        /// </summary>
        public Factory D2DFactory { get; set; }

        /// <summary>
        /// DirectWriteOf the factory
        /// </summary>
        public SlimDX.DirectWrite.Factory DWFactory { get; private set; }

        public BlendStateManager BlendingManager { get; private set; }

        /// <summary>
        /// An associative array of control and ScreenContext
        /// </summary>
        public Dictionary<Control, ScreenContext> ScreenContexts
        {
            get { return this.screenContexts; }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        public RenderContext()
        {
        }

        public RenderContext(BasicGraphicDeviceManager deviceManager)
        {
            this.DeviceManager = deviceManager;
        }

        /// <summary>
        /// Unmanaged リソースの解放およびリセットに関連付けられているアプリケーション定義のタスクを実行します。
        /// </summary>
        public void Dispose()
        {
            foreach (var screenContext in this.screenContexts)
            {
                screenContext.Value.Dispose();
            }
            if (this.CullingRasterizerState != null && !this.CullingRasterizerState.Disposed)
                this.CullingRasterizerState.Dispose();
            if (this.NonCullingRasterizerState != null && !this.NonCullingRasterizerState.Disposed)
                this.NonCullingRasterizerState.Dispose();
            foreach (var disposable in this.Disposables)
            {
                disposable.Dispose();
            }
            if (this.D2DFactory != null && !this.D2DFactory.Disposed) this.D2DFactory.Dispose();
            if (this.DWFactory != null && !this.DWFactory.Disposed) this.DWFactory.Dispose();

            if (this.disposeDeviceManager)
                this.DeviceManager.Dispose();
        }

        public void Initialize()
        {
            InitializeDevices();
            this.Timer = new MotionTimer(this);
        }

        /// <summary>
        ///     RenderContextThe initialization process
        /// </summary>
        public ScreenContext Initialize(Control targetControl)
        {
            InitializeDevices();
            this.Timer = new MotionTimer(this);
            this.BlendingManager = new BlendStateManager(this);
            this.BlendingManager.SetBlendState(BlendStateManager.BlendStates.Alignment);
            //Initialization of matrix
            var matrixManager = InitializeMatricies();

            ScreenContext primaryContext = new ScreenContext(targetControl, this, matrixManager);

            this.screenContexts.Add(targetControl, primaryContext);
            this.CurrentTargetContext = primaryContext;
            //PhysicsManager=new PhysicsManager(this);
            ResetTargets();

            return primaryContext;
        }

        private MatrixManager InitializeMatricies()
        {
            BasicCamera Camera = new BasicCamera(new Vector3(0, 20, -40), new Vector3(0, 3, 0), new Vector3(0, 1, 0));
            BasicProjectionMatrixProvider Projection = new BasicProjectionMatrixProvider();
            Projection.InitializeProjection((float) Math.PI/4f, 1.618f, 1, 2000);
            MatrixManager matrixManager = new MatrixManager(new BasicWorldMatrixProvider(), Camera, Projection);
            this.LightManager = new LightMatrixManager(matrixManager);
            return matrixManager;
        }

        private void InitializeDevices()
        {
            InitializeMatricies();
            //Initializing the 3D CG
            if (this.DeviceManager == null)
            {
                this.disposeDeviceManager = true;
                this.DeviceManager = new BasicGraphicDeviceManager();
                this.DeviceManager.Load();
            }
            RasterizerStateDescription desc = new RasterizerStateDescription();
            desc.CullMode = CullMode.Back;
            desc.FillMode = FillMode.Solid;
            this.CullingRasterizerState = RasterizerState.FromDescription(this.DeviceManager.Device, desc);
            desc.CullMode = CullMode.None;
            this.NonCullingRasterizerState = RasterizerState.FromDescription(this.DeviceManager.Device, desc);

            //Creation of DirectWrite factory
#if VSG_DEBUG
#else
            this.DWFactory = new SlimDX.DirectWrite.Factory(FactoryType.Isolated);
            this.D2DFactory = new Factory(SlimDX.Direct2D.FactoryType.Multithreaded, DebugLevel.Information);
#endif
        }

        /// <summary>
        /// To set the default render target for the current screen context
        /// </summary>
        private void ResetTargets()
        {
            this.CurrentRenderColorTargets[0] = this.CurrentTargetContext.RenderTargetView;
            this.CurrentRenderDepthStencilTarget = this.CurrentTargetContext.DepthTargetView;
            this.DeviceManager.Context.OutputMerger.SetTargets(this.CurrentRenderDepthStencilTarget,
                this.CurrentRenderColorTargets);
        }

        /// <summary>
        /// To clear the screen for the current screen context
        /// </summary>
        /// <param name="color"></param>
        public void ClearScreenTarget(Color4 color)
        {
            ResetTargets();
            this.DeviceManager.Context.ClearRenderTargetView(this.CurrentTargetContext.RenderTargetView,
                color);
            this.DeviceManager.Context.ClearDepthStencilView(this.CurrentTargetContext.DepthTargetView,
                DepthStencilClearFlags.Depth, 1, 0);
        }

        /// <summary>
        /// To update all the world space
        /// </summary>
        public void UpdateWorlds()
        {
            foreach (var updateReqireWorld in this.UpdateRequireWorlds)
            {
                updateReqireWorld.UpdateAllDynamicTexture();
                foreach (var drawableGroup in updateReqireWorld.DrawableGroups)
                {
                    drawableGroup.ForEach(drawable => drawable.Update());
                }
            }
            Update(this, new EventArgs());
        }

        /// <summary>
        /// The specified control will be drawn
        /// </summary>
        /// <param name="context"></param>
        public void SetRenderScreen(ITargetContext context)
        {
            this.CurrentTargetContext = context;
            context.SetViewport();
        }

        public ScreenContext CreateScreenContext(Control control)
        {
            BasicCamera camera = new BasicCamera(new Vector3(0, 20, -40), new Vector3(0, 3, 0), new Vector3(0, 1, 0));
            BasicProjectionMatrixProvider projection = new BasicProjectionMatrixProvider();
            projection.InitializeProjection((float) Math.PI/4f, 1.618f, 1, 200);
            MatrixManager matrixManager = new MatrixManager(new BasicWorldMatrixProvider(), camera, projection);
            ScreenContext context = new ScreenContext(control, this, matrixManager);
            this.screenContexts.Add(control, context);

            return context;
        }

        /// <summary>
        /// The generated textures
        /// </summary>
        /// <returns></returns>
        public Texture2D CreateTexture2D(Texture2DDescription desc)
        {
            return new Texture2D(this.DeviceManager.Device, desc);
        }
    }
}