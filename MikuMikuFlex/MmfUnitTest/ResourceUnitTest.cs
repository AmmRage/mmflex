using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MMF;
using MMF.Controls.Forms;
using MMF.DeviceManager;
using MMF.Model.PMX;

using SlimDX;

namespace MmfUnitTest
{
    [TestClass]
    public class ResourceUnitTest
    {
        [TestMethod]
        public void TestLoadmodel()
        {
            var form = new RenderForm();
            RenderContext Context = new RenderContext();
            ScreenContext _scContext = Context.Initialize(form);
            PMXModel Model = PMXModelWithPhysics.OpenLoad(@"C:\Users\ZhiYong\Documents\CodeBase\mmflex\debug\1.pmx", Context);
            Model.Transformer.Position = new Vector3(0, 0, 0);

            _scContext.WorldSpace.AddResource(Model);

            _scContext.Dispose();
            Model.Dispose();
            Context.Dispose();
        }
    }
}
