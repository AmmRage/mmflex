using System.Runtime.InteropServices;
using SlimDX;

namespace MMF.MME.VariableSubscriber.ConstantSubscriber.ConstantBufferManager
{
    public struct BasicMaterialConstantBufferInputLayout
    {
        #region Order replacement risks
        public Vector4 AmbientLight;

        public Vector4 DiffuseLight;

        public Vector4 SpecularLight;

        public float SpecularPower;
        #endregion
        public static int SizeInBytes
        {
            get
            {
                int size = Marshal.SizeOf(typeof (BasicMaterialConstantBufferInputLayout));
                size = size%16 == 0 ? size : size + 16 - size%16; //Not multiples of 16 and seems to be useless in a multiple of 16
                return size;
            }
        }
    }
}