using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX.Direct3D11;

namespace MMF.DeviceManager
{
    public class BlendStateManager : IDisposable

    {
        private readonly RenderContext _context;
        private Dictionary<BlendStates, BlendState> blendingStates = new Dictionary<BlendStates, BlendState>();

        public enum BlendStates
        {
            Disable,
            Alignment,
            Add,
            ReverseSubtruct,
            Subtruct,
            Multiply
        }

        public BlendStateManager(RenderContext context)
        {
            this._context = context;
            context.Disposables.Add(this);
            generateBlendingStates();
        }

        protected virtual void generateBlendingStates()
        {
            BlendStateDescription defaultDesc = new BlendStateDescription()
            {
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = true
            };
            defaultDesc.RenderTargets[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                BlendOperation = BlendOperation.Add,
                BlendOperationAlpha = BlendOperation.Add,
                DestinationBlend = BlendOption.Zero,
                DestinationBlendAlpha = BlendOption.Zero,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.One,
                SourceBlendAlpha = BlendOption.One
            };
            BlendState disableState = BlendState.FromDescription(this._context.DeviceManager.Device, defaultDesc);
            this.blendingStates.Add(BlendStates.Disable, disableState);
            defaultDesc.RenderTargets[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                BlendOperation = BlendOperation.Add,
                BlendOperationAlpha = BlendOperation.Add,
                DestinationBlend = BlendOption.InverseDestinationAlpha,
                DestinationBlendAlpha = BlendOption.InverseDestinationAlpha,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                SourceBlendAlpha = BlendOption.SourceAlpha
            };
            BlendState alignment = BlendState.FromDescription(this._context.DeviceManager.Device, defaultDesc);
            this.blendingStates.Add(BlendStates.Alignment, alignment);
            defaultDesc.RenderTargets[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                BlendOperation = BlendOperation.Add,
                BlendOperationAlpha = BlendOperation.Add,
                DestinationBlend = BlendOption.One,
                DestinationBlendAlpha = BlendOption.One,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                SourceBlendAlpha = BlendOption.SourceAlpha
            };
            BlendState add = BlendState.FromDescription(this._context.DeviceManager.Device, defaultDesc);
            this.blendingStates.Add(BlendStates.Add, add);
            defaultDesc.RenderTargets[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                BlendOperation = BlendOperation.ReverseSubtract,
                BlendOperationAlpha = BlendOperation.ReverseSubtract,
                DestinationBlend = BlendOption.One,
                DestinationBlendAlpha = BlendOption.One,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                SourceBlendAlpha = BlendOption.SourceAlpha
            };
            BlendState rsubtract = BlendState.FromDescription(this._context.DeviceManager.Device, defaultDesc);
            this.blendingStates.Add(BlendStates.ReverseSubtruct, rsubtract);
            defaultDesc.RenderTargets[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                BlendOperation = BlendOperation.Subtract,
                BlendOperationAlpha = BlendOperation.Subtract,
                DestinationBlend = BlendOption.One,
                DestinationBlendAlpha = BlendOption.One,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                SourceBlendAlpha = BlendOption.SourceAlpha
            };
            BlendState subtruct = BlendState.FromDescription(this._context.DeviceManager.Device, defaultDesc);
            this.blendingStates.Add(BlendStates.Subtruct, subtruct);
            defaultDesc.RenderTargets[0] = new RenderTargetBlendDescription()
            {
                BlendEnable = true,
                BlendOperation = BlendOperation.Add,
                BlendOperationAlpha = BlendOperation.Add,
                DestinationBlend = BlendOption.SourceColor,
                DestinationBlendAlpha = BlendOption.SourceAlpha,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.Zero,
                SourceBlendAlpha = BlendOption.Zero
            };
            BlendState multiply = BlendState.FromDescription(this._context.DeviceManager.Device, defaultDesc);
            this.blendingStates.Add(BlendStates.Multiply, multiply);
        }

        public void SetBlendState(BlendStates state)
        {
            this._context.DeviceManager.Context.OutputMerger.BlendState = this.blendingStates[state];
        }

        public void Dispose()
        {
            foreach (var blendingState in this.blendingStates)
            {
                if (blendingState.Value != null && !blendingState.Value.Disposed) blendingState.Value.Dispose();
            }
        }
    }
}
