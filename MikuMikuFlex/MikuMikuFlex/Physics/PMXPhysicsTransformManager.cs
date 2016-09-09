using System.Collections.Generic;
using System.Diagnostics;
using BulletSharp;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.JointParam;
using MMF.Bone;
using MMF.Model;
using MMF.Morph;
using SlimDX;
using IDisposable = System.IDisposable;

namespace MMF.Physics
{
	/// <summary>
	/// Bone physical operations manager
	/// </summary>
	public class PMXPhysicsTransformManager:ITransformUpdater,IDisposable
    {
        /// <summary>
        /// Rigid data use again at a later time, that you first calculate
        /// </summary>
		class TempRigidBodyData 
        {
            /// <summary>
            /// Initial position
            /// </summary>
			public readonly Vector3 position;

            /// <summary>
            /// Initial orientation matrix
            /// </summary>
			public readonly Matrix init_matrix;

            /// <summary>
            /// Offset matrix
            /// </summary>
			public readonly Matrix offset_matrix;

            /// <summary>
            /// Bone index
            /// </summary>
			public readonly int boneIndex;
            
            /// <summary>
            /// Physics calculation type
            /// </summary>
			public readonly PhysicsCalcType physicsCalcType;
            
            /// <summary>
            /// Rigid-body shape
            /// </summary>
			public readonly RigidBodyShape shape;

            /// <summary>
            /// Temporary rigid data
            /// </summary>
            /// <param name="rigidBodyData">Rigid data</param>
			public TempRigidBodyData(RigidBodyData rigidBodyData)
            {
                this.position = rigidBodyData.Position;
				var r = rigidBodyData.Rotation;
                this.init_matrix = Matrix.RotationYawPitchRoll(r.Y, r.X, r.Z) * Matrix.Translation(this.position);
                this.offset_matrix = Matrix.Invert(this.init_matrix);
                this.boneIndex = rigidBodyData.BoneIndex;
                this.physicsCalcType = rigidBodyData.PhysicsCalcType;
                this.shape = rigidBodyData.Shape;
			}
		}

        /// <summary>
        /// Bone
        /// </summary>
		private PMXBone[] bones;

        /// <summary>
        /// Temporary list of rigid data
        /// </summary>
        private List<TempRigidBodyData> tempRigidBodyData = new List<TempRigidBodyData>();
		
        /// <summary>
		/// BulletPhysical operation
		/// </summary>
        private BulletManager bulletManager;
        
        /// <summary>
        /// BulletOf rigid bodies list
        /// </summary>
        private List<RigidBody> rigidBodies = new List<RigidBody>();

	    private static bool physicsAsserted;

	    /// <summary>
        /// To create a rigid
        /// </summary>
        /// <param name="rigidBodyData">Rigid data</param>
        private void CreateRigid(List<RigidBodyData> rigidBodyData)
        {
            foreach (var r in rigidBodyData)
            {
                var tempRigidBodyData = new TempRigidBodyData(r);
                //Debug.WriteLine("名字: {0}, 质量: {1}， 排斥力: {2}， 摩擦: {3}， 移动衰减: {4}， 旋转衰减: {5}",
                //    r.RigidBodyName, r.Mass, r.Repulsion, r.Friction, r.MoveAttenuation, r.Rotation);
                var init_matrix = tempRigidBodyData.init_matrix;
                this.tempRigidBodyData.Add(tempRigidBodyData);
                RigidBody rigidBody = null;
                CollisionShape collisionShape;
                switch (r.Shape)
                {
                    case RigidBodyShape.Sphere: // 球体
                        collisionShape = new SphereShape(r.Size.X);
                        break;
                    case RigidBodyShape.Box:        // box
                        collisionShape = new BoxShape(r.Size.X, r.Size.Y, r.Size.Z);
                        break;
                    case RigidBodyShape.Capsule:    // capsule
                        collisionShape = new CapsuleShape(r.Size.X, r.Size.Y);
                        break;
                    default:    //  Exception handling
                        throw new System.Exception("Invalid rigid body data");
                }
                //质量， 排斥力， 摩擦， 移动衰减， 旋转衰减
                var rigidProperty = new RigidProperty(r.Mass, r.Repulsion, r.Friction, r.MoveAttenuation, r.RotationAttenuation);
                var superProperty = new SuperProperty(r.PhysicsCalcType == PhysicsCalcType.Static,  (CollisionFilterGroups)(1 << r.RigidBodyGroup), (CollisionFilterGroups)r.UnCollisionGroupFlag);
                rigidBody = this.bulletManager.CreateRigidBody(collisionShape, init_matrix, rigidProperty, superProperty);
                this.rigidBodies.Add(rigidBody);
            }
        }

        /// <summary>
        /// Joint production
        /// </summary>
        /// <param name="jointData_s">Joint data list</param>
        private void CreateJoint(List<JointData> jointData_s) 
        {
			foreach (var jointData in jointData_s)
            {
				var jointParam = (Spring6DofJointParam)jointData.JointParam;
                var connectedBodyPair = CreateConnectedBodyPair(jointParam);
                var restriction = CreateRestriction(jointParam);
                var stiffness = CreateStiffness(jointParam);
                this.bulletManager.Add6DofSpringConstraint(connectedBodyPair, restriction, stiffness);
			}
		}

        /// <summary>
        /// Make a pair of 6-axis joints connecting rigid body
        /// </summary>
        /// <param name="jointParam">6-axis joints parameters</param>
        /// <returns>A pair of 6-axis joints connecting rigid body</returns>
        private Joint6ConnectedBodyPair CreateConnectedBodyPair(Spring6DofJointParam jointParam)
        {
            var bodyA = this.rigidBodies[jointParam.RigidBodyAIndex];
            var bodyAworld_inv = Matrix.Invert(this.bulletManager.GetWorld(bodyA));
            var bodyB = this.rigidBodies[jointParam.RigidBodyBIndex];
            var bodyBworld_inv = Matrix.Invert(this.bulletManager.GetWorld(bodyB));
            var jointRotation = jointParam.Rotation;
            var jointPosition = jointParam.Position;
            var jointWorld = Matrix.RotationYawPitchRoll(jointRotation.Y, jointRotation.X, jointRotation.Z) * Matrix.Translation(jointPosition.X, jointPosition.Y, jointPosition.Z);
            var connectedBodyA = new Joint6ConnectedBody(bodyA, jointWorld * bodyAworld_inv);
            var connectedBodyB = new Joint6ConnectedBody(bodyB, jointWorld * bodyBworld_inv);
            return new Joint6ConnectedBodyPair(connectedBodyA, connectedBodyB);
        }

        /// <summary>
        /// Make a 6-axis mobility limited
        /// </summary>
        /// <param name="jointParam">6-axis joints parameters</param>
        /// <returns>6-axis motion limits</returns>
        private Joint6Restriction CreateRestriction(Spring6DofJointParam jointParam)
        {
            var movementRestriction = new Joint6MovementRestriction(jointParam.MoveLimitationMin, jointParam.MoveLimitationMax);
            var rotationRestriction = new Joint6RotationRestriction(jointParam.RotationLimitationMin, jointParam.RotationLimitationMax);
            return new Joint6Restriction(movementRestriction, rotationRestriction);
        }

        private Joint6Stiffness CreateStiffness(Spring6DofJointParam jointParam)
        {
            return new Joint6Stiffness(jointParam.SpringMoveCoefficient, jointParam.SpringRotationCoefficient);
        }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="bones">Banlist</param>
		/// <param name="rigidBodies">Rigid list</param>
		public PMXPhysicsTransformManager(PMXBone[] bones, List<RigidBodyData> rigidBodyData, 
            List<JointData> jointData)
        {
			this.bones = bones;
			var gravity = new Vector3(0, -9.8f*2.5f, 0);
		    this.bulletManager = new BulletManager(gravity);
			CreateRigid(rigidBodyData);
			CreateJoint(jointData);
		}

		/// <summary>
		/// To open the resource
		/// </summary>
		public void Dispose()
        {
            this.tempRigidBodyData.Clear();
            this.bulletManager.Dispose();
            this.rigidBodies.Clear();
		}

	    /// <summary>
	    /// ITransformUpdaterThe implementation of the Member
	    /// </summary>
	    public bool UpdateTransform()
        {
            // In the processing of the following sets all those rigid body type is bone follow the bone matrix
            // Set rigid type in after the physical operation is a thing of the physical operation, after which the rigid body type
            // Set the things of physical + bone alignment. Thing to pay attention to the order in which the setting and calculation.

            // Set the bone matrix in the bone follow-up type of rigid body
            for (int i = 0; i < this.rigidBodies.Count; ++i) 
            { 
				var t = this.tempRigidBodyData[i];
                // If the rigid type in there related bone is bone follow-up
                if (t.boneIndex != -1 && t.physicsCalcType == PhysicsCalcType.Static)
                {
                    // Setting process bone matrix
                    var bone = this.bones[t.boneIndex];
					var rigidMat = t.init_matrix*bone.GlobalPose;
                    this.bulletManager.MoveRigidBody(this.rigidBodies[i], rigidMat);
				}
			}
            // Physics simulation
            this.bulletManager.StepSimulation();

            // Set the bone in accordance with the result of the physical operation
            for (int i = 0; i < this.rigidBodies.Count; ++i)
            {
				var t = this.tempRigidBodyData[i];
                // Does not process when no related bone
                if (t.boneIndex != -1)
                {
                    var bone = this.bones[t.boneIndex];
                    if (bone.Parent == null)
                        continue;
                    var globalPose = t.offset_matrix *this.bulletManager.GetWorld(this.rigidBodies[i]);
                    if (float.IsNaN(globalPose.M11))
                    {
                        if (!physicsAsserted)
                            Debug.WriteLine("The results of the physical operation was output incorrect results. Please review the setting of \nPMX. There is a possibility that not well motion operation.");
                        physicsAsserted = true;
                        continue;
                    }
                    var localPose = globalPose * Matrix.Invert(bone.Parent.GlobalPose);
                    var mat = Matrix.Translation(bone.Position) * localPose * Matrix.Translation(-bone.Position);
                    bone.Translation = new Vector3(mat.M41, mat.M42, mat.M43);
                    bone.Rotation = Quaternion.RotationMatrix(mat);
                    bone.UpdateGrobalPose();
				}
			}

            // Set the position the amount of movement of the bone to position the amount of movement of the bone alignment type of rigid body
            for (int i = 0; i < this.rigidBodies.Count; ++i)
            { 
				var t = this.tempRigidBodyData[i];
                // Set the position movement amount in the case of physical + bone alignment bone is a rigid body type in there related bone
                if (t.boneIndex != -1 && t.physicsCalcType == PhysicsCalcType.BoneAlignment)
                {
					var bone = this.bones[t.boneIndex];
					var v = new Vector3(bone.GlobalPose.M41, bone.GlobalPose.M42, bone.GlobalPose.M43); // The amount of movement of the bone
                    var p = new Vector3(t.init_matrix.M41, t.init_matrix.M42, t.init_matrix.M43) + v;
					var m = this.bulletManager.GetWorld(this.rigidBodies[i]);
					m.M41 = p.X; m.M42 = p.Y; m.M43 = p.Z;
                    this.bulletManager.MoveRigidBody(this.rigidBodies[i], m);
				}
			}
            return false;
	    }
	}
}
