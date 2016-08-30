namespace MMF.MME.Includer
{
    /// <summary>
    ///     IncludeDirectories used to search for files
    /// </summary>
    public class IncludeDirectory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directory">Directory path</param>
        /// <param name="priorty">Priority</param>
        public IncludeDirectory(string directory, int priorty)
        {
            this.DirectoryPath = directory;
            this.Priorty = priorty;
        }

        /// <summary>
        /// Path of the directory
        /// </summary>
        public string DirectoryPath { get; private set; }

        /// <summary>
        /// The priority of the directory
        /// </summary>
        public int Priorty { get; private set; }
    }
}