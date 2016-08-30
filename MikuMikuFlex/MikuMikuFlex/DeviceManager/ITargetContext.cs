using System;
using MMF.Matricies;
using MMF.Matricies.Camera;
using SlimDX.Direct3D11;

namespace MMF.DeviceManager
{
    public interface ITargetContext : IDisposable
    {
        /// <summary>
        /// Gets the render target view.
        /// </summary>
        /// <value>
        /// The render target view.
        /// </value>
        RenderTargetView RenderTargetView { get; }

        /// <summary>
        /// Gets the depth target view.
        /// </summary>
        /// <value>
        /// The depth target view.
        /// </value>
        DepthStencilView DepthTargetView { get; }

        /// <summary>
        /// Gets or sets the matrix manager.
        /// </summary>
        /// <value>
        /// The matrix manager.
        /// </value>
        MatrixManager MatrixManager { get; set; }

        /// <summary>
        /// Properties that control the behavior of the camera motion
        /// </summary>
        ICameraMotionProvider CameraMotionProvider { get; set; }

        /// <summary>
        /// World space is tied to this screen
        /// </summary>
        WorldSpace WorldSpace { get; set; }

        /// <summary>
        /// MMESpecial parameters
        /// object_ssの時に取得するセルフシャドウモード
        /// bool parthfにあたる
        /// </summary>
        bool IsSelfShadowMode1 { get; }

        /// <summary>
        /// MMESpecial parameters
        /// bool transpにあたる値
        /// </summary>
        bool IsEnabledTransparent { get; }

        RenderContext Context { get; set; }
        
        void SetViewport();
    }
}