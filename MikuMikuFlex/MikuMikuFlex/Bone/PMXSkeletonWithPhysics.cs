using MMDFileParser.PMXModelParser;
using MMF.Physics;

namespace MMF.Bone
{
    internal class PMXSkeletonWithPhysics : PMXSkeleton
    {
        private readonly PMXPhysicsTransformManager physicsTransformManager;

        public PMXSkeletonWithPhysics(ModelData model) : base(model)
        {
            this.physicsTransformManager = new PMXPhysicsTransformManager(this.Bone, 
                model.RigidBodyList.RigidBodies, 
                model.JointList.Joints);
            this.KinematicsProviders.Add(this.physicsTransformManager);
        }

        public override void Dispose()
        {
            this.physicsTransformManager.Dispose();
        }
    }
}