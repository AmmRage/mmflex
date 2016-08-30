using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D11;

namespace MMF.Utility
{
    /// <summary>
    /// PrimitiveTopology=TriangleListの場合用
    /// </summary>
    public class IndexBufferBuilder
    {
        private readonly RenderContext _context;
        private List<uint> list=new List<uint>();

        public IndexBufferBuilder(RenderContext context)
        {
            this._context = context;
        }

        public void AddTriangle(uint p, uint q, uint r)
        {
            this.list.Add(p);
            this.list.Add(q);
            this.list.Add(r);
        }

        public void AddSquare(uint p, uint q, uint r,uint s)
        {
            this.list.Add(p);
            this.list.Add(q);
            this.list.Add(s);
            this.list.Add(s);
            this.list.Add(q);
            this.list.Add(r);
        }

        public Buffer build()
        {
            return CGHelper.CreateBuffer(this.list, this._context.DeviceManager.Device, BindFlags.IndexBuffer);
        }
    }
}
