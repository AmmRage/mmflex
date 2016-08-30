using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.Matricies;
using MMF.Matricies.Camera;
using SlimDX.Direct3D11;

namespace MMF.DeviceManager
{
    public abstract class TargetContextBase:ITargetContext
    {
        public TargetContextBase(RenderContext renderContext)
        {
            this.Context = renderContext;
        }

        /// <summary>
        /// For example, when resizing the Dispose DepthtargetView and RenderTargetView。
        /// nullThe will be skipped if the。
        /// </summary>
        protected void DisposeTargetViews()
        {
            if (this.RenderTargetView != null && !this.RenderTargetView.Disposed) this.RenderTargetView.Dispose();
            if (this.DepthTargetView != null && !this.DepthTargetView.Disposed) this.DepthTargetView.Dispose();
        }

        /// <summary>
        /// Copy the RenderTarget for STAGING texture
        /// </summary>
        /// <param name="stagingTexture"></param>
        public void CopyRenderTarget(Texture2D stagingTexture)
        {
            this.Context.DeviceManager.Context.CopyResource(this.RenderTargetView.Resource,stagingTexture);
        }

        /// <summary>
        /// Copy the RenderTarget for STAGING texture
        /// </summary>
        /// <param name="stagingTexture">Destination</param>
        /// <param name="left">Annular target</param>
        /// <param name="top">ImageFile</param>
        /// <param name="height">Height</param>
        /// <param name="width">Width</param>
        public void CopyRegionOfRenderTarget(Texture2D stagingTexture,int left,int top,int height,int width)
        {
            this.Context.DeviceManager.Context.CopySubresourceRegion(this.RenderTargetView.Resource, 0,
                new ResourceRegion(left, top, 0, left + width, top + height, 1), stagingTexture, 0, 0, 0, 0);
        }

        public abstract void Dispose();
        public RenderTargetView RenderTargetView { get; protected set; }
        public DepthStencilView DepthTargetView { get; protected set; }
        public abstract MatrixManager MatrixManager { get; set; }
        public abstract ICameraMotionProvider CameraMotionProvider { get; set; }
        public abstract WorldSpace WorldSpace { get; set; }
        public bool IsSelfShadowMode1 { get; protected set; }
        public bool IsEnabledTransparent { get; protected set; }
        public RenderContext Context { get; set; }
        public abstract void SetViewport();
    }
}
