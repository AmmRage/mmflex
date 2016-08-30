using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.Model;

namespace MMF.DeviceManager
{
    public class DrawableGroup:IDisposable
    {
        public class DrawableGroupComparer : IComparer<DrawableGroup>
        {
            public int Compare(DrawableGroup x, DrawableGroup y)
            {
                return x.priorty - y.priorty;
            }
        }

        private List<IDrawable> drawables=new List<IDrawable>();

        protected int priorty;

        private string groupName;
        protected readonly RenderContext _context;

        public DrawableGroup(int priorty, string groupName,RenderContext context)
        {
            this.priorty = priorty;
            this.groupName = groupName;
            this._context = context;
        }

        public string GroupName
        {
            get { return this.groupName; }
        }

        public void AddDrawable(IDrawable drawable)
        {
            this.drawables.Add(drawable);
        }

        public bool DeleteDrawable(IDrawable drawable)
        {
            if (this.drawables.Contains(drawable))
            {
                this.drawables.Remove(drawable);
                return true;
            }
            return false;
        }

        public void ForEach(Action<IDrawable> act)
        {
            foreach (var drawable in this.drawables)
            {
                act(drawable);
            }
        }

        public void DrawAll()
        {
            PreDraw();
            foreach (var drawable in this.drawables)
            {
                if(drawable.Visibility)drawable.Draw();
            }
            PostDraw();
        }

        protected virtual void PostDraw()
        {
            
        }

        protected virtual void PreDraw()
        {
            
        }

        public IDrawable getDrawableByFileName(string fileName)
        {
            return this.drawables.FirstOrDefault(drawable => drawable.FileName.Equals(fileName));
        }

        public int CompareTo(DrawableGroup other)
        {
            return this.priorty - other.priorty;
        }

        public void Dispose()
        {
            foreach (var drawable in this.drawables)
            {
                drawable.Dispose();
            }
        }
    }
}
