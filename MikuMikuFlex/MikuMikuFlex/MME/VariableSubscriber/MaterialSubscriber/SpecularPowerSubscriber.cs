using SlimDX.Direct3D11;

namespace MMF.MME.VariableSubscriber.MaterialSubscriber
{
    internal sealed class SpecularPowerSubscriber : MaterialSubscriberBase
    {
        public override string Semantics
        {
            get { return "SPECULARPOWER"; }
        }

        public override void Subscribe(EffectVariable subscribeTo, SubscribeArgument variable)
        {
            SetAsFloat(variable.Material.SpecularPower, subscribeTo); //SPECULARPOWER p:System.Windows.controls.canvas.left Light annotation
        }

        protected override SubscriberBase GetSubscriberInstance(TargetObject target, bool isVector3)
        {
            return new SpecularPowerSubscriber();
        }
    }
}