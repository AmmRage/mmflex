using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.Model.Shape;
using SlimDX;

namespace MMF.Model.Controller.ControllerComponent
{
    class CenterCrossLine:IDrawable
    {
        private CubeShape xLine;

        private CubeShape yLine;

        private CubeShape zLine;

        private static float thickness = 0.1f;

        public CenterCrossLine(RenderContext context)
        {
            this.xLine=new CubeShape(context,new Vector4(1,0.55f,0,0.7f));
            this.yLine = new CubeShape(context, new Vector4(1, 0.55f, 0, 0.7f));
            this.zLine = new CubeShape(context, new Vector4(1, 0.55f, 0, 0.7f));
            this.xLine.Initialize();
            this.yLine.Initialize();
            this.zLine.Initialize();
            this.xLine.Transformer.Scale=new Vector3(30f,thickness,thickness);
            this.yLine.Transformer.Scale = new Vector3(thickness, 30f, thickness);
            this.zLine.Transformer.Scale = new Vector3(thickness, thickness, 30f);
        }

        public void AddTranslation(Vector3 trans)
        {
            this.xLine.Transformer.Position += trans;
            this.yLine.Transformer.Position += trans;
            this.zLine.Transformer.Position += trans;
        }
        

        public void Dispose()
        {
            this.xLine.Dispose();
            this.yLine.Dispose();
            this.zLine.Dispose();
        }

        public bool Visibility { get; set; }
        public string FileName { get; private set; }
        public int SubsetCount { get; private set; }
        public int VertexCount { get; private set; }
        public ITransformer Transformer { get; private set; }
        public void Draw()
        {
            this.xLine.Draw();
            this.yLine.Draw();
            this.zLine.Draw();
        }

        public void Update()
        {
        }

        public Vector4 SelfShadowColor { get; set; }
        public Vector4 GroundShadowColor { get; set; }
    }
}
