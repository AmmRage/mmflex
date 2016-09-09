using System.Collections.Generic;
using System.Timers;

namespace MMF.Utility
{
    /// <summary>
    ///     FPSCounter
    /// </summary>
    public class FPSCounter
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public FPSCounter()
        {
            this.frameHistory = new Queue<int>();
            this.AvarageSpan = 10;
            this.FpsTimer = new Timer(1000d);
            this.FpsTimer.Elapsed += Tick;
        }

        /// <summary>
        ///     FPSOf history
        /// </summary>
        private Queue<int> frameHistory { get; set; }

        /// <summary>
        ///     FPSCounter
        /// </summary>
        private int counter { get; set; }

        /// <summary>
        ///     FPSTimer to count
        /// </summary>
        public Timer FpsTimer { get; private set; }

        /// <summary>
        ///     FPSThe average number of seconds
        /// </summary>
        public int AvarageSpan { get; set; }

        private bool isCached;

        private float cachedFPS;

        /// <summary>
        ///     FPS
        /// </summary>
        public float FPS
        {
            get
            {
                if (!this.isCached)
                {
                    int sum = 0;
                    foreach (int i in this.frameHistory)
                    {
                        sum += i;
                    }
                    this.cachedFPS=sum/(float) this.frameHistory.Count;
                    this.isCached = true;
                    return this.cachedFPS;
                }
                return this.cachedFPS;
            }
        }

        /// <summary>
        ///     FPSThe start count
        /// </summary>
        public void Start()
        {
            this.counter = 0;
            this.FpsTimer.Start();
        }

        /// <summary>
        ///     Frame advance
        /// </summary>
        public void CountFrame()
        {
            this.counter++;
        }

        private void Tick(object sender, ElapsedEventArgs args)
        {
            if (this.frameHistory.Count > this.AvarageSpan) this.frameHistory.Dequeue();
            this.frameHistory.Enqueue(this.counter);
            this.counter = 0;
            this.isCached = false;
        }
    }
}