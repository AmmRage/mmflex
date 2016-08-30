using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace MMF.Model.Shape.Overlay
{
    public class OverlaySilinderShape:SilinderShape
    {
        private readonly Vector4 baseColor;
        private readonly Vector4 _overlayColor;

        public OverlaySilinderShape(RenderContext context, Vector4 color,Vector4 overlayColor, SilinderShapeDescription desc) : base(context, color, desc)
        {
            this.baseColor = color;
            this._overlayColor = overlayColor;
        }

        public override void HitTestResult(bool result, bool mouseState, Point mousePosition)
        {
            base.HitTestResult(result, mouseState, mousePosition);
            this._color = result ? this._overlayColor : this.baseColor;
        }
    }
}
