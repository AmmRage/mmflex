using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
{
    public class BezierCurve
    {
        private const float Epsilon = 1.0e-3f;

        public Vector2 v1;

        public Vector2 v2;

        /// <summary>
        /// Advanced degree from a transition degree
        /// </summary>
        /// <param name="Progress">Advanced degree</param>
        /// <returns>Transition degree</returns>
        public float Evaluate(float Progress)
        {
            //Newton method approximation
            float t = CGHelper.Clamp(Progress, 0, 1);
            float dt;
            do
            {
                dt = -(fx(t) - Progress) / dfx(t);
                if (float.IsNaN(dt))
                    break;
                t += CGHelper.Clamp(dt, -1f, 1f);//To prevent moving dramatically, reaching a different solution for
            } while (Math.Abs(dt) > Epsilon);
            return CGHelper.Clamp(fy(t), 0f, 1f);//Just in case the fit between 0-1.
        }
        //function to calculate the FY (t)
        private float fy(float t)
        {
            //FY (t)=(1-t)^3*0+3*(1-t)^2*t*v1.y+3*(1-t)*t^2*v2.y+t^3*1
            return 3 * (1 - t) * (1 - t) * t *this.v1.Y + 3 * (1 - t) * t * t *this.v2.Y + t * t * t;
        }
        //function to calculate the FX (t)
        float fx(float t)
        {
            //FX (t)=(1-t)^3*0+3*(1-t)^2*t*v1.x+3*(1-t)*t^2*v2.x+t^3*1
            return 3 * (1 - t) * (1 - t) * t *this.v1.X + 3 * (1 - t) * t * t *this.v2.X + t * t * t;
        }
        //DFX/dtを計算する関数
        float dfx(float t)
        {
            //DFX (t)/dt=-6(1-t)*t*v1.x+3(1-t)^2*v1.x-3t^2*v2.x+6(1-t)*t*v2.x+3t^2
            return -6 * (1 - t) * t *this.v1.X + 3 * (1 - t) * (1 - t) *this.v1.X
                - 3 * t * t *this.v2.X + 6 * (1 - t) * t *this.v2.X + 3 * t * t;
        }
    }
}
