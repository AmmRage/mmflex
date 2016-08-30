using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MMDFileParser.MotionParser;
using MMF.MME;
using MMF.Model;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace MMF.Utility
{
    public static class CGHelper
    {
        public static readonly Vector3 EularMaximum = new Vector3((float)Math.PI - float.Epsilon, 0.5f * (float)Math.PI - float.Epsilon, (float)Math.PI - float.Epsilon);

        public static readonly Vector3 EularMinimum = -EularMaximum;

        public static bool Between(this float value, float min, float max)
        {
            return (min <= value) && (max >= value);
        }

        public static Vector3 NormalizeEular(this Vector3 source)
        {
            if (!source.X.Between((float) -Math.PI, (float) Math.PI))
            {
                if (source.X > 0)
                {
                    source.X -= (float)Math.PI*2;
                }
                else
                {
                    source.X += (float)Math.PI * 2;
                }
            }
            if (!source.Y.Between((float)-Math.PI*0.5f, (float)Math.PI*0.5f))
            {
                if (source.Y > 0)
                {
                    source.Y -= (float)Math.PI * 2;
                }
                else
                {
                    source.Y += (float)Math.PI * 2;
                }
            }
            if (!source.Z.Between((float)-Math.PI, (float)Math.PI))
            {
                if (source.Z > 0)
                {
                    source.Z -= (float)Math.PI * 2;
                }
                else
                {
                    source.Z += (float)Math.PI * 2;
                }
            }
            return source;
        }

        /// <summary>
        ///     Buffers are created from a sequence of bytes。
        /// </summary>
        /// <param name="dataList">List containing the data in the buffer</param>
        /// <param name="device">Device</param>
        /// <param name="flag">Buffer usage</param>
        /// <returns></returns>
        public static Buffer CreateBuffer(IEnumerable<byte> dataList, Device device, BindFlags flag)
        {
            using (DataStream ds = new DataStream(dataList.ToArray(), true, true))
            {
                BufferDescription bufDesc = new BufferDescription
                {
                    BindFlags = flag,
                    SizeInBytes = (int) ds.Length
                };
                return new Buffer(device, ds, bufDesc);
            }
        }

        public static Buffer CreateBuffer<T>(IEnumerable<T> dataList, Device device, BindFlags flag)
        {
            using (DataStream ds = new DataStream(dataList.ToArray(), true, true))
            {
                BufferDescription bufDesc = new BufferDescription
                {
                    BindFlags = flag,
                    SizeInBytes = (int) ds.Length
                };
                return new Buffer(device, ds, bufDesc);
            }
        }

        public static Buffer CreateBuffer(Device device,int size, BindFlags flag)
        {
                BufferDescription bufDesc = new BufferDescription
                {
                    BindFlags = flag,
                    SizeInBytes = size
                };
                return new Buffer(device, bufDesc);
        }

        public static Effect CreateEffectFx5(string filePath, Device device)
        {
            using(FileStream fs=File.OpenRead(filePath))
            using (ShaderBytecode byteCode = EffectLoader.Instance.GetShaderBytecode(filePath,fs))
            {
                return new Effect(device, byteCode);
            }
        }

        internal static Effect CreateEffectFx5FromResource(string resourcePath, Device device)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] ss = asm.GetManifestResourceNames();
            using (Stream fs = asm.GetManifestResourceStream(resourcePath))
            using (ShaderBytecode byteCode = EffectLoader.Instance.GetShaderBytecode(resourcePath, fs))
            {
                return new Effect(device, byteCode);
            }
        }

        /// <summary>
        ///     Generate parallel motion vectors from matrices
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static Vector3 GetTranslation(Matrix matrix)
        {
            return new Vector3(matrix.M41, matrix.M42, matrix.M43);
        }

        public static Quaternion getRotationQuaternion(Vector3 rotation)
        {
            return Quaternion.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }

        /// <summary>
        /// Yaw (Y rotation) to Quaternion, Pitch(X回転), Roll(Z回転)に分解する関数
        /// </summary>
        /// <param name="input">Quaternion to decompose</param>
        /// <param name="ZRot">ZRotation.</param>
        /// <param name="XRot">XRotation (-PI/2～PI/2)</param>
        /// <param name="YRot">YRotation.</param>
        /// <returns>Gimbal lock occurs when false.。ジンバルロックはX軸回転で発生</returns>
        public static bool FactoringQuaternionZXY(Quaternion input, out float ZRot, out float XRot, out float YRot)
        {
            //Normalizing a Quaternion
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //To generate the matrix
            Matrix rot;
            Matrix.RotationQuaternion(ref inputQ, out rot);
            //Get yaw (rotation around the x-axis)
            if (rot.M32 > 1 - 1.0e-4 || rot.M32 < -1 + 1.0e-4)
            {//Gimbal lock detection
                XRot = (float) (rot.M32 < 0 ? Math.PI / 2 : -Math.PI / 2);
                ZRot = 0; YRot = (float)Math.Atan2(-rot.M13, rot.M11);
                return false;
            }
            XRot = -(float)Math.Asin(rot.M32);
            //Gets the role
            ZRot = (float)Math.Asin(rot.M12 / Math.Cos(XRot));
            if (float.IsNaN(ZRot))
            {//Leakage protection
                XRot = (float) (rot.M32 < 0 ? Math.PI / 2 : -Math.PI / 2);
                ZRot = 0; YRot = (float)Math.Atan2(-rot.M13, rot.M11);
                return false;
            }
            if (rot.M22 < 0)
                ZRot = (float) (Math.PI - ZRot);
            //Get the pitch
            YRot = (float)Math.Atan2(rot.M31, rot.M33);
            return true;
        }


        /// <summary>
        /// Describes a Quaternion's X,Y,Z回転に分解する関数
        /// </summary>
        /// <param name="input">Quaternion to decompose</param>
        /// <param name="XRot">XRotation.</param>
        /// <param name="YRot">YRotation (-PI/2～PI/2)</param>
        /// <param name="ZRot">ZRotation.</param>
        /// <returns></returns>
        public static bool FactoringQuaternionXYZ(Quaternion input, out float XRot, out float YRot, out float ZRot)
        {
            //Normalizing a Quaternion
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //To generate the matrix
            Matrix rot;
            Matrix.RotationQuaternion(ref inputQ, out rot);
            //Get the rotation around the Y axis
            if (rot.M13 > 1 - 1.0e-4 || rot.M13 < -1 + 1.0e-4)
            {//Gimbal lock detection
                XRot = 0;
                YRot = (float) (rot.M13 < 0 ? Math.PI / 2 : -Math.PI / 2);
                ZRot = -(float)Math.Atan2(-rot.M21, rot.M22);
                return false;
            }
            YRot = -(float)Math.Asin(rot.M13);
            //Get the rotation around the X axis
            XRot = (float)Math.Asin(rot.M23 / Math.Cos(YRot));
            if (float.IsNaN(XRot))
            {//Gimbal lock detection (leak protection)
                XRot = 0;
                YRot = (float) (rot.M13 < 0 ? Math.PI / 2 : -Math.PI / 2);
                ZRot = -(float)Math.Atan2(-rot.M21, rot.M22);
                return false;
            }
            if (rot.M33 < 0)
                XRot = (float) (Math.PI - XRot);
            //Get the rotation around the Z axis
            ZRot = (float)Math.Atan2(rot.M12, rot.M11);
            return true;
        }
        /// <summary>
        /// Describes a Quaternion's Y,Z,X回転に分解する関数
        /// </summary>
        /// <param name="input">Quaternion to decompose</param>
        /// <param name="YRot">YRotation.</param>
        /// <param name="ZRot">ZRotation (-PI/2～PI/2)</param>
        /// <param name="XRot">XRotation.</param>
        /// <returns></returns>
        public static bool FactoringQuaternionYZX(Quaternion input, out float YRot, out float ZRot, out float XRot)
        {
            //Normalizing a Quaternion
            Quaternion inputQ = new Quaternion(input.X, input.Y, input.Z, input.W);
            inputQ.Normalize();
            //To generate the matrix
            Matrix rot;
            Matrix.RotationQuaternion(ref inputQ, out rot);
            //Get the rotation around the Z axis
            if (rot.M21 > 1 - 1.0e-4 || rot.M21 < -1 + 1.0e-4)
            {//Gimbal lock detection
                YRot = 0;
                ZRot = (float) (rot.M21 < 0 ? Math.PI / 2 : -Math.PI / 2);
                XRot = -(float)Math.Atan2(-rot.M32, rot.M33);
                return false;
            }
            ZRot = -(float)Math.Asin(rot.M21);
            //Get the rotation around the Y axis
            YRot = (float)Math.Asin(rot.M31 / Math.Cos(ZRot));
            if (float.IsNaN(YRot))
            {//Gimbal lock detection (leak protection)
                YRot = 0;
                ZRot = (float) (rot.M21 < 0 ? Math.PI / 2 : -Math.PI / 2);
                XRot = -(float)Math.Atan2(-rot.M32, rot.M33);
                return false;
            }
            if (rot.M11 < 0)
                YRot = (float) (Math.PI - YRot);
            //Get the rotation around the X axis
            XRot = (float)Math.Atan2(rot.M23, rot.M22);
            return true;
        }

        public static Vector3 MulEachMember(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(vec1.X*vec2.X,vec1.Y*vec2.Y,vec1.Z*vec2.Z);
        }

        public static Vector4 MulEachMember(Vector4 vec1, Vector4 vec2)
        {
            return new Vector4(vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z,vec1.W*vec2.W);
        }

        public static Vector3 ToRadians(Vector3 degree)
        {
            return new Vector3(ToRadians(degree.X), ToRadians(degree.Y), ToRadians(degree.Z));
        }

        public static float ToRadians(float degree)
        {
            return degree*(float) Math.PI/180f;
        }

        public static float ToDegree(float radians)
        {
            return 180f*radians/(float) Math.PI;
        }

        public static Vector3 ToDegree(Vector3 radian)
        {
            return new Vector3(ToDegree(radian.X), ToDegree(radian.Y), ToDegree(radian.Z));
        }

        /// <summary>
        ///     Values in the range of maximum and minimum values fit
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>Contains the value</returns>
        public static float Clamp(float value, float min, float max)
        {
            if (min > value)
                return min;
            if (max < value)
                return max;
            return value;
        }


        #region Byte column converts to assign system

        public static void LoadIndex(int surface, List<byte> InputIndexBuffer)
        {
            UInt16 vertex = (UInt16) surface;
            byte[] v = BitConverter.GetBytes(vertex);
            for (int i = 0; i < v.Length; i++)
            {
                InputIndexBuffer.Add(v[i]);
            }
        }

        public static void AddListBuffer(float value, List<byte> InputBuffer)
        {
            byte[] v = BitConverter.GetBytes(value);
            for (int i = 0; i < v.Length; i++)
            {
                InputBuffer.Add(v[i]);
            }
        }

        public static void AddListBuffer(int value, List<byte> InputBuffer)
        {
            AddListBuffer((UInt16) value, InputBuffer);
        }

        public static void AddListBuffer(UInt16 value, List<byte> InputBuffer)
        {
            byte[] v = BitConverter.GetBytes(value);
            for (int i = 0; i < v.Length; i++)
            {
                InputBuffer.Add(v[i]);
            }
        }

        public static void AddListBuffer(Vector2 value, List<byte> InputBuffer)
        {
            AddListBuffer(value.X, InputBuffer);
            AddListBuffer(value.Y, InputBuffer);
        }

        public static void AddListBuffer(Vector3 value, List<byte> InputBuffer)
        {
            AddListBuffer(value.X, InputBuffer);
            AddListBuffer(value.Y, InputBuffer);
            AddListBuffer(value.Z, InputBuffer);
        }

        public static void AddListBuffer(Vector4 value, List<byte> InputBuffer)
        {
            AddListBuffer(value.X, InputBuffer);
            AddListBuffer(value.Y, InputBuffer);
            AddListBuffer(value.Z, InputBuffer);
            AddListBuffer(value.W, InputBuffer);
        }

        #endregion

        #region Constant systems

        public const float PI = (float) Math.PI;

        public const string ToonDir = @"Resource\Toon\";

        #endregion

        #region Complement system

        /// <summary>
        ///     Computes a rotation matrix in the specified frame gave two frames from。
        /// </summary>
        /// <param name="frame1">A lower keyframe number</param>
        /// <param name="frame2"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static Quaternion ComplementRotateQuaternion(BoneFrameData frame1, BoneFrameData frame2, float progress)
        {
            return Quaternion.Slerp(frame1.BoneRotatingQuaternion, frame2.BoneRotatingQuaternion, progress);
        }

        public static Quaternion ComplementRotateQuaternion(CameraFrameData frame1,CameraFrameData frame2, float progress)
        {
            Quaternion q1 =
                Quaternion.RotationYawPitchRoll(frame1.CameraRotation.Y,frame1.CameraRotation.X,frame1.CameraRotation.Z);
            Quaternion q2 =
                Quaternion.RotationYawPitchRoll(frame2.CameraRotation.Y, frame2.CameraRotation.X, frame2.CameraRotation.Z);
            return Quaternion.Slerp(q1, q2, progress);
        }

        /// <summary>
        ///     Calculates the complementary locations in the specified frame from the frame gave the two。
        /// </summary>
        /// <param name="frame1"></param>
        /// <param name="frame2"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static Vector3 ComplementTranslate(BoneFrameData frame1, BoneFrameData frame2, Vector3 progress)
        {
            return new Vector3(Lerp(frame1.BonePosition.X, frame2.BonePosition.X, progress.X),
                Lerp(frame1.BonePosition.Y, frame2.BonePosition.Y, progress.Y),
                Lerp(frame1.BonePosition.Z, frame2.BonePosition.Z, progress.Z));
        }

        public static Vector3 ComplementTranslate(CameraFrameData frame1, CameraFrameData frame2, Vector3 progress)
        {
            return new Vector3(Lerp(frame1.CameraPosition.X, frame2.CameraPosition.X, progress.X),
                Lerp(frame1.CameraPosition.Y, frame2.CameraPosition.Y, progress.Y),
                Lerp(frame1.CameraPosition.Z, frame2.CameraPosition.Z, progress.Z));
        }

        public static float Lerp(float start, float end, float factor)
        {
            return start + ((end - start)*factor);
        }

        #endregion
    }
}