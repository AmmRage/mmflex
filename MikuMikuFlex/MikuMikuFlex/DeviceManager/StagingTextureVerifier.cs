using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using MapFlags = SlimDX.Direct3D11.MapFlags;

namespace MMF.DeviceManager
{
    /// <summary>
    /// Determine the supported texture information
    /// </summary>
    class StagingTextureVerifier
    {
        private static StagingTextureVerifier instance;

        public Format SupportedType { get; private set; }

        private StagingTextureVerifier(RenderContext context)
        {
            Texture2DDescription commonDesc = new Texture2DDescription()
            {
                Height = 1,
                Width = 1,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,

                /*
                Width = ClientWidth,
                Height = ClientHeight,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription =
                    (Enable4XMsaa && Device.FeatureLevel >= FeatureLevel.Level_10_1)
                        ? new SampleDescription(aa, Msaa4XQuality - 1)
                        : new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
                 */
            };
            Texture2DDescription gpuDesc = commonDesc;
            gpuDesc.CpuAccessFlags=CpuAccessFlags.None;
            gpuDesc.Usage=ResourceUsage.Default;
            gpuDesc.Format=Format.R32_UInt;
            gpuDesc.BindFlags=BindFlags.RenderTarget;
            Texture2DDescription uintDesc = commonDesc;
            uintDesc.Format=Format.R32_UInt;
            using (Texture2D gpuTexture = context.CreateTexture2D(gpuDesc))
            using (Texture2D uintTexture=context.CreateTexture2D(uintDesc))
            using (RenderTargetView renderTarget=new RenderTargetView(context.DeviceManager.Device,gpuTexture))
            {
                context.DeviceManager.Context.ClearRenderTargetView(renderTarget,new Color4(1,1,1,1));
                context.DeviceManager.Context.CopyResource(gpuTexture,uintTexture);
                var data = context.DeviceManager.Context.MapSubresource(uintTexture, 0, MapMode.Read, MapFlags.None);
                if (data.Data.Read<uint>() != 0)
                {
                    this.SupportedType=Format.R32_UInt;
                }
                context.DeviceManager.Context.UnmapSubresource(uintTexture,0);
            }
        }

        public static StagingTextureVerifier getInstance(RenderContext context)
        {
            if(instance==null)instance=new StagingTextureVerifier(context);
            return instance;
        }
    }
}
