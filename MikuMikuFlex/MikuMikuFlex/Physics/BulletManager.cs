using BulletSharp;
using SlimDX;

namespace MMF.Physics
{
    /// <summary>
    /// Wrapper class for physics
    /// </summary>
    internal class BulletManager : System.IDisposable
    {
        /// <summary>
        /// BulletOf the world
        /// </summary>
        private DiscreteDynamicsWorld dynamicsWorld;

        /// <summary>
        /// BulletOf making world class
        /// </summary>
        private DynamicsWorldFactory dynamicsWorldFactory = new DynamicsWorldFactory();

        /// <summary>
        /// Making rigid class
        /// </summary>
        private RigidBodyFactory rigidBodyFactory;

        /// <summary>
        /// Create a constraint class
        /// </summary>
        private ConstraintFactory constraintFactory;

        /// <summary>
        /// Class elapsed time
        /// </summary>
        private BulletTimer bulletTimer = new BulletTimer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gravity">Gravity</param>
        public BulletManager(Vector3 gravity)
        {
            this.dynamicsWorld = this.dynamicsWorldFactory.CreateDynamicsWorld(gravity);
            this.rigidBodyFactory = new RigidBodyFactory(this.dynamicsWorld);
            this.constraintFactory = new ConstraintFactory(this.dynamicsWorld);
        }

        /// <summary>
        /// Whether or not you open resources
        /// </summary>
        private bool isDisposed = false;

        /// <summary>
        /// To open the resource
        /// </summary>
        public void Dispose()
        {
            if (this.isDisposed) return;
            this.constraintFactory.Dispose();
            this.rigidBodyFactory.Dispose();
            this.dynamicsWorld.Dispose();
            this.dynamicsWorldFactory.Dispose();
            this.isDisposed = true;
        }

        /// <summary>
        /// Create rigid bodies
        /// </summary>
        /// <param name="collisionShape">Shape of rigid body</param>
        /// <param name="world">World transform of rigid body</param>
        /// <param name="rigidProperty">Rigid body physics</param>
        /// <param name="superProperty">Characteristics that transcend the physical operations</param>
        /// <returns>Rigid body</returns>
        public RigidBody CreateRigidBody(CollisionShape collisionShape, Matrix world, RigidProperty rigidProperty, SuperProperty superProperty)
        {
            return this.rigidBodyFactory.CreateRigidBody(collisionShape, world, rigidProperty, superProperty);
        }

        /// <summary>
        /// Add to points in space constrained rigid body
        /// </summary>
        /// <param name="body">Rigid body</param>
        /// <param name="pivot">Constraint points</param>
        public void AddPointToPointConstraint(RigidBody body, ref Vector3 pivot)
        {
            this.constraintFactory.AddPointToPointConstraint(body, ref pivot);
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
            this.constraintFactory.AddPointToPointConstraint(bodyA, bodyB, ref pivotInA, ref pivotInB);
        }

        /// <summary>
        /// Between rigid and rigid six-axis spring add a constraint
        /// </summary>
        /// <param name="connectedBodyPair">A pair of rigid links</param>
        /// <param name="restriction">6-axis motion limits</param>
        /// <param name="stiffness">6-axis spring.</param>
        public void Add6DofSpringConstraint(Joint6ConnectedBodyPair connectedBodyPair, Joint6Restriction restriction, Joint6Stiffness stiffness)
        {
            this.constraintFactory.Add6DofSpringConstraint(connectedBodyPair, restriction, stiffness);
        }

        /// <summary>
        /// A moving rigid body kinematics
        /// </summary>
        /// <param name="body">Rigid body</param>
        /// <param name="world">World transformation matrix</param>
        public void MoveRigidBody(RigidBody body, Matrix world)
        {
            body.MotionState.WorldTransform = world;
        }

        /// <summary>
        /// Advances in the physics world time 
        /// </summary>
        public void StepSimulation()
        {
            var elapsedTime = this.bulletTimer.GetElapsedTime(); //[ms]
            this.dynamicsWorld.StepSimulation(elapsedTime / 1000f, 10);
        }

        /// <summary>
        /// The rigid gets physical result world matrix する
        /// </summary>
        /// <param name="body">Rigid body</param>
        /// <returns>World transformation matrix</returns>
        public Matrix GetWorld(RigidBody body)
        {
            return body.MotionState.WorldTransform;
        }

    }
}