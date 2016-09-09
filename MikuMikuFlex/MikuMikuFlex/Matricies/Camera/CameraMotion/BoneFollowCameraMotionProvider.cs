using System;
using System.Linq;
using MMF.Bone;
using MMF.Matricies.Projection;
using MMF.Model.PMX;
using SlimDX;

namespace MMF.Matricies.Camera.CameraMotion
{
    /// <summary>
    /// Bourne tracking camera motion
    /// camera motion tracking model bone
    /// </summary>
    public class BoneFollowCameraMotionProvider:ICameraMotionProvider
    {
        /// <summary>
        /// Model of target tracking
        /// </summary>
        private readonly PMXModel followModel;

        /// <summary>
        /// Bone tracking
        /// </summary>
        private readonly PMXBone followBone;

        /// <summary>
        /// Distance between the camera and the bone
        /// Distance of camera and bone
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// Seen from the camera Z axis to rotate, whether or not
        /// </summary>
        public bool IsRotationZAxis { get; set; }

        /// <summary>
        /// Seen from the time of the initial state, from which?。
        /// (0,0,1)ならば前から、(0,0,-1)ならば後ろから
        /// 
        /// </summary>
        public Vector3 ViewFrom { get; set; }

        /// <summary>
        /// Constractor
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="boneName"></param>
        /// <param name="distance"></param>
        /// <param name="viewFrom"></param>
        /// <param name="rotationZaxis"></param>
        public BoneFollowCameraMotionProvider(PMXModel model,string boneName,float distance,Vector3 viewFrom,bool rotationZaxis=false)
        {
            this.followModel = model;
            var bones= (from bone in model.Skinning.Bone where bone.BoneName == boneName select bone).ToArray();
            if (bones.Length == 0)
            {
                throw new InvalidOperationException(string.Format("ボーン\"{0}\"は見つかりませんでした。",boneName));
            }
            this.followBone = bones[0];
            this.Distance = distance;
            this.IsRotationZAxis = rotationZaxis;
            this.ViewFrom = viewFrom;
        }


        void ICameraMotionProvider.UpdateCamera(CameraProvider cp, IProjectionMatrixProvider proj)
        {
            //Ask the world coordinates of the bone matrix
            Matrix bonePoseMatrix = this.followBone.GlobalPose*Matrix.Scaling(this.followModel.Transformer.Scale)*
                                    Matrix.RotationQuaternion(this.followModel.Transformer.Rotation)*
                                    Matrix.Translation(this.followModel.Transformer.Position);
            Vector3 bonePosition = Vector3.TransformCoordinate(this.followBone.Position, bonePoseMatrix);
            Vector3 la2cp = Vector3.TransformNormal(-this.ViewFrom, bonePoseMatrix);//Fixation and then head to camera location vector
            la2cp.Normalize();
            cp.CameraPosition = bonePosition + this.Distance*la2cp;
            cp.CameraLookAt = bonePosition;
            if (this.IsRotationZAxis)
            {
                Vector3 newUp = Vector3.TransformNormal(new Vector3(0, 1, 0), bonePoseMatrix);
                newUp.Normalize();
                cp.CameraUpVec = newUp;
            }
        }
    }
}
