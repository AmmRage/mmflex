using System;
using SlimDX.Direct3D11;

namespace MMF.Model
{
    public interface IToonTextureManager : IDisposable
    {
        /// <summary>
        ///     An array of Toon avatar
        /// </summary>
        ShaderResourceView[] ResourceViews { get; }

        /// <summary>
        ///     Initialize method
        /// </summary>
        /// <param name="context"></param>
        /// <param name="subresourceManager">Target subresources Manager</param>
        void Initialize(RenderContext context, ISubresourceLoader subresourceManager);

        /// <summary>
        ///     Loading Toon
        /// </summary>
        /// <param name="path">TextureName</param>
        /// <returns></returns>
        int LoadToon(string path);
    }
}