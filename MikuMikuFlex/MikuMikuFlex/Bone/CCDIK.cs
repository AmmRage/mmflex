using System;
using System.Collections.Generic;
using System.Diagnostics;
using MMF.Model;
using MMF.Morph;
using MMF.Utility;
using SlimDX;

namespace MMF.Bone
{
    /// <summary>
    ///  CCDIKIn the class to update the IK bones
    /// </summary>
    public class CCDIK : ITransformUpdater
    {
        /// <summary>
        /// IKList of bones
        /// </summary>
        private List<PMXBone> IKbones;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="IKbones">IKList of bones</param>
        public CCDIK(List<PMXBone> IKbones)
        {
            this.IKbones = IKbones;
        }

        /// <summary>
        ///  ITransformUpdaterImplementation of a Member
        /// </summary>
        public bool UpdateTransform()
        {
            foreach (PMXBone IKbone in this.IKbones) UpdateEachIKBoneTransform(IKbone);
            return false;
        }

        /// <summary>
        /// Update each IK bone
        /// </summary>
        private void UpdateEachIKBoneTransform(PMXBone IKbone)
        {
            // ループ回数のリセット
            foreach (var link in IKbone.ikLinks) link.loopCount = 0;

            // 決められた回数IKループを繰り返す
            for (int it = 0; it < IKbone.Iterator; it++) IKloop(IKbone);
        }

        /// <summary>
        /// IKCalculation loop
        /// </summary>
        /// <param name="IKbone">IKBone</param>
        private void IKloop(PMXBone IKbone)
        {
            var effector = IKbone.IkTargetBone;
            var TargetGlobalPos = Vector3.TransformCoordinate(IKbone.Position, IKbone.GlobalPose);
            foreach (var ikLink in IKbone.ikLinks)
            {
                var link2Effector = GetLink2Effector(ikLink, effector);
                var link2Target = GetLink2Target(ikLink, TargetGlobalPos);
                IKLinkCalc(ikLink, link2Effector, link2Target, IKbone.RotationLimited);
            }
        }


        /// <summary>
        /// IKTo get the link based effector 1 position
        /// </summary>
        /// <param name="ikLink">IKLink</param>
        /// <param name="effector">Effector</param>
        /// <returns>IKLink based effector 1 position</returns>
        private Vector3 GetLink2Effector(IkLink ikLink, PMXBone effector)
        {
            var ToLinkLocal = Matrix.Invert(ikLink.ikLinkBone.GlobalPose);
            var effectorPos = Vector3.TransformCoordinate(effector.Position, effector.GlobalPose * ToLinkLocal); //●
            return Vector3.Normalize(effectorPos - ikLink.ikLinkBone.Position);
        }

        /// <summary>
        /// IKTo get the link relative target position
        /// </summary>
        /// <param name="ikLink">IKLink</param>
        /// <param name="TargetGlobalPos">Global relative target position</param>
        /// <returns>IKTarget position of a standard link</returns>
        private Vector3 GetLink2Target(IkLink ikLink, Vector3 TargetGlobalPos)
        {
            var ToLinkLocal = Matrix.Invert(ikLink.ikLinkBone.GlobalPose);
            Vector3 targetPos;
            Vector3.TransformCoordinate(ref TargetGlobalPos, ref ToLinkLocal, out targetPos);
            return Vector3.Normalize(targetPos - ikLink.ikLinkBone.Position);
        }

