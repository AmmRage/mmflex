using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMF.Physics
{
    /// <summary>
    /// Rigid body physics
    /// </summary>
    internal class RigidProperty
    {
        /// <summary>
        /// Mass
        /// </summary>
        public float mass { private set; get; }

        /// <summary>
        /// Coefficient of restitution
        /// </summary>
        public float restitution { private set; get; }

        /// <summary>
        /// Coefficient of friction
        /// </summary>
        public float friction { private set; get; }

        /// <summary>
        /// Movement damping
        /// </summary>
        public float linear_damp { private set; get; }

        /// <summary>
        /// Rotational damping
        /// </summary>
        public float angular_damp { private set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mass">Mass. When you zero in case of static rigid body that does not move. ����</param>
        /// <param name="restitution">Coefficient of restitution �ָ�ϵ��</param>
        /// <param name="friction">Coefficient of friction Ħ��ϵ��</param>
        /// <param name="linear_damp">Movement damping �˶�����</param>
        /// <param name="angular_damp">Rotational damping ��ת����</param>
        public RigidProperty(float mass = 0, float restitution = 0, float friction = 0.5f, float linear_damp = 0, float angular_damp = 0)
        {
            this.mass = mass;
            this.restitution = restitution;
            this.friction = friction;
            this.linear_damp = linear_damp;
            this.angular_damp = angular_damp;
        }
    }

}
