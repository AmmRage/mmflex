using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace MMDFileParser.PMXModelParser.JointParam
{
    public class Basic6DofJointParam: JointParamBase
    {
        internal override void getJointParam(Stream fs, Header header)
        {
            this.RigidBodyAIndex = ParserHelper.getIndex(fs, header.RigidBodyIndexSize);
            this.RigidBodyBIndex = ParserHelper.getIndex(fs, header.RigidBodyIndexSize);
            this.Position = ParserHelper.getFloat3(fs);
            this.Rotation = ParserHelper.getFloat3(fs);
            this.MoveLimitationMin = ParserHelper.getFloat3(fs);
            this.MoveLimitationMax = ParserHelper.getFloat3(fs);
            this.RotationLimitationMin = ParserHelper.getFloat3(fs);
            this.RotationLimitationMax = ParserHelper.getFloat3(fs);
            ParserHelper.getFloat3(fs);//6 DOF parallel move, rotate spring constant is not valid because the read and seek
            ParserHelper.getFloat3(fs);
        }

        public int RigidBodyAIndex { get; private set; }

        public int RigidBodyBIndex { get; private set; }

        public Vector3 Position { get; private set; }

        public Vector3 Rotation { get; private set; }

        public Vector3 MoveLimitationMin { get; private set; }

        public Vector3 MoveLimitationMax { get; private set; }

        public Vector3 RotationLimitationMin { get; private set; }

        public Vector3 RotationLimitationMax { get; private set; }


    }
}
