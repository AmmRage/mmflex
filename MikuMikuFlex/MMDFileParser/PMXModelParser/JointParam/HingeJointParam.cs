using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace MMDFileParser.PMXModelParser.JointParam
{
    public class HingeJointParam: JointParamBase
    {
        internal override void getJointParam(Stream fs, Header header)
        {
            this.RigidBodyAIndex = ParserHelper.getIndex(fs, header.RigidBodyIndexSize);
            this.RigidBodyBIndex = ParserHelper.getIndex(fs, header.RigidBodyIndexSize);
            this.Position = ParserHelper.getFloat3(fs);
            this.Rotation = ParserHelper.getFloat3(fs);
            Vector3 moveLimitationMin = ParserHelper.getFloat3(fs);
            Vector3 moveLimitationMax = ParserHelper.getFloat3(fs);
            Vector3 rotationLimitationMin = ParserHelper.getFloat3(fs);
            Vector3 rotationLimitationMax = ParserHelper.getFloat3(fs);
            Vector3 springMoveCoefficient = ParserHelper.getFloat3(fs);
            Vector3 springRotationCoefficient = ParserHelper.getFloat3(fs);
            this.Low = rotationLimitationMin.X;
            this.High = rotationLimitationMax.X;
            this.SoftNess = springMoveCoefficient.X;
            this.BiasFactor = springMoveCoefficient.Y;
            this.RelaxationFactor = springMoveCoefficient.Z;
            this.MotorEnabled = Math.Abs(springRotationCoefficient.X - 1) < 0.3f;
            this.TargetVelocity = springRotationCoefficient.Y;
            this.MaxMotorImpulse = springRotationCoefficient.Z;
        }

        public int RigidBodyAIndex { get; private set; }

        public int RigidBodyBIndex { get; private set; }

        public Vector3 Position { get; private set; }

        public Vector3 Rotation { get; private set; }

        public float Low { get; private set; }

        public float High { get; private set; }

        public float SoftNess { get; private set; }

        public float BiasFactor { get; private set; }

        public float RelaxationFactor { get; private set; }

        public bool MotorEnabled { get; private set; }

        public float TargetVelocity { get; private set; }

        public float MaxMotorImpulse { get; private set; }
    }
}
