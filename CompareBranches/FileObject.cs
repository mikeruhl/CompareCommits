using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareBranches
{
    public class FileObject
    {
        private readonly string _sourcePath;
        private readonly string _targetPath;
        private readonly string _relativePath;

        public FileObject(string sourceBase, string targetBase, string relativeFilePath)
        {
            _sourcePath = Path.Combine(sourceBase, relativeFilePath);
            _targetPath = Path.Combine(targetBase, relativeFilePath);
            _relativePath = relativeFilePath;
        }

        public string SourcePath => _sourcePath;
        public string TargetPath => _targetPath;
        public string RelativePath => _relativePath;
    }
}
