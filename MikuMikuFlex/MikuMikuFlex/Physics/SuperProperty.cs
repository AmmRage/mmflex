using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace MMF.Physics
{
    /// <summary>
    /// Characteristics that transcend the physical operations
    /// </summary>
    internal class SuperProperty
    {
        /// <summary>
        /// 运动
        /// </summary>
        public bool kinematic { private set; get; }
        /// <summary>
        /// 碰撞过滤器组
        /// </summary>
        public CollisionFilterGroups group { private set; get; }
        /// <summary>
        /// 碰撞过滤器组
        /// </summary>
        public CollisionFilterGroups mask { private set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kinematic">Whether or not affected by physics Kinematic rigid</param>
        /// <param name="group">Hit Group No's.</param>
        /// <param name="mask">No other impact group collide with itself.</param>
        public SuperProperty(bool kinematic = false, CollisionFilterGroups group = CollisionFilterGroups.DefaultFilter, CollisionFilterGroups mask = CollisionFilterGroups.AllFilter)
        {
            this.kinematic = kinematic;
            this.group = group;
            this.mask = mask;
        }
    }

}
