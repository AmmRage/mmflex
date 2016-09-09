using System;
using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using SlimDX;

namespace MMF.Bone
{
    /// <summary>
    ///     Bone class
    /// </summary>
    public class PMXBone : IBone
    {

        private readonly ISkinningProvider skinning;

        /// <summary>
        ///     The index of the bone
        /// </summary>
        public int BoneIndex;

        /// <summary>
        ///     Bone name
        /// </summary>
        public string BoneName;

        /// <summary>
        ///     Child born
        /// </summary>
        public List<PMXBone> Children = new List<PMXBone>();

        public Vector3 DefaultLocalX;
        public Vector3 DefaultLocalY;
        public Vector3 DefaultLocalZ;

        /// <summary>
        ///     Parent bone
        /// </summary>
        public PMXBone Parent;

        /// <summary>
        ///     The original local position vector
        /// </summary>
        public Vector3 Position;

        /// <summary>
        ///     Parallel movement position vector
        /// </summary>
        public Vector3 Translation { get; set; }

        public bool isLocalAxis;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bones"></param>
        /// <param name="index"></param>
        /// <param name="layer"></param>
        /// <param name="skinning"></param>
        public PMXBone(List<BoneData> bones, int index, int layer, ISkinningProvider skinning)
        {
            this.skinning = skinning;
            BoneData me = bones[index]; //This bone
            skinning.Bone[index] = this;
            this.BoneIndex = index;
            this.Position = me.Position;
            this.BoneName = me.BoneName;

            this.isLocalAxis = me.isLocalAxis;
            this.PhysicsOrder = me.transformAfterPhysics ? PhysicsOrder.After : PhysicsOrder.Before;
            this.Layer = layer;
            if (this.isLocalAxis)
            {
                this.DefaultLocalX = me.DimentionXDirectionVector;
                this.DefaultLocalY = Vector3.Cross(me.DimentionZDirectionVector, this.DefaultLocalX);
                this.DefaultLocalZ = Vector3.Cross(this.DefaultLocalX, this.DefaultLocalY);
            }
            if (me.isIK) //For IK bones
            {
                skinning.IkBone.Add(this);
                this.isIK = true;
                this.RotationLimited = me.IKLimitedRadian;
                this.targetBoneIndex = me.IKTargetBoneIndex;
                this.Iterator = me.IKLoopNumber;
                foreach (IkLinkData ikLink in me.ikLinks)
                {
                    this.ikLinks.Add(new IkLink(skinning, ikLink));
                }
            }
            this.isRotateProvided = me.isRotateProvided;
            this.isMoveProvided = me.isMoveProvided;
            if (me.ProvidedParentBoneIndex == -1)
            {
                this.isRotateProvided = this.isMoveProvided = false;
            }
            if (this.isMoveProvided || this.isRotateProvided)
            {
                this.ProvideParentBone = me.ProvidedParentBoneIndex;
                this.ProvidedRatio = me.ProvidedRatio;
            }
            else
            {
                this.ProvideParentBone = -1;
            }
            for (int i = 0; i < bones.Count; i++)
            {
                BoneData bone = bones[i];
                if (bone.ParentBoneIndex == index)
                {
                    PMXBone child = new PMXBone(bones, i, layer + 1, skinning);
                    AddChild(child);
                }
            }
        }

        /// <summary>
        ///     Rotation matrix
        /// </summary>
        public Quaternion Rotation
        {
            get { return this.rotation; }
            set
            {
                this.rotation = value;
                this.rotation.Normalize();
            }
        }

        /// <summary>
        ///     Transform hierarchy
        /// </summary>
        public int Layer { get; private set; }

        /// <summary>
        ///     Physical order
        /// </summary>
        public PhysicsOrder PhysicsOrder { get; private set; }


        public Vector3 LocalX
        {
            get { return Vector3.TransformNormal(this.DefaultLocalX, Matrix.RotationQuaternion(this.Rotation)); }
        }

        public Vector3 LocalY
        {
            get { return Vector3.TransformNormal(this.DefaultLocalY, Matrix.RotationQuaternion(this.Rotation)); }
        }

        public Vector3 LocalZ
        {
            get { return Vector3.TransformNormal(this.DefaultLocalZ, Matrix.RotationQuaternion(this.Rotation)); }
        }


        /// <summary>
        ///     Local pose
        /// </summary>
        public Matrix LocalPose { get; set; }

        /// <summary>
        ///     Global pose
        /// </summary>
        public Matrix GlobalPose { get; private set; }

        #region IKAbout the item

        private readonly int targetBoneIndex;

        /// <summary>
        ///     Repeated number of operations
        /// </summary>
        public int Iterator;

        /// <summary>
        ///     Rotation limit per operation
        /// </summary>
        public float RotationLimited;

        /// <summary>
        ///     IKList of links
        /// </summary>
        public List<IkLink> ikLinks = new List<IkLink>();

        /// <summary>
        ///     This bone is IK bones?
        /// </summary>
        public bool isIK = false;

        private Quaternion rotation;

        public PMXBone IkTargetBone
        {
            get { return this.skinning.Bone[this.targetBoneIndex]; }
        }

        #endregion

        #region Grant topics

        public bool isMoveProvided { get; private set; }

        public bool isRotateProvided { get; private set; }

        public int ProvideParentBone { get; private set; }

        public float ProvidedRatio { get; private set; }

        #endregion

        /// <summary>
        ///     Adds a child articulated。
        /// </summary>
        /// <param name="child">To add a child articulated</param>
        public void AddChild(PMXBone child)
        {
            this.Children.Add(child);
            child.Parent = this;
        }

        public void UpdateGrobalPose()
        {
            this.LocalPose = Matrix.Translation(-this.Position)*Matrix.RotationQuaternion(this.Rotation)*Matrix.Translation(this.Translation)*Matrix.Translation(this.Position);

            this.GlobalPose = this.LocalPose;
            if (this.Parent != null) //When this element is not the root
            {
                this.GlobalPose *= this.Parent.GlobalPose;
            }
            // Parallel.ForEach(Children, (item) => item.UpdateGrobalPose());
            foreach (var child in this.Children)
                child.UpdateGrobalPose();
        }
    }
}