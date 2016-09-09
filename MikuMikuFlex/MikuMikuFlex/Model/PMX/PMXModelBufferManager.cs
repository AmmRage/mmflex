using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.BoneWeight;
using MMF.Utility;
using SlimDX;
using SlimDX.Direct3D11;
using Debug = System.Diagnostics.Debug;

namespace MMF.Model.PMX
{
    public class PMXModelBufferManager : IBufferManager
    {
        public Buffer VertexBuffer { get; private set; }

        public Buffer IndexBuffer { get; private set; }

        public InputLayout VertexLayout { get; set; }

        private DataBox vertexDataBox;

        public void Dispose()
        {
            if (this.VertexBuffer != null && !this.VertexBuffer.Disposed) this.VertexBuffer.Dispose();
            if (this.IndexBuffer != null && !this.IndexBuffer.Disposed) this.IndexBuffer.Dispose();
            if (this.VertexLayout != null && !this.VertexLayout.Disposed) this.VertexLayout.Dispose();
        }

        #region IBufferManager member

        public void Initialize(object model, Device device, Effect effect)
        {
            this.Device = device;
            InitializeBuffer(model, device);
            this.VertexLayout = new InputLayout(device, effect.GetTechniqueByIndex(1).GetPassByIndex(0).Description.Signature,
                BasicInputLayout.VertexElements);
        }

        public int VerticiesCount
        {
            get
            {
                return this.InputVerticies.Length;
            }
        }

        #endregion

        private void InitializeBuffer(object model, Device device)
        {
            
            ModelData modelData = (ModelData) model;
            List<BasicInputLayout> verticies = new List<BasicInputLayout>();
            List<uint> indexes = new List<uint>();
            for (int i = 0; i < modelData.VertexList.VertexCount; i++)
            {
                LoadVertex(modelData.VertexList.Vertexes[i], verticies);
            }
            this.vertexDataBox=new DataBox(0,0,new DataStream(verticies.ToArray(),true,true));
            this.VertexBuffer = CGHelper.CreateBuffer(device, verticies.Count*BasicInputLayout.SizeInBytes,
                BindFlags.VertexBuffer);
            device.ImmediateContext.UpdateSubresource(this.vertexDataBox, this.VertexBuffer, 0);

            this.InputVerticies = verticies.ToArray();
            foreach (SurfaceData surface in modelData.SurfaceList.Surfaces)
            {
                indexes.Add(surface.p);
                indexes.Add( surface.q);
                indexes.Add( surface.r);
            }
            this.IndexBuffer = CGHelper.CreateBuffer(indexes, device, BindFlags.IndexBuffer);
        }

        /// <summary>
        ///     Vertex data contains the input layout
        /// </summary>
        /// <param name="vertexData"></param>
        /// <param name="verticies"></param>
        private void LoadVertex(VertexData vertexData, List<BasicInputLayout> verticies)
        {
            BasicInputLayout vertexInputlayout = new BasicInputLayout();
            vertexInputlayout.Position =new Vector4(vertexData.Position,1f);
            vertexInputlayout.Normal = vertexData.Normal;
            vertexInputlayout.UV = vertexData.UV;
            vertexInputlayout.Index =(uint)verticies.Count;
            if (vertexData.BoneWeight is BDEF1)
            {
                BDEF1 v = (BDEF1) vertexData.BoneWeight;
                vertexInputlayout.BoneIndex1 =  (uint) v.boneIndex;
                vertexInputlayout.BoneWeight1 = 1.0f;
            }
            else if (vertexData.BoneWeight is BDEF2)
            {
                BDEF2 v = (BDEF2) vertexData.BoneWeight;
                vertexInputlayout.BoneIndex1 =  (uint) v.Bone1ReferenceIndex;
                vertexInputlayout.BoneIndex2 =  (uint) v.Bone2ReferenceIndex;
                vertexInputlayout.BoneWeight1 = v.Weight;
                vertexInputlayout.BoneWeight2 = 1f - v.Weight;
            }
            else if (vertexData.BoneWeight is SDEF)
            {
                //TODO 以下はBDEF2としての実装SDEFとして実装はされていない。
                SDEF v = (SDEF) vertexData.BoneWeight;
                vertexInputlayout.BoneIndex1 =  (uint) v.Bone1ReferenceIndex;
                vertexInputlayout.BoneIndex2 =  (uint) v.Bone2ReferenceIndex;
                vertexInputlayout.BoneWeight1 = v.Bone1Weight;
                vertexInputlayout.BoneWeight2 = 1f - v.Bone1Weight;
                vertexInputlayout.Sdef_C = new Vector4(v.SDEF_C,1f);
                vertexInputlayout.SdefR0 = v.SDEF_R0;
                vertexInputlayout.SdefR1 = v.SDEF_R1;
            }
            else
            {
                BDEF4 v = (BDEF4) vertexData.BoneWeight;
                float sumWeight = v.Weights.X + v.Weights.X + v.Weights.Z + v.Weights.W;
                vertexInputlayout.BoneIndex1 =  (uint) v.Bone1ReferenceIndex;
                vertexInputlayout.BoneIndex2 =  (uint) v.Bone2ReferenceIndex;
                vertexInputlayout.BoneIndex3 =  (uint) v.Bone3ReferenceIndex;
                vertexInputlayout.BoneIndex4 =  (uint) v.Bone4ReferenceIndex;
                vertexInputlayout.BoneWeight1 = v.Weights.X/sumWeight;
                vertexInputlayout.BoneWeight2 = v.Weights.Y/sumWeight;
                vertexInputlayout.BoneWeight3 = v.Weights.Z/sumWeight;
                vertexInputlayout.BoneWeight4 = v.Weights.W/sumWeight;
            }
            verticies.Add((vertexInputlayout));
        }


        public BasicInputLayout[] InputVerticies { get; private set; }

        public void RecreateVerticies()
        {
            if (this.NeedReset)
            {
                this.vertexDataBox.Data.WriteRange(this.InputVerticies);
                this.vertexDataBox.Data.Position = 0;
                this.Device.ImmediateContext.UpdateSubresource(this.vertexDataBox, this.VertexBuffer, 0);
                this.NeedReset = false;
            }
        }

        public Device Device { get; set; }


        public bool NeedReset { get; set; }
    }
}