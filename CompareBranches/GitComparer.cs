using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareBranches
{
    public class GitComparer
    {
        private readonly string _sourceDir;
        private readonly string _targetDir;
        private bool _hasHashed;
        HashSet<string> _fileList = new HashSet<string>();
        private string _targetBranch;

        public GitComparer(string sourceDir, string targetDir)
        {
            _sourceDir = sourceDir;
            _targetDir = targetDir;
        }

        public List<FileObject> GetEditedFiles()
        {
            if (!_hasHashed)
                GetCommitFiles(_sourceDir);

            var baseSource = GetGitCommand("rev-parse --show-toplevel", _sourceDir).Split("\n")[0].Replace("/", "\\");
            var baseTarget = GetGitCommand("rev-parse --show-toplevel", _targetDir).Split("\n")[0].Replace("/", "\\");

            var fileObjects = new List<FileObject>();

            foreach (var relativePath in _fileList)
            {
                if(!string.IsNullOrEmpty(relativePath))
                fileObjects.Add(new FileObject(baseSource, baseTarget, relativePath.Replace("/", "\\")));
            }

            return fileObjects;
        }

        private void GetCommitFiles(string initialFileDirectory)
        {
            if (_hasHashed)
                return;

            var branches = GetGitCommand("branch", _targetDir);
            _targetBranch = branches.Split("\n").FirstOrDefault(f => f.StartsWith("*")).Substring(2);
            if (_targetBranch.StartsWith("(HEAD detached at"))
                _targetBranch = _targetBranch.Substring(17, 9);
            var baseDir = GetGitCommand("--no-pager rev-parse --show-toplevel", initialFileDirectory).Split("\n")[0].Replace("/", "\\");
            var output = GetGitCommand($"--no-pager diff {_targetBranch} --name-only", baseDir).Split("\n");
            foreach (var path in output)
                _fileList.Add(path);
            _hasHashed = true;
        }

        public bool IsGitIgnored(FileInfo file)
        {
            return !string.IsNullOrEmpty(GetGitCommand($"check-ignore {file.FullName}", file.DirectoryName));
        }

        private string GetGitCommand(string argument, string workDirectory)
        {
            var pi = new ProcessStartInfo();
            pi.FileName = "git";
            pi.RedirectStandardError = true;
            pi.RedirectStandardOutput = true;
            pi.UseShellExecute = false;
            pi.WorkingDirectory = workDirectory;
            pi.Arguments = argument;

            var process = new Process();
            process.StartInfo = pi;
            process.Start();
            process.WaitForExit(5000);
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            return output;
        }
    }
}
