using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using SlimDX;

namespace MMF.Physics
{
    /// <summary>
    /// Making rigid class
    /// </summary>
    internal class RigidBodyFactory : System.IDisposable
    {
        /// <summary>
        ///Made resource open to the Array to manage the rigid body
        /// </summary>
        private AlignedCollisionShapeArray collisionShapes = new AlignedCollisionShapeArray();

        /// <summary>
        /// Physics World
        /// </summary>
        private DiscreteDynamicsWorld dynamicsWorld;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamicsWorld">Physics World</param>
        public RigidBodyFactory(DiscreteDynamicsWorld dynamicsWorld)
        {
            this.dynamicsWorld = dynamicsWorld;
        }

        /// <summary>
        /// Create rigid bodies
        /// </summary>
        /// <param name="collisionShape">Shape of rigid body</param>
        /// <param name="world">World transform of rigid body</param>
        /// <param name="rigidProperty">Rigid body physics</param>
        /// <param name="superProperty">Characteristics that transcend the physical operations</param>
        /// <returns></returns>
        public RigidBody CreateRigidBody(CollisionShape collisionShape, Matrix world, RigidProperty rigidProperty,
            SuperProperty superProperty)
        {
            var mass = superProperty.kinematic ? 0 : rigidProperty.mass;
            this.collisionShapes.Add(collisionShape);
            Vector3 localInertia = new Vector3(0, 0, 0); //局部惯性
            if (mass != 0) collisionShape.CalculateLocalInertia(mass, out localInertia);
            DefaultMotionState motionState = new DefaultMotionState(world);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, motionState, collisionShape, localInertia);
            RigidBody body = new RigidBody(rbInfo)
            {
                Restitution = rigidProperty.restitution,
                Friction = rigidProperty.friction
            };
            //设置阻尼
            body.SetDamping(rigidProperty.linear_damp, rigidProperty.angular_damp);
            float linearDamp = body.LinearDamping;
            float angularDamp = body.AngularDamping;
            if (superProperty.kinematic) body.CollisionFlags = body.CollisionFlags | CollisionFlags.KinematicObject;
            body.ActivationState = ActivationState.DisableDeactivation;
            this.dynamicsWorld.AddRigidBody(body, superProperty.group, superProperty.mask);
            return body;
        }

        /// <summary>
        /// To open the resource
        /// </summary>
        public void Dispose()
        {
            for (int i = this.dynamicsWorld.NumCollisionObjects - 1; i >= 0; --i)
            {
                CollisionObject obj = this.dynamicsWorld.CollisionObjectArray[i];
                RigidBody body = RigidBody.Upcast(obj);
                if (body != null && body.MotionState != null) body.MotionState.Dispose();
                this.dynamicsWorld.RemoveCollisionObject(obj);
                obj.Dispose();
            }
            for (int i = 0; i < this.collisionShapes.Count; ++i)
            {
                CollisionShape collisionShape = this.collisionShapes[i];
                this.collisionShapes[i] = null;
                collisionShape.Dispose();
            }
            this.collisionShapes.Dispose();
        }

    }
}