        /*
         *CCD-IKのIKLink計算の仕組み
         *-◎-◎-◎-●
         * ↑関節      ▲←IKターゲット(MMD内ではIKBoneと呼ぶ)。関節を近づけたい目標
         * (関節内の◎にあたる、IKで曲げる途中の関節をIKリンク、●にあたる関節の終点をエフェクタ(MMD内ではターゲットボーン)
         * とする。
         * 1)まず、◎-●の方向ベクタを求める。ベクトルA
         * 2)次に、◎-▲の方向ベクタを求める。ベクトルB
         * 3)∠●◎▲における最短回転角度を求める。arccos(A・B)よりラジアン単位で求まる
         * 4)２つの方向ベクトルの外積から回転軸を求める
         */
        /// <summary>
        /// IKLinkCalculation
        /// </summary>
        /// <param name="ikLink">IKLink</param>
        /// <param name="link2Effector">IKLink based effector position</param>
        /// <param name="link2Target">IKTarget position of a standard link</param>
        /// <param name="RotationLimited">The absolute value of the upper and lower limits for the angle of rotation</param>
        private void IKLinkCalc(IkLink ikLink, Vector3 link2Effector, Vector3 link2Target, float RotationLimited)
        {
            //To determine the rotation angle
            var dot = Vector3.Dot(link2Effector, link2Target);
            if (dot > 1f) dot = 1f;
            var rotationAngle = ClampFloat((float)Math.Acos(dot), RotationLimited);
            if (float.IsNaN(rotationAngle)) return;
            if (rotationAngle <= 1.0e-3f) return;

            //Ask the axis
            var rotationAxis = Vector3.Cross(link2Effector, link2Target);
            ikLink.loopCount++;

            //Builds a matrix that rotates around axis。
            var rotation = Quaternion.RotationAxis(rotationAxis, rotationAngle);
            rotation.Normalize();
            ikLink.ikLinkBone.Rotation = rotation * ikLink.ikLinkBone.Rotation;

            #region Rotation amount limit
            RestrictRotation(ikLink);
            #endregion

            ikLink.ikLinkBone.UpdateGrobalPose();
        }

        /// <summary>
        /// Clamp in the absolute value of a number of upper and lower bounds
        /// </summary>
        /// <param name="f">Number of the clamp</param>
        /// <param name="limit">The absolute value of the upper and lower limits</param>
        /// <returns>Number of clamped</returns>
        private float ClampFloat(float f, float limit)
        {
            return Math.Max(Math.Min(f, limit), -limit);
        }

        /// <summary>
        /// To limit the amount of rotation
        /// </summary>
        /// <param name="ikLink">IKLink</param>
        private void RestrictRotation(IkLink ikLink)
        {
            if (!ikLink.isLimited) return;

            float xRotation, yRotation, zRotation;
            var type = SplitRotation(ikLink.ikLinkBone.Rotation, out xRotation, out yRotation, out zRotation);
            var clamped = Vector3.Clamp(new Vector3(xRotation, yRotation, zRotation).NormalizeEular(), ikLink.minRot, ikLink.maxRot);
            xRotation = clamped.X;
            yRotation = clamped.Y;
            zRotation = clamped.Z;
            switch (type)
            {
                case 0:
                    ikLink.ikLinkBone.Rotation = Quaternion.RotationMatrix(Matrix.RotationX(xRotation) * Matrix.RotationY(yRotation) * Matrix.RotationZ(zRotation));
                    break;
                case 1:
                    ikLink.ikLinkBone.Rotation = Quaternion.RotationMatrix(Matrix.RotationY(yRotation) * Matrix.RotationZ(zRotation) * Matrix.RotationX(xRotation));
                    break;
                case 2:
                    ikLink.ikLinkBone.Rotation = Quaternion.RotationYawPitchRoll(yRotation, xRotation, zRotation);
                    break;
            }
        }

        /// <summary>
        /// //X,Y,Z軸回転量に分解する
        /// </summary>
        /// <param name="Rotation">Amount of rotation</param>
        /// <param name="xRotation">XRotation axis</param>
        /// <param name="yRotation">YRotation axis</param>
        /// <param name="zRotation">ZRotation axis</param>
        /// <returns>Decomposition patterns</returns>
        private int SplitRotation(Quaternion Rotation, out float xRotation, out float yRotation, out float zRotation)
        {
            if (CGHelper.FactoringQuaternionXYZ(Rotation, out xRotation, out yRotation, out zRotation)) return 0;
            if (CGHelper.FactoringQuaternionYZX(Rotation, out yRotation, out zRotation, out xRotation)) return 1;
            CGHelper.FactoringQuaternionZXY(Rotation, out zRotation, out xRotation, out yRotation);
            return 2;
        }


    }
}