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
    /// Create a constraint class
    /// 约束
    /// </summary>
    internal class ConstraintFactory : System.IDisposable
    {
        /// <summary>
        /// Physics World
        /// </summary>
        private DiscreteDynamicsWorld dynamicsWorld;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dynamicsWorld">Physics World</param>
        public ConstraintFactory(DiscreteDynamicsWorld dynamicsWorld)
        {
            this.dynamicsWorld = dynamicsWorld;
        }

        /// <summary>
        /// Add to points in space constrained rigid body
        /// </summary>
        /// <param name="body">Rigid body</param>
        /// <param name="pivot">Constraint points</param>
        public void AddPointToPointConstraint(RigidBody body, ref Vector3 pivot)
        {
            this.dynamicsWorld.AddConstraint(new Point2PointConstraint(body, pivot));
        }

        /// <summary>
        /// Add a point constraint between rigid and rigid
        /// </summary>
        /// <param name="bodyA">A rigid body</param>
        /// <param name="bodyB">Rigid body B</param>
        /// <param name="pivotInA">Looked at from A rigid constraint points</param>
        /// <param name="pivotInB">Position of restraint from the rigid body B</param>
        public void AddPointToPointConstraint(RigidBody bodyA, RigidBody bodyB, ref Vector3 pivotInA, ref Vector3 pivotInB)
        {
            this.dynamicsWorld.AddConstraint(new Point2PointConstraint(bodyA, bodyB, pivotInA, pivotInB));
        }

        /// <summary>
        /// Between rigid and rigid six-axis spring add a constraint
        /// </summary>
        /// <param name="connectedBodyPair">A pair of rigid links</param>
        /// <param name="restriction">6-axis motion limits</param>
        /// <param name="stiffness">6-axis spring.</param>
        public void Add6DofSpringConstraint(Joint6ConnectedBodyPair connectedBodyPair, Joint6Restriction restriction, Joint6Stiffness stiffness)
        {
            var bodyA = connectedBodyPair.connectedBodyA.rigidBody;
            var bodyB = connectedBodyPair.connectedBodyB.rigidBody;
            var frameInA = connectedBodyPair.connectedBodyA.world;
            var frameInB = connectedBodyPair.connectedBodyB.world;
            var constraint = new Generic6DofSpringConstraint(bodyA, bodyB, frameInA, frameInB, true); // 第五引数の効果は謎。どちらでも同じ様に見える……。

            var c_p1 = restriction.movementRestriction.c_p1;
            var c_p2 = restriction.movementRestriction.c_p2;
            var c_r1 = restriction.rotationRestriction.c_r1;
            var c_r2 = restriction.rotationRestriction.c_r2;
            constraint.LinearLowerLimit = new Vector3(c_p1.X, c_p1.Y, c_p1.Z); // 型はベクトルだがベクトル量ではないのでZは反転しない。
            constraint.LinearUpperLimit = new Vector3(c_p2.X, c_p2.Y, c_p2.Z);
            constraint.AngularLowerLimit = new Vector3(c_r1.X, c_r1.Y, c_r1.Z);
            constraint.AngularUpperLimit = new Vector3(c_r2.X, c_r2.Y, c_r2.Z);

            SetStiffness(stiffness.translation.X, 0, constraint);
            SetStiffness(stiffness.translation.Y, 1, constraint);
            SetStiffness(stiffness.translation.Z, 2, constraint);
            SetStiffness(stiffness.rotation.X, 3, constraint);
            SetStiffness(stiffness.rotation.Y, 4, constraint);
            SetStiffness(stiffness.rotation.Z, 5, constraint);

            this.dynamicsWorld.AddConstraint(constraint);
        }

        /// <summary>
        /// Set the spring to the freedom of a captive one
        /// </summary>
        /// <param name="stiffness">The value of the spring</param>
        /// <param name="index">Index to specify the degrees of freedom (0~5。平行移動X, Y, Z, 回転移動X, Y, Zの順)</param>
        /// <param name="constraint">Captive</param>
        private void SetStiffness(float stiffness, int index, Generic6DofSpringConstraint constraint)
        {
            if (stiffness == 0.0f) return;
            constraint.EnableSpring(index, true);
            constraint.SetStiffness(index, stiffness);
        }

        /// <summary>
        /// To open the resource
        /// </summary>
        public void Dispose()
        {
            for (int i = this.dynamicsWorld.NumConstraints - 1; i >= 0; --i)
            {
                var constraint = this.dynamicsWorld.GetConstraint(i);
                this.dynamicsWorld.RemoveConstraint(constraint);
                constraint.Dispose();
            }
        }
    }
}
