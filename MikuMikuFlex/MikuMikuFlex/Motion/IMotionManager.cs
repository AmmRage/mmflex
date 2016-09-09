using System;
using System.Collections.Generic;
using System.IO;
using MMDFileParser.PMXModelParser;
using MMF.Bone;
using MMF.Model;
using MMF.Morph;

namespace MMF.Motion
{
    /// <summary>
    ///     For model run
    /// </summary>
    public interface IMotionManager:ITransformUpdater
    {

        IMotionProvider CurrentMotionProvider { get; }

        /// <summary>
        /// Currently playing motion frame position (second unit) gets。
        /// </summary>
        float CurrentFrame { get; }

        /// <summary>
        /// Just how long it took from the last frame (second unit) gets
        /// </summary>
        float ElapsedTime { get; }

        /// <summary>
        /// Table of filenames and promotion provider
        /// </summary>
        List<KeyValuePair<string, IMotionProvider>> SubscribedMotionMap { get; }
        
        /// <summary>
        ///     The initialization process
        /// </summary>
        /// <param name="skinning"></param>
        void Initialize(ModelData model,IMorphManager morph, ISkinningProvider skinning, IBufferManager bufferManager);

        /// <summary>
        ///     Adds a motion from a file。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ignoreParent"></param>
        /// <returns>The index of the added motion</returns>
        IMotionProvider AddMotionFromFile(string filePath,bool ignoreParent);

        /// <summary>
        ///     Applying the motion, move the。
        /// </summary>
        /// <param name="index"></param>
        void ApplyMotion(IMotionProvider provider, int startFrame = 0, ActionAfterMotion setting = ActionAfterMotion.Nothing);

        void StopMotion(bool toIdentity=false);


        event EventHandler<ActionAfterMotion> MotionFinished;


        /// <summary>
        ///     Called when an updated list of motion。
        /// </summary>
        event EventHandler MotionListUpdated;

        IMotionProvider AddMotionFromStream(string fileName, Stream stream, bool ignoreParent);
    }
}