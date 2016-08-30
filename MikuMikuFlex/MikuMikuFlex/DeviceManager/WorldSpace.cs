using System;
using System.Collections.Generic;
using System.Linq;
using MMF.Model;
using MMF.Motion;
using MMF.Sprite;

namespace MMF.DeviceManager
{
    public class WorldSpace:IDisposable
    {
        private readonly RenderContext _context;

        public WorldSpace(RenderContext context)
        {
            this._context = context;
            this.drawableGroups.Add(new DrawableGroup(0, "Default", this._context));
        }

        public void AddDrawableGroup(DrawableGroup group)
        {
            this.drawableGroups.Add(group);
            this.drawableGroups.Sort(new DrawableGroup.DrawableGroupComparer());
        }

        public void RemoveDrawableGroup(string key)
        {
            DrawableGroup removeTarget=null;
            foreach (var drawableGroup in this.drawableGroups)
            {
                if (drawableGroup.GroupName.Equals(key))
                {
                    removeTarget = drawableGroup;
                    break;
                }
            }
            if (removeTarget == null) return;
            this.drawableGroups.Remove(removeTarget);
        }

        private List<DrawableGroup> drawableGroups=new List<DrawableGroup>();

        public IReadOnlyList<DrawableGroup> DrawableGroups
        {
            get { return this.drawableGroups; }
        }

        private List<IMovable> moveResources=new List<IMovable>();
        private List<IDynamicTexture> dynamicTextures=new List<IDynamicTexture>();
        private List<IGroundShadowDrawable> groundShadowDrawables=new List<IGroundShadowDrawable>();
        private List<IEdgeDrawable> edgeDrawables=new List<IEdgeDrawable>();
        private bool isDisposed;

        public List<IMovable> MoveResources
        {
            get { return this.moveResources; }
            private set { this.moveResources = value; }
        }

        public List<IDynamicTexture> DynamicTextures
        {
            get { return this.dynamicTextures; }
            private set { this.dynamicTextures = value; }
        }

        public List<IGroundShadowDrawable> GroundShadowDrawables
        {
            get { return this.groundShadowDrawables; }
            private set { this.groundShadowDrawables = value; }
        }

        public List<IEdgeDrawable> EdgeDrawables
        {
            get { return this.edgeDrawables; }
            private set { this.edgeDrawables = value; }
        }

        /// <summary>
        ///     To add a resource
        /// </summary>
        /// <param name="drawable"></param>
        public void AddResource(IDrawable drawable,String groupName="Default")
        {
            this.drawableGroups.First(group=>group.GroupName.Equals(groupName)).AddDrawable(drawable);
            if (drawable is IMovable)
            {
                this.moveResources.Add((IMovable)drawable);
            }
            if (drawable is IEdgeDrawable)
            {
                this.edgeDrawables.Add((IEdgeDrawable)drawable);
            }
            if (drawable is IGroundShadowDrawable)
            {
                this.groundShadowDrawables.Add((IGroundShadowDrawable)drawable);
            }
        }

        /// <summary>
        ///     To remove a resource from
        /// </summary>
        /// <param name="drawable"></param>
        public void RemoveResource(IDrawable drawable)
        {
            foreach (var drawableGroup in this.drawableGroups)
            {
                if (drawableGroup.DeleteDrawable(drawable))
                {
                    if (drawable is IMovable)
                    {
                        this.moveResources.Remove((IMovable)drawable);
                    }
                }
            }
        }

        /// <summary>
        /// Draw something that needs all the drawing is registered
        /// </summary>
        public void DrawAllResources(TexturedBufferHitChecker hitChecker)
        {
            //foreach (var edgeDrawable in this.edgeDrawables)
            //{
            //    if (edgeDrawable.Visibility) edgeDrawable.DrawEdge();
            //}
            foreach (var drawableGroup in this.drawableGroups)
            {
                drawableGroup.DrawAll();
                this._context.BlendingManager.SetBlendState(BlendStateManager.BlendStates.Alignment);
            }
            //foreach (var groundShadowDrawable in this.groundShadowDrawables)
            //{
            //    if (groundShadowDrawable.Visibility) groundShadowDrawable.DrawGroundShadow();
            //}
            hitChecker.CheckTarget();
        }

        public IDrawable getDrawableByFileName(string fileName)
        {
            foreach (var drawableGroup in this.drawableGroups)
            {
                var drawable = drawableGroup.getDrawableByFileName(fileName);
                if (drawable != null) return drawable;
            }
            return null;
        }

        /// <summary>
        /// Add dynamic texture update
        /// </summary>
        /// <param name="dtexture">Dynamic textures need to be updated</param>
        public void AddDynamicTexture(IDynamicTexture dtexture)
        {
            this.dynamicTextures.Add(dtexture);
        }

        /// <summary>
        /// The dynamic texture update exclude
        /// </summary>
        /// <param name="dtexture">No more dynamic textures</param>
        public void RemoveDynamicTexture(IDynamicTexture dtexture)
        {
            if (this.dynamicTextures.Contains(dtexture)) this.dynamicTextures.Remove(dtexture);
        }

        public void Dispose()
        {
            foreach (var drawableGroup in this.drawableGroups)
            {
                drawableGroup.Dispose();
            }
            foreach (var dynamicTexture in this.DynamicTextures)
            {
                if(this.dynamicTextures!=null)dynamicTexture.Dispose();
            }
            this.isDisposed = true;
        }

        public bool IsDisposed
        {
            get { return this.isDisposed; }
        }

        public void UpdateAllDynamicTexture()
        {
            foreach (var dynamicTexture in this.DynamicTextures)
            {
                dynamicTexture.UpdateTexture();
            }
        }
    }
}
