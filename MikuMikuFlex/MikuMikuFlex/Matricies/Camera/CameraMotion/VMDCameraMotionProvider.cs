using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MMDFileParser.MotionParser;
using MMF.Matricies.Projection;
using MMF.Utility;
using SlimDX;

namespace MMF.Matricies.Camera.CameraMotion
{
    /// <summary>
    /// VMDClass to manage the camera motion move file
    /// This class manage camera moved with vmd moton file.
    /// </summary>
    public class VMDCameraMotionProvider:ICameraMotionProvider
    {
        public static VMDCameraMotionProvider OpenFile(string path)
        {
            return new VMDCameraMotionProvider(MotionData.getMotion(File.OpenRead(path)));
        }

        private List<CameraFrameData> CameraFrames;

        public float CurrentFrame
        {
            get { return this.currentFrame; }
            set { this.currentFrame = value; }
        }

        private Stopwatch stopWatch;
        private long lastMillisecound;
        private float currentFrame=0;
        private bool isPlaying = false;
        private float finalFrame;
        private bool needReplay;

        /// <summary>
        /// Constructor
        /// Call the Start method after you create an instance when you play the motion
        /// Constractor
        /// If you want to play camera motion,you should call Start method after making instance.
        /// </summary>
        /// <param name="cameraMotion">VMDData file (VMD parsed data)</param>
        public VMDCameraMotionProvider(MotionData cameraMotion)
        {
            this.CameraFrames = cameraMotion.CameraFrames.CameraFrames;
            this.CameraFrames.Sort(new CameraFrameData());
            this.stopWatch=new Stopwatch();
            if (this.CameraFrames.Count == 0) this.finalFrame = 0;
            else
                this.finalFrame = this.CameraFrames.Last().FrameNumber;
        }

        public float FinalFrame
        {
            get { return this.finalFrame; }
        }

        /// <summary>
        /// To play motion
        /// Start to play motion
        /// </summary>
        public void Start(float startFrame=0,bool needReplay=false)
        {
            this.stopWatch.Start();
            this.currentFrame = startFrame;
            this.isPlaying = true;
            this.needReplay = needReplay;
        }

        /// <summary>
        /// Stop motion
        /// Stop to play motion
        /// </summary>
        public void Stop()
        {
            this.stopWatch.Stop();
            this.isPlaying = false;
        }

        

        private void Leap(CameraProvider cp,IProjectionMatrixProvider projection,float frame)
        {
            if(this.CameraFrames.Count==0)return;
            for (int j = 0; j < this.CameraFrames.Count - 1; j++)
            {
                if (this.CameraFrames[j].FrameNumber < frame && this.CameraFrames[j + 1].FrameNumber >= frame)
                {
                    //When you have pinched frame
                    
                    uint frameMargin = this.CameraFrames[j + 1].FrameNumber - this.CameraFrames[j].FrameNumber;
                    float progress = (frame - this.CameraFrames[j].FrameNumber)/(float) frameMargin;
                    LeapFrame(this.CameraFrames[j], this.CameraFrames[j + 1], cp, projection,progress);
                    return;
                }
            }
            //did not return when (or after the last frame)
            LeapFrame(this.CameraFrames.Last(), this.CameraFrames.Last(),cp,projection,0);
        }

        private void LeapFrame(CameraFrameData cf1,CameraFrameData cf2,CameraProvider cp,IProjectionMatrixProvider proj,float f)
        {
            float ProgX, ProgY, ProgZ, ProgR, ProgL, ProgP; ;
            ProgX = cf1.Curves[0].Evaluate(f);
            ProgY = cf1.Curves[1].Evaluate(f);
            ProgZ = cf1.Curves[2].Evaluate(f);
            ProgR = cf1.Curves[3].Evaluate(f);
            ProgL = cf1.Curves[4].Evaluate(f);
            ProgP = cf1.Curves[5].Evaluate(f);
            cp.CameraLookAt = CGHelper.ComplementTranslate(cf1, cf2,
                new Vector3(ProgX, ProgY, ProgZ));
            Quaternion rotation = CGHelper.ComplementRotateQuaternion(cf1,cf2,
                ProgR);
            float length = CGHelper.Lerp(cf1.Distance,cf2.Distance, ProgL);
            float angle = CGHelper.Lerp(cf1.ViewAngle,cf2.ViewAngle, ProgP);
            Vector3 Position2target = Vector3.TransformCoordinate(new Vector3(0, 0, 1),
                Matrix.RotationQuaternion(rotation));
            Vector3 TargetPosition = cp.CameraLookAt + length * Position2target;
            cp.CameraPosition = TargetPosition;
            proj.Fovy = CGHelper.ToRadians(angle);
        }

        public void UpdateCamera(CameraProvider cp, IProjectionMatrixProvider proj)
        {
            if (this.lastMillisecound == 0)
            {
                this.lastMillisecound = this.stopWatch.ElapsedMilliseconds;
            }
            else
            {
                long currentMillisecound = this.stopWatch.ElapsedMilliseconds;
                long elapsed = currentMillisecound - this.lastMillisecound;//Difference between the frame and the previous
                if (this.isPlaying) this.currentFrame += elapsed/30f;
                if (this.needReplay && this.finalFrame < this.currentFrame) this.currentFrame = 0;
                this.lastMillisecound = currentMillisecound;
            }
            Leap(cp, proj, this.currentFrame);
        }
    }
}
