using System;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;

namespace MMF.DeviceManager
{
    public interface IDeviceManager : IDisposable
    {
        /// <summary>
        ///     The initialize device
        /// </summary>
        Device Device { get; }

        /// <summary>
        /// Direct2D/DirectWrite用DX10デバイス
        /// </summary>
        SlimDX.Direct3D10.Device Device10 { get; }

        FeatureLevel DeviceFeatureLevel { get; }


        /// <summary>
        ///     Device context is initialized
        /// </summary>
        DeviceContext Context { get; }

        /// <summary>
        /// Adapters are used to draw the
        /// </summary>
        Adapter CurrentAdapter { get; }

        Factory1 Factory { get; set; }

        /// <summary>
        /// Rastalizastate
        /// </summary>
        //RasterizerStateDescription RasterizerStateDesc { get; set; }

        /// <summary>
        ///     Load control
        /// </summary>
        /// <param name="control"></param>
        void Load();
    }
}