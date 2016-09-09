using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace MMF.Physics
{
    /// <summary>
    /// BulletOf making world class
    /// </summary>
    internal class DynamicsWorldFactory : System.IDisposable
    {
        /// <summary>
        /// Initialize member variables
        /// </summary>
        private readonly DefaultCollisionConfiguration collisionConfiguration;
        private readonly CollisionDispatcher dispatcher;
        private readonly BroadphaseInterface overlappingPairCache;
        private readonly SequentialImpulseConstraintSolver solver;

        /// <summary>
        /// Constructor
        /// </summary>
        public DynamicsWorldFactory()
        {
            this.collisionConfiguration = new DefaultCollisionConfiguration();
            this.dispatcher = new CollisionDispatcher(this.collisionConfiguration);
            this.overlappingPairCache = new DbvtBroadphase();
            this.solver = new SequentialImpulseConstraintSolver();
        }

        /// BulletIn making the world
        /// </summary>
        /// <param name="gravity">Gravity</param>
        /// <returns>BulletOf the world</returns>
        public DiscreteDynamicsWorld CreateDynamicsWorld(SlimDX.Vector3 gravity)
        {
            var dynamicsWorld = new DiscreteDynamicsWorld(this.dispatcher, this.overlappingPairCache, this.solver, this.collisionConfiguration);
            dynamicsWorld.Gravity = gravity;
            return dynamicsWorld;
        }

        /// <summary>
        /// To open the resource
        /// </summary>
        public void Dispose()
        {
            this.solver.Dispose();
            this.overlappingPairCache.Dispose();
            this.dispatcher.Dispose();
            this.collisionConfiguration.Dispose();
        }

    }
}
