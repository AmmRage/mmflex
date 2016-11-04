using System.Diagnostics;

namespace MMF.Motion
{
    /// <summary>
    ///     About motion timer
    /// </summary>
    public class MotionTimer
    {
        private readonly RenderContext _context;

         
        /// <summary>
        /// The number of frames per second around
        /// Typically 30。
        /// </summary>
        public int MotionFramePerSecond = 30;

        /// <summary>
        /// Timer refresh rate
        /// Perfect fps
        /// Generally, 60.
        /// </summary>
        public int TimerPerSecond = 300;

        public float ElapesedTime { get; private set; }

        public static Stopwatch stopWatch;

        private long lastMillisecound = 0;

        static MotionTimer()
        {
            stopWatch=new Stopwatch();
            stopWatch.Start();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="updateGrobal">From the hand is called during update</param>
        public MotionTimer(RenderContext context)
        {
            this._context = context;
        }

        public void TickUpdater()
        {
            if (this.lastMillisecound == 0)
            {
                this.lastMillisecound = stopWatch.ElapsedMilliseconds;
            }
            else
            {
                long currentMillisecound = stopWatch.ElapsedMilliseconds;
                //if (currentMillisecound - this.lastMillisecound > 1000/this.TimerPerSecond)
                if (currentMillisecound - this.lastMillisecound > 1000/this.MotionFramePerSecond)
                {
                    this.ElapesedTime = currentMillisecound - this.lastMillisecound;
                    this._context.UpdateWorlds();
                    this.lastMillisecound = stopWatch.ElapsedMilliseconds;
                }
            }
        }

    }
}