using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;

namespace MMF.Model.Controller.ControllerComponent
{
    class DragControlManager
    {
        private bool lastState;

        private bool lastMouseState;

        private bool isDragging;

        private Point lastPoint;

        private ILockableController locker;

        public DragControlManager(ILockableController locker)
        {
            this.locker = locker;
        }

        public bool IsDragging
        {
            get
            {
                return this.isDragging;
            }
        }

        public bool checkNeedHighlight(bool result)
        {
            return this.isDragging || (result && !this.locker.IsLocked);
        }

        public Vector2 Delta { get; private set; }

        public void checkBegin(bool result,bool mouseState,Point mousePosition)
        {
            if (result && this.lastState && !this.lastMouseState && mouseState && !this.isDragging && !this.locker.IsLocked)
            {
                this.locker.IsLocked = true;
                this.isDragging = true;
            }
            this.Delta = new Vector2(mousePosition.X - this.lastPoint.X, mousePosition.Y - this.lastPoint.Y);
        }

        public void checkEnd(bool result,bool mouseState,Point mousePosition)
        {
            if (!mouseState && this.isDragging)
            {
                this.locker.IsLocked = false;
                this.isDragging = false;
            }

            this.lastState = result;
            this.lastMouseState = mouseState;
            this.lastPoint = mousePosition;
        }


    }
}
