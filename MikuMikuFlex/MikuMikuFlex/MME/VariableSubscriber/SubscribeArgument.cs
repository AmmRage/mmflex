using MMF.MME.VariableSubscriber.MaterialSubscriber;
using MMF.Model;

namespace MMF.MME.VariableSubscriber
{
    /// <summary>
    ///     Effects register arguments
    /// </summary>
    public class SubscribeArgument
    {
        public SubscribeArgument(IDrawable model, RenderContext context)
        {
            this.Model = model;
            this.Context = context;
        }

        public SubscribeArgument(MaterialInfo info, IDrawable model, RenderContext context)
        {
            this.Material = info;
            this.Context = context;
            this.Model = model;
        }

        public IDrawable Model { get; private set; }

        public RenderContext Context { get; private set; }

        public MaterialInfo Material { get; private set; }
    }
}