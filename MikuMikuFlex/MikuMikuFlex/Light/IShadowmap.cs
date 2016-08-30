using System;
using MMF.Matricies;
using MMF.Model;
using SlimDX.Direct3D11;

namespace MMF.Light
{
    /// <summary>
    ///     What can cast a shadow on the interface
    /// </summary>
    public interface IShadowmap : IDisposable
    {
        /// <summary>
        ///     Matrix light from
        /// </summary>
        MatrixManager MatriciesFromLight { get; }

        /// <summary>
        ///     Render target
        /// </summary>
        RenderTargetView ShadowBufferRenderTarget { get; }

        /// <summary>
        ///     Depth-stencil-view
        /// </summary>
        DepthStencilView ShadowBufferDepthStencil { get; }

        /// <summary>
        ///     Texture
        /// </summary>
        Texture2D ShadowBufferDepthTexture { get; }

        /// <summary>
        ///     Depth from textures resources
        /// </summary>
        ShaderResourceView DepthTextureResource { get; }


        /// <summary>
        ///     Portable Transformer
        /// </summary>
        ITransformer Transformer { get; }
    }
}