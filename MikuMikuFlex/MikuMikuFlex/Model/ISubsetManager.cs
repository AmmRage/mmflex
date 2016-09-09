using System;
using System.Collections.Generic;
using MMF.MME;

namespace MMF.Model
{
    /// <summary>
    ///     Take on the method of rendering interface
    /// </summary>
    public interface ISubsetManager : IDisposable
    {
        /// <summary>
        /// Initializes the subset Manager and perform to a subset。
        /// </summary>
        /// <param name="context">Device</param>
        /// <param name="effect"></param>
        /// <param name="subresourceManager"></param>
        /// <param name="ToonManager"></param>
        void Initialze(RenderContext context, MMEEffectManager effect, ISubresourceLoader subresourceManager,
            IToonTextureManager ToonManager);

        void ResetEffect(MMEEffectManager effect);

        void DrawAll();

        void DrawEdges();

        void DrawGroundShadow();

        int SubsetCount { get; }

        List<ISubset> Subsets { get;}
    }
}