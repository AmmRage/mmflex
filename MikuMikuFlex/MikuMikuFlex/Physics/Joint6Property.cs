using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using BulletSharp;

namespace MMF.Physics
{
    /// <summary>
    /// Rigid leads to 6-axis joint
    /// </summary>
    internal class Joint6ConnectedBody
    {
        /// <summary>
        /// Rigid body
        /// </summary>
        public RigidBody rigidBody { private set; get; }

        /// <summary>
        /// World transformation matrix
        /// </summary>
        public Matrix world { private set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rigidBody">Rigid body</param>
        /// <param name="world">World transformation matrix (rigid local coordinate system) of the joint</param>
        public Joint6ConnectedBody(RigidBody rigidBody, Matrix world)
        {
            this.rigidBody = rigidBody;
            this.world = world;
        }
    }

    /// <summary>
    /// 6-axis joint movement restriction
    /// </summary>
    internal class Joint6MovementRestriction
    {
        /// <summary>
        /// Move limit 1
        /// </summary>
        public Vector3 c_p1 { private set; get; }

        /// <summary>
        /// Transfer limit 2
        /// </summary>
        public Vector3 c_p2 { private set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="c_p1">Move limit 1</param>
        /// <param name="c_p2">Transfer limit 2</param>
        public Joint6MovementRestriction(Vector3 c_p1, Vector3 c_p2)
        {
            this.c_p1 = c_p1;
            this.c_p2 = c_p2;
        }
    }

    /// <summary>
    /// 6-axis joint rotation limited
    /// </summary>
    internal class Joint6RotationRestriction
    {
        /// <summary>
        /// Rotation limit 1
        /// </summary>
        public Vector3 c_r1 { private set; get; }

        /// <summary>
        /// Rotation limit 2
        /// </summary>
        public Vector3 c_r2 { private set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="c_r1">Rotation limit 1</param>
        /// <param name="c_r2">Rotation limit 2</param>
        public Joint6RotationRestriction(Vector3 c_r1, Vector3 c_r2)
        {
            this.c_r1 = c_r1;
            this.c_r2 = c_r2;
        }
    }

    /// <summary>
    /// Spring stiffness
    /// </summary>
    internal class Joint6Stiffness
    {
        /// <summary>
        /// Translational components
        /// </summary>
        public Vector3 translation { private set; get; }

        /// <summary>
        /// Rotating component
        /// </summary>
        public Vector3 rotation { private set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="translation">Translational components</param>
        /// <param name="rotation">Rotating component</param>
        public Joint6Stiffness(Vector3 translation, Vector3 rotation)
        {
            this.translation = translation;
            this.rotation = rotation;
        }
    }

    /// <summary>
    /// A pair of rigid leads to 6-axis joint
    /// </summary>
    internal class Joint6ConnectedBodyPair
    {
        /// <summary>
        /// Rigid A lead to a six-axis joint
        /// </summary>
        public Joint6ConnectedBody connectedBodyA { private set; get; }

        /// <summary>
        /// Rigid body B leads to 6-axis joint
        /// </summary>
        public Joint6ConnectedBody connectedBodyB { private set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectedBodyA">Rigid A lead to a six-axis joint</param>
        /// <param name="connectedBodyB">Rigid body B leads to 6-axis joint</param>
        public Joint6ConnectedBodyPair(Joint6ConnectedBody connectedBodyA, Joint6ConnectedBody connectedBodyB)
        {
            this.connectedBodyA = connectedBodyA;
            this.connectedBodyB = connectedBodyB;
        }
    }

    /// <summary>
    /// 6-axis motion limits
    /// </summary>
    internal class Joint6Restriction
    {
        /// <summary>
        /// Movement restrictions
        /// </summary>
        public Joint6MovementRestriction movementRestriction { private set; get; }

        /// <summary>
        /// Rotation limit
        /// </summary>
        public Joint6RotationRestriction rotationRestriction { private set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="movementRestriction">Movement restrictions</param>
        /// <param name="rotationRestriction">Rotation limit</param>
        public Joint6Restriction(Joint6MovementRestriction movementRestriction, Joint6RotationRestriction rotationRestriction)
        {
            this.movementRestriction = movementRestriction;
            this.rotationRestriction = rotationRestriction;
        }
    }

}
