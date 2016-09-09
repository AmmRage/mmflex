using System.Diagnostics;
using System.IO;
using MMF.Utility;

namespace MMF.Model
{
    /// <summary>
    ///     Standard resource loading the specified directory as the base load resources
    /// </summary>
    public class BasicSubresourceLoader : ISubresourceLoader
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="baseDir">The base directory</param>
        public BasicSubresourceLoader(string baseDir)
        {
            if (string.IsNullOrEmpty(baseDir))
            {
                this.BaseDirectory = ".\\";
                return;
            }
            this.BaseDirectory = Path.GetFullPath(baseDir);
        }

        /// <summary>
        ///     Base directory
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        ///     Loads the specified resource
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <returns>Stream for the resource</returns>
        public Stream getSubresourceByName(string name)
        {
            if (Path.GetExtension(name).ToUpper().Equals(".TGA"))
            {
                if (string.IsNullOrEmpty(this.BaseDirectory)) return TargaSolver.LoadTargaImage(name);
                return TargaSolver.LoadTargaImage(Path.Combine(this.BaseDirectory, name));
            }else if (string.IsNullOrEmpty(this.BaseDirectory)) return File.OpenRead(name);
            else
            {
                string path = Path.Combine(this.BaseDirectory, name);
                if (File.Exists(path))
                {
                    return File.OpenRead(path);
                }
                else
                {
                    return null;
                    Debug.WriteLine(string.Format("\"{0}\"は見つかりませんでした。",path));
                }
            }
        }
    }
}