#region

using System;
using System.Diagnostics;
using System.Windows.Forms;
using MMF.MME;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Debug = System.Diagnostics.Debug;
using Device = SlimDX.Direct3D11.Device;
using Device1 = SlimDX.Direct3D10_1.Device1;
using DriverType = SlimDX.Direct3D10.DriverType;

#endregion

namespace MMF.DeviceManager
{
    /// <summary>
    ///     A typical device initialization, such as class。
    ///     I hope to inherit from if you want to change the
    /// </summary>
    public class BasicGraphicDeviceManager : IDeviceManager
    {
        // private RasterizerStateDescription rasterizerStateDesc;

        public FeatureLevel DeviceFeatureLevel { get; private set; }

        /// <summary>
        ///     Device
        /// </summary>
        public Device Device { get; private set; }

        /// <summary>
        ///     Device.ImmideateContext
        /// </summary>
        public DeviceContext Context { get; private set; }

        /// <summary>
        ///     Direct2D/DirectWrite用DX10デバイス
        /// </summary>
        public SlimDX.Direct3D10.Device Device10 { get; private set; }

        /// <summary>
        ///     Adapters are used to draw the
        /// </summary>
        public Adapter CurrentAdapter { get; private set; }

        public void Load()
        {
            Load(false,DeviceCreationFlags.None,SlimDX.Direct3D10.DeviceCreationFlags.BgraSupport);
        }

        /// <summary>
        ///     Device initialization
        /// </summary>
        /// <param name="control">A reference to the controls applied</param>
        public void Load(bool needDX11 = false, 
            DeviceCreationFlags dx11flag = DeviceCreationFlags.None,
            SlimDX.Direct3D10.DeviceCreationFlags dx10flag_for2DDraw = SlimDX.Direct3D10.DeviceCreationFlags.BgraSupport)
        {
            ApplyDebugFlags(ref dx11flag,ref dx10flag_for2DDraw);
            this.Factory = new Factory1();
            this.CurrentAdapter = this.Factory.GetAdapter1(0);
            //Initialization of the swap chain
            try
            {
                this.Device = new Device(this.CurrentAdapter, dx11flag,
                    new[] {FeatureLevel.Level_11_0});
            }
            catch (Direct3D11Exception)
            {
                if (needDX11)
                    throw new NotSupportedException("DX11 is not supported. To initialize in DX10.1 should be the first argument needDraw = false of Load.");
                try
                {
                    this.Device = new Device(this.CurrentAdapter, dx11flag, new[] {FeatureLevel.Level_10_0});
                    //this.Device.CheckMultisampleQualityLevels( Format.)

                }
                catch (Direct3D11Exception)
                {
                    throw new NotSupportedException("DX11,DX10.1 An attempt was made to initialization in, but both are not supported.");
                }
            }

            this.DeviceFeatureLevel = this.Device.FeatureLevel;
            this.Context = this.Device.ImmediateContext;
#if VSG_DEBUG
#else
            this.Device10 = new Device1(this.CurrentAdapter, DriverType.Hardware,
                dx10flag_for2DDraw, SlimDX.Direct3D10_1.FeatureLevel.Level_9_3);
#endif
            MMEEffectManager.IniatializeMMEEffectManager(this);
        }

        [Conditional("DEBUG")]
        private void ApplyDebugFlags(ref DeviceCreationFlags dx11flag, ref SlimDX.Direct3D10.DeviceCreationFlags dx10flag_for2DDraw)
        {
            Debug.Print("The device was created in debug mode.");
            //dx11flag = dx11flag | DeviceCreationFlags.Debug;
            dx10flag_for2DDraw = dx10flag_for2DDraw | SlimDX.Direct3D10.DeviceCreationFlags.BgraSupport;
        }

        public virtual void Dispose()
        {
            if (!this.Context.Disposed && this.Context.Rasterizer.State != null && !this.Context.Rasterizer.State.Disposed)
                this.Context.Rasterizer.State.Dispose();
            if (this.Device != null && !this.Device.Disposed) this.Device.Dispose();
            if (this.Device10 != null && !this.Device10.Disposed) this.Device10.Dispose();
            if (this.CurrentAdapter != null && !this.CurrentAdapter.Disposed) this.CurrentAdapter.Dispose();
            if (this.Factory != null && !this.Factory.Disposed) this.Factory.Dispose();
        }

        public Factory1 Factory { get; set; }

        protected virtual void PostLoad(Control control)
        {
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
                    Height = control.Height,
                    Width = control.Width,
                    RefreshRate = new Rational(60, 1)
                },
                OutputHandle = control.Handle,
                SampleDescription = sampDesc,
                SwapEffect = SwapEffect.Discard,
                              Usage = Usage.RenderTargetOutput,
                
            };
        }

        /// <summary>
        ///     Gets the setting of the depth stencil Buffa。
        ///     Override if you want to change the setting of the depth stencil Buffa。
        /// </summary>
        /// <param name="control">A reference to the controls applied</param>
        /// <returns>Setting of Texture2D for depth stencil Buffa</returns>
        protected virtual Texture2DDescription getDepthBufferTexture2DDescription(Control control,
            SampleDescription desc)
        {
            return new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D32_Float,
                Width = control.Width,
                Height = control.Height,
                MipLevels = 1,
                SampleDescription = desc
            };
        }

        /// <summary>
        ///     Gets the contents of the viewport。
        /// </summary>
        /// <param name="control">A reference to the controls applied</param>
        /// <returns>To set the viewport</returns>
        protected virtual Viewport getViewport(Control control)
        {
            return new Viewport
            {
                Width = control.Width,
                Height = control.Height,
                MaxZ = 1
            };
        }

        /// <summary>
        ///     Gets the rasterizer。
        /// </summary>
        /// <param name="control">A reference to the controls applied</param>
        /// <returns>To set the viewport</returns>
        protected virtual RasterizerStateDescription getRasterizerStateDescription(Control control)
        {
            return new RasterizerStateDescription
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
                DepthBias = 0,
                DepthBiasClamp = 0,
                IsAntialiasedLineEnabled = true,
                IsDepthClipEnabled = true,
                IsFrontCounterclockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0
            };
        }
    }
}