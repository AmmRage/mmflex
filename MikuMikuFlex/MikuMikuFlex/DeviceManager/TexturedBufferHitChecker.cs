using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Debug = System.Diagnostics.Debug;
using MapFlags = SlimDX.Direct3D11.MapFlags;
using Resource = SlimDX.Direct3D11.Resource;

namespace MMF.DeviceManager
{
    /// <summary>
    /// Model dependency prevention and making a two texture to no avail but so good to not feel。
    /// </summary>
    public class TexturedBufferHitChecker:IDisposable
    {
        private RenderContext renderContext;

        private TargetContextBase screenContext;

        private Texture2D targetUIntTexture;

        private Texture2D targetFloatTexture;

        private RenderTargetView floatRenderTarget;

        private RenderTargetView uintRenderTarget;

        private DepthStencilView depthTarget;

        public Point CheckPoint { get; set; }

        public bool IsMouseDown { get; set; }

        private Size currentSize;


        /// <summary>
        /// To check whether or not the hit list
        /// </summary>
        public List<IHitTestable> CheckTargets=new List<IHitTestable>(); 

        public TexturedBufferHitChecker(RenderContext context,TargetContextBase targetScreenContext)
        {
            this.renderContext = context;
            this.screenContext = targetScreenContext;
            this.targetUIntTexture = this.renderContext.CreateTexture2D(new Texture2DDescription()
            {
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging,
                Format = Format.R32_UInt,
                OptionFlags = ResourceOptionFlags.None,
                BindFlags = BindFlags.None,
                Height = 1,
                Width = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0)
            });
            this.targetFloatTexture = this.renderContext.CreateTexture2D(new Texture2DDescription()
            {
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging,
                Format = Format.R32_Float,
                OptionFlags = ResourceOptionFlags.None,
                BindFlags = BindFlags.None,
                Height = 1,
                Width = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0)
            });
            Resize(new Size(100,100));
        }

        public void Resize(Size size)
        {
            if(this.floatRenderTarget!=null&&!this.floatRenderTarget.Disposed) this.floatRenderTarget.Dispose();
            if (this.depthTarget != null && !this.depthTarget.Disposed) this.depthTarget.Dispose();
            if (this.floatRenderTarget != null && !this.floatRenderTarget.Disposed) this.floatRenderTarget.Dispose();
            if (this.uintRenderTarget != null && !this.uintRenderTarget.Disposed) this.uintRenderTarget.Dispose();
            using (Texture2D renderTexture= this.renderContext.CreateTexture2D(new Texture2DDescription()
            {
                CpuAccessFlags = CpuAccessFlags.None,Usage = ResourceUsage.Default,Format = Format.R32_Float,OptionFlags = ResourceOptionFlags.None,SampleDescription = new SampleDescription(1,0),Width = size.Width,Height = size.Height,BindFlags = BindFlags.RenderTarget,MipLevels = 1,ArraySize = 1
            }))
            {
                this.floatRenderTarget=new RenderTargetView(this.renderContext.DeviceManager.Device,renderTexture);
            }
            using (Texture2D renderTexture = this.renderContext.CreateTexture2D(new Texture2DDescription()
            {
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                Format = Format.R32_UInt,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Width = size.Width,
                Height = size.Height,
                BindFlags = BindFlags.RenderTarget,
                MipLevels = 1,
                ArraySize = 1
            }))
            {
                this.uintRenderTarget = new RenderTargetView(this.renderContext.DeviceManager.Device, renderTexture);
            }
            using (Texture2D depthtexture= this.renderContext.CreateTexture2D(new Texture2DDescription()
            {
                CpuAccessFlags = CpuAccessFlags.None,Usage = ResourceUsage.Default,Format = Format.D32_Float,OptionFlags = ResourceOptionFlags.None,SampleDescription = new SampleDescription(1,0),Width = size.Width,BindFlags = BindFlags.DepthStencil,Height = size.Height,MipLevels = 1,ArraySize = 1
            }))
            {
                this.depthTarget=new DepthStencilView(this.renderContext.DeviceManager.Device,depthtexture);
            }
            this.CheckPoint=new Point();
            this.currentSize = size;
        }

        public void CheckTarget()
        {
            if(this.currentSize.Width< this.CheckPoint.X+2|| this.currentSize.Height< this.CheckPoint.Y+2|| this.CheckPoint.X<0|| this.CheckPoint.Y<0)return;
            //Draw a check for target (UINT)
            this.renderContext.DeviceManager.Device.ImmediateContext.OutputMerger.SetTargets(this.depthTarget, this.floatRenderTarget);
            this.renderContext.DeviceManager.Device.ImmediateContext.ClearDepthStencilView(this.depthTarget,DepthStencilClearFlags.Depth, 1,0);
            this.renderContext.DeviceManager.Device.ImmediateContext.ClearRenderTargetView(this.floatRenderTarget,
                new Color4(0, 0, 0, 0));
            for (int index = 0; index < this.CheckTargets.Count; index++)
            {
                var checkTarget = this.CheckTargets[index];
                if(checkTarget.Visibility)checkTarget.RenderHitTestBuffer(index+1);
            }
            this.renderContext.DeviceManager.Device.ImmediateContext.Flush();
            //Draw a check for target (FLOAT)
            this.renderContext.DeviceManager.Device.ImmediateContext.OutputMerger.SetTargets(this.depthTarget, this.uintRenderTarget);
            this.renderContext.DeviceManager.Device.ImmediateContext.ClearDepthStencilView(this.depthTarget, DepthStencilClearFlags.Depth, 1, 0);
            this.renderContext.DeviceManager.Device.ImmediateContext.ClearRenderTargetView(this.uintRenderTarget,
                new Color4(0, 0, 0, 0));
            for (int index = 0; index < this.CheckTargets.Count; index++)
            {
                var checkTarget = this.CheckTargets[index];
                if(checkTarget.Visibility)checkTarget.RenderHitTestBuffer(index + 1);
            }
            this.renderContext.DeviceManager.Device.ImmediateContext.Flush();
            //HP:⇒CPUテクスチャ転送
            this.renderContext.DeviceManager.Context.CopySubresourceRegion(this.uintRenderTarget.Resource, 0,
                new ResourceRegion(this.CheckPoint.X, this.CheckPoint.Y, 0, this.CheckPoint.X + 1, this.CheckPoint.Y + 1, 1), this.targetUIntTexture,
                0, 0, 0, 0);
            this.renderContext.DeviceManager.Context.CopySubresourceRegion(this.floatRenderTarget.Resource, 0,
                new ResourceRegion(this.CheckPoint.X, this.CheckPoint.Y, 0, this.CheckPoint.X + 1, this.CheckPoint.Y + 1, 1), this.targetFloatTexture,
                0, 0, 0, 0);
            DataBox floatMapResource = this.renderContext.DeviceManager.Context.MapSubresource(this.targetFloatTexture, 0, MapMode.Read,
                MapFlags.None);
            DataBox uintMapResource = this.renderContext.DeviceManager.Context.MapSubresource(this.targetUIntTexture, 0, MapMode.Read,
    MapFlags.None);
            float ud = uintMapResource.Data.Read<float>();//Works and why this is the float.
            float fd = floatMapResource.Data.Read<float>();
            for (int index = 0; index < this.CheckTargets.Count; index++)
            {
                var checkTarget = this.CheckTargets[index];
                checkTarget.HitTestResult((ud ==(index+1)||fd==(index+1))&&checkTarget.Visibility, this.IsMouseDown, this.CheckPoint);
            }

            // Debug.WriteLine(CheckPoint.ToString()+b);
            //for (int index = 0; index < CheckTargets.Count; index++)
           // {
           //     var checkTarget = CheckTargets[index];
           //     checkTarget.HitTestResult((index + 1) == b, IsMouseDown, CheckPoint);
            //}
            this.renderContext.DeviceManager.Context.UnmapSubresource(this.targetUIntTexture,0);
            this.renderContext.DeviceManager.Context.UnmapSubresource(this.targetFloatTexture, 0);
        }


        public void Dispose()
        {
            if(this.targetUIntTexture!=null&&!this.targetUIntTexture.Disposed) this.targetUIntTexture.Dispose();
            if (this.targetFloatTexture != null && !this.targetFloatTexture.Disposed) this.targetFloatTexture.Dispose();
            if(this.depthTarget!=null&&!this.depthTarget.Disposed) this.depthTarget.Dispose();
            if(this.floatRenderTarget!=null&&!this.floatRenderTarget.Disposed) this.floatRenderTarget.Dispose();
            if (this.uintRenderTarget != null && !this.uintRenderTarget.Disposed) this.uintRenderTarget.Dispose();
        }
    }
}
