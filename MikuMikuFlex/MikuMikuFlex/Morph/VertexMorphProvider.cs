using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.MorphOffset;
using MMF.Model;
using MMF.Motion;
using SlimDX;

namespace MMF.Morph
{
    /// <summary>
    /// Class to manage the vertex morph
    /// </summary>
    public class VertexMorphProvider : IMorphProvider
    {
        public Dictionary<string,VertexMorphData> MorphList=new Dictionary<string,VertexMorphData>();

        private HashSet<uint> movedVertex=new HashSet<uint>();

        private ModelData model;

        public VertexMorphProvider(ModelData model,IBufferManager bufManager)
        {
            this.model = model;
            this.Buffermanager = bufManager;
            foreach (MorphData morphData in model.MorphList.Morphes)
            {
                if (morphData.type == MorphType.Vertex)
                {
                    this.MorphList.Add(morphData.MorphName,new VertexMorphData(morphData));
                }
            }
        }

        public void ApplyMorphProgress(float frameNumber, IEnumerable<MorphMotion> morphMotions)
        {
            
            foreach (var morphMotion in morphMotions)
            {
                SetMorphProgress(morphMotion.GetMorphValue(frameNumber), morphMotion.MorphName);
            }
        }

        public bool ApplyMorphProgress(float progress, string morphName)
        {
            return SetMorphProgress(progress, morphName);
        }

        public void UpdateFrame()
        {
            ResetVertexPosition();
        }


        private void ResetVertexPosition()
        {
            foreach (uint i in this.movedVertex)
            {
                VertexData vertexData = this.model.VertexList.Vertexes[i];
                this.Buffermanager.InputVerticies[i].Position = new Vector4(vertexData.Position, 1f);
            }
            this.movedVertex=new HashSet<uint>();
        }


        private bool SetMorphProgress(float progress, string morphName)
        {
            if(!this.MorphList.ContainsKey(morphName))return false;
            VertexMorphData data = this.MorphList[morphName];
            foreach (VertexMorphOffset vertexMorph in data.MorphOffsets)
            {
                this.movedVertex.Add(vertexMorph.VertexIndex);
                this.Buffermanager.InputVerticies[vertexMorph.VertexIndex].Position += new Vector4(vertexMorph.PositionOffset*progress,0);
            }
            this.Buffermanager.NeedReset = true;
            return true;
        }

        private IBufferManager Buffermanager { get; set; }
    }
}
