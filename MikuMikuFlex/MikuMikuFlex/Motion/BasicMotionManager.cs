using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MMDFileParser.PMXModelParser;
using MMF.Bone;
using MMF.Model;
using MMF.Morph;

namespace MMF.Motion
{
    /// <summary>
    ///     Motion control class
    /// </summary>
    public class BasicMotionManager : IMotionManager
    {
        private readonly RenderContext context;

        private Stopwatch motionTimer;

        /// <summary>
        ///     Use skinned interface
        /// </summary>
        private ISkinningProvider skinningProvider;

        private IMorphManager morphManager;

        public BasicMotionManager(RenderContext context)
        {
            this.context = context;
        }

        private long lastTime { get; set; }

        /// <summary>
        /// Table of filenames and promotion provider
        /// </summary>
        public List<KeyValuePair<string, IMotionProvider>> SubscribedMotionMap { get; private set; }

        /// <summary>
        ///     Now playing motion index
        /// </summary>
        public IMotionProvider CurrentMotionProvider { get; set; }

        /// <summary>
        ///     Currently playing motion frame position (second unit) gets。
        /// </summary>
        public float CurrentFrame
        {
            get
            {
                if (this.CurrentMotionProvider == null) return float.NaN;
                return this.CurrentMotionProvider.CurrentFrame/this.context.Timer.MotionFramePerSecond;
            }
        }

        /// <summary>
        ///     From the last frame took much time (in milliseconds) or get the
        /// </summary>
        public float ElapsedTime { get; private set; }

        /// <summary>
        ///     Initialization
        /// </summary>
        /// <param name="skinning">Use skinned interface</param>
        public void Initialize(ModelData model,IMorphManager morph,ISkinningProvider skinning,IBufferManager bufferManager)
        {
            this.skinningProvider = skinning;
            this.motionTimer=new Stopwatch();
            this.motionTimer.Start();
            this.morphManager = morph;
            this.SubscribedMotionMap = new List<KeyValuePair<string, IMotionProvider>>();
        }

        /// <summary>
        /// ITransformUpdaterThe implementation of the Member
        /// </summary>
        public bool UpdateTransform()
        {
            if (this.lastTime == 0)
            {
                this.lastTime = this.motionTimer.ElapsedMilliseconds;
            }
            else
            {
                long currentTime = this.motionTimer.ElapsedMilliseconds;
                this.ElapsedTime = currentTime - this.lastTime;
                if (this.CurrentMotionProvider != null)
                    this.CurrentMotionProvider.Tick(this.context.Timer.MotionFramePerSecond, this.ElapsedTime / 1000f, this.morphManager);
                this.lastTime = currentTime;
            }
            return true;
        }


        /// <summary>
        ///     Motion to add from file
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="ignoreParent">Whether or not to ignore any parent</param>
        /// <returns>Motion index</returns>
        public IMotionProvider AddMotionFromFile(string filePath,bool ignoreParent)
        {
            // To create a motion provider. Assign the appropriate class in accordance with the extension of the file
            IMotionProvider motion;
            var extension = Path.GetExtension(filePath);
            if (String.Compare(extension, ".vmd", true) == 0) motion = new MMDMotion(filePath, ignoreParent);
            else if (String.Compare(extension, ".vme", true) == 0) motion = new MMDMotionForVME(filePath, ignoreParent);
            else throw new Exception("File is incorrect!");

            motion.AttachMotion(this.skinningProvider.Bone);
            motion.MotionFinished += motion_MotionFinished;
            this.SubscribedMotionMap.Add(new KeyValuePair<string, IMotionProvider>(filePath, motion));
            if (MotionListUpdated != null) MotionListUpdated(this, new EventArgs());
            return motion;
        }

        /// <summary>
        ///     To play the specified motion
        /// </summary>
        /// <param name="id">Id of the motion</param>
        /// <param name="startFrame">In the first frame</param>
        /// <param name="setting">After the end of</param>
        public void ApplyMotion(IMotionProvider motionProvider, int startFrame = 0,
            ActionAfterMotion setting = ActionAfterMotion.Nothing)
        {
            if (this.CurrentMotionProvider != null) this.CurrentMotionProvider.Stop();
            motionProvider.Start(startFrame, setting);
            this.CurrentMotionProvider = motionProvider;
        }

        /// <summary>
        ///     To exit motion
        /// </summary>
        public void StopMotion(bool toIdentity=false)
        {
            if(this.CurrentMotionProvider!=null) this.CurrentMotionProvider.Stop();
            if (toIdentity)
            {
                this.CurrentMotionProvider = null;
            }
        }

        /// <summary>
        ///     To indicate that the motion was
        /// </summary>
        public event EventHandler<ActionAfterMotion> MotionFinished;

        /// <summary>
        ///     To tell you the updated list of motion
        /// </summary>
        public event EventHandler MotionListUpdated;

        public IMotionProvider AddMotionFromStream(string fileName, Stream stream,bool ignoreParent)
        {
            IMotionProvider motion = new MMDMotion(stream, ignoreParent);
            motion.MotionFinished += motion_MotionFinished;
            this.SubscribedMotionMap.Add(new KeyValuePair<string, IMotionProvider>(fileName, motion));
            if (MotionListUpdated != null)MotionListUpdated(this, new EventArgs());
            return motion;
        }

        private void motion_MotionFinished(object owner, ActionAfterMotion obj)
        {
            if (MotionFinished != null) MotionFinished(this, obj);
        }
    }
}