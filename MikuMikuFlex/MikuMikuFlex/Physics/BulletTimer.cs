using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMF.Physics
{
    /// <summary>
    /// Elapsed time[ms]を計るクラス
    /// </summary>
    internal class BulletTimer
    {
        /// <summary>
        /// Watch
        /// </summary>
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// When I saw the clock the last time
        /// </summary>
        private long lastTime = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public BulletTimer()
        {
            this.stopWatch.Start();
        }

        /// <summary>
        /// Elapsed time since the last time this function call[ms]を得る
        /// </summary>
        /// <returns>Elapsed time[ms]</returns>
        public long GetElapsedTime()
        {
            var currentTime = this.stopWatch.ElapsedMilliseconds;
            var elapsedTime = currentTime - this.lastTime;
            this.lastTime = currentTime;
            return elapsedTime;
        }
    }
}
