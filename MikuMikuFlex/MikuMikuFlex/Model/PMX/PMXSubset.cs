using System;
using MMDFileParser.PMXModelParser;
using MMF.MME.VariableSubscriber.MaterialSubscriber;
using SlimDX.Direct3D11;

namespace MMF.Model.PMX
{
    /// <summary>
    ///     Drawing unit
    /// </summary>
    public class PMXSubset : IDisposable, IPMXSubset
    {
        public MaterialInfo MaterialInfo { get; private set; }

        public int SubsetId { get; private set; }

        public PMXSubset(IDrawable drawable,MaterialData data,int subsetId)
        {
            this.Drawable = drawable;
            this.MaterialInfo = MaterialInfo.FromMaterialData(this.Drawable, data);
            this.SubsetId = subsetId;
        }

        /// <summary>
        ///     Starting point of this resource in the index Buffa
        /// </summary>
        public int StartIndex { get; set; }


        /// <summary>
        ///     Number of vertices contained in this resource
        /// </summary>
        public int VertexCount { get; set; }

        public void Dispose()
        {
            if(this.MaterialInfo!=null) this.MaterialInfo.Dispose();
        }

        public IDrawable Drawable { get; set; }

        public bool DoCulling { get; set; }

        public void Draw(Device device)
        {
            device.ImmediateContext.DrawIndexed(3 *this.VertexCount, this.StartIndex, 0);
        }
    }
}