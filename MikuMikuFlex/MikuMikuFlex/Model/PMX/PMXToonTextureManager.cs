using System.Collections.Generic;
using System.IO;
using MMF.Utility;
using SlimDX.Direct3D11;

namespace MMF.Model.PMX
{
    internal class PMXToonTextureManager : IToonTextureManager
    {
        private readonly List<ShaderResourceView> resourceViewsList = new List<ShaderResourceView>();
        private Device _device;

        private ISubresourceLoader _subresourceManager;

        public ShaderResourceView[] ResourceViews
        {
            get { return this.resourceViewsList.ToArray(); }
        }

        public void Initialize(RenderContext context, ISubresourceLoader subresourceManager)
        {
            this._device = context.DeviceManager.Device;
            this._subresourceManager = subresourceManager;
            for (int i = 0; i < 11; i++)
            {
                string path = CGHelper.ToonDir + string.Format(@"toon{0}.bmp", i);
                if (File.Exists(path))
                    this.resourceViewsList.Add(ShaderResourceView.FromFile(context.DeviceManager.Device, path));
                else
                {
                    
                }
            }
        }

        public int LoadToon(string path)
        {
            using (Stream stream = this._subresourceManager.getSubresourceByName(path))
            {
                if (stream == null) return 0;
                this.resourceViewsList.Add(ShaderResourceView.FromStream(this._device, stream, (int) stream.Length));
                return this.resourceViewsList.Count - 1;
            }
        }

        public void Dispose()
        {
            foreach (ShaderResourceView shaderResourceView in this.resourceViewsList)
            {
                shaderResourceView.Dispose();
            }
        }
    }
}