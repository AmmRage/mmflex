using System;
using SlimDX;
using SlimDX.Direct3D11;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace MMF.MME.VariableSubscriber.ConstantSubscriber.ConstantBufferManager
{
    public abstract class ConstantBufferManagerBase<T> : IDisposable where T : struct
    {
        protected DataBox BufferDataBox;

        public Buffer ConstantBuffer;
        protected Device device;

        protected EffectConstantBuffer target;

        public void Dispose()
        {
            this.ConstantBuffer.Dispose();
        }

        public void Initialize(Device device, EffectConstantBuffer effectVariable, int size, T obj)
        {
            this.device = device;
            this.target = effectVariable;
            this.BufferDataBox = new DataBox(0, 0, new DataStream(new[] {obj}, true, true));
            this.ConstantBuffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = size,
                BindFlags = BindFlags.ConstantBuffer
            });
            OnInitialize();
        }

        protected abstract void OnInitialize();

        public abstract void ApplyToEffect(T obj);

        protected void WriteToBuffer(T obj)
        {
            this.BufferDataBox.Data.WriteRange(new[] {obj});
            this.BufferDataBox.Data.Position = 0;
            this.device.ImmediateContext.UpdateSubresource(this.BufferDataBox, this.ConstantBuffer, 0);
        }

        /// <summary>
        ///     Sets the buffer to the constant buffer with the specified name
        /// </summary>
        /// <param name="cbufferName"></param>
        /// <param name="buffer"></param>
        protected void SetConstantBuffer()
        {
            this.target.ConstantBuffer = this.ConstantBuffer;
        }
    }
}