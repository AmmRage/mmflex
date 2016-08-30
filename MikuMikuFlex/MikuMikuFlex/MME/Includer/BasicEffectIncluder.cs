using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SlimDX.D3DCompiler;

namespace MMF.MME.Includer
{
    /// <summary>
    /// Basic#includeのパス解決クラス
    /// </summary>
    public class BasicEffectIncluder
        : Include,IComparer<IncludeDirectory>

    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BasicEffectIncluder()
        {
            this.IncludeDirectories = new ObservableCollection<IncludeDirectory>();
            this.IncludeDirectories.CollectionChanged += IncludeDirectories_CollectionChanged;
            this.IncludeDirectories.Add(new IncludeDirectory("Shader\\include",0));
        }

        /// <summary>
        /// When the collection has changed back and sort
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void IncludeDirectories_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<IncludeDirectory> sorted = this.IncludeDirectories.ToList();
            this.IncludeDirectories=new ObservableCollection<IncludeDirectory>(sorted);
        }

        /// <summary>
        /// A list of directories that are registered and their priorities
        /// </summary>
        public ObservableCollection<IncludeDirectory> IncludeDirectories { get; private set; }

        #region Include メンバー

        public void Close(Stream stream)
        {
            stream.Close();
        }

        /// <summary>
        /// When you open a file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <param name="parentStream"></param>
        /// <param name="stream"></param>
        public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
        {
            if (Path.IsPathRooted(fileName))//If the absolute path directly attributable
            {
                stream = File.OpenRead(fileName);
                return;
            }
            foreach (IncludeDirectory directory in this.IncludeDirectories)
            {
                if (File.Exists(Path.Combine(directory.DirectoryPath, fileName)))
                {
                    stream = File.OpenRead(Path.Combine(directory.DirectoryPath, fileName));
                    return;
                }
            }
            stream = null;
        }

        #endregion


        #region IComparer<IncludeDirectory> メンバー

        /// <summary>
        /// Interface to use when comparing
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(IncludeDirectory x, IncludeDirectory y)
        {
            return x.Priorty - y.Priorty;
        }

        #endregion
    }
}