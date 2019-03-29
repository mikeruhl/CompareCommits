using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DiffMatchPatch;
using NChardet;
using ShellProgressBar;

namespace CompareBranches
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length != 2)
            {
                Exit("Call application with parameters: [sourcepath] [destinationpath].");
            }

            Console.WriteLine("Setting up...");
            var sourcePath = args[0];
            var destPath = args[1];
            var sourceDir = new DirectoryInfo(sourcePath);
            var destDir = new DirectoryInfo(destPath);

            if (!sourceDir.Exists)
                Exit("Source Path does not exist");
            if (!destDir.Exists)
                Exit("Destination Path does not exist");

            var gitComparer = new GitComparer(sourcePath, destPath);
            //TODO: get files
            var filesToCompare = gitComparer.GetEditedFiles();

            var changedList = new List<ChangedFile>();
            var filer = new Filer();
            var troubleList = new List<FileObject>();

            var options = new ProgressBarOptions
            {
                ProgressCharacter = '-',
                ProgressBarOnBottom = false
            };




            using (var pbar = new ProgressBar(filesToCompare.Count, "Converting Files", options))

                foreach (var file in filesToCompare)
                {
                    pbar.Message = file.RelativePath;
                    if (gitComparer.IsGitIgnored(new FileInfo(file.SourcePath)))
                        continue;
                    var progressBar = pbar;
                    var sourceTask = new Task<string>(() => filer.GetFileContents(file.SourcePath, progressBar));
                    sourceTask.Start();
                    var targetTask = new Task<string>(() => filer.GetFileContents(file.TargetPath, progressBar));
                    targetTask.Start();
                    await Task.WhenAll(sourceTask, targetTask);

                    if (sourceTask.Result == null | targetTask.Result == null)
                    {
                        troubleList.Add(file);
                        continue;
                    }

                    var diff = new diff_match_patch();
                    var diffs = diff.diff_main(sourceTask.Result, targetTask.Result);

                    var changed = new ChangedFile();
                    changed.FileObject = file;

                    diff.diff_cleanupSemantic(diffs);
                    diffs = diffs.Where(d => d.operation != Operation.EQUAL).ToList();
                    if (diffs.Count != 0)
                    {
                        changed.ChangedItems = diffs;
                        changedList.Add(changed);
                    }
                    pbar.Tick();
                }

            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);

            var outputFile = Path.Combine(fi.DirectoryName,
                $"output-{DateTime.Now.ToLongTimeString().Replace(":", ".")}.txt");

            using (var writer = new StreamWriter(outputFile))
            {
                foreach (var file in changedList)
                {
                    writer.WriteLine(file.FileObject.RelativePath);
                    for (var i = 0; i < file.ChangedItems.Count; i++)
                    {
                        writer.WriteLine($":Diff {i}. Operation: {file.ChangedItems[i].operation} Item: {file.ChangedItems[i]}");
                        writer.WriteLine("---------------------------------");
                        //writer.Write("Source:");
                        //writer.Write("---------------------------------");
                        //writer.Write(file.ChangedItems[i].SourceDiff);
                        //writer.Write("=================================");
                        //writer.Write("Target:");
                        //writer.Write("---------------------------------");
                        //writer.Write(file.ChangedItems[i].TargetDiff);
                        //writer.Write("_________________________________");

                    }

                    writer.WriteLine("=============================================");
                    writer.WriteLine();
                }

                writer.WriteLine("Errors below");
                writer.WriteLine("=============================================");
                foreach (var trouble in troubleList)
                {
                    writer.WriteLine(trouble.RelativePath);
                }

            }
            //File.WriteAllLines(outputFile, changedList.Select(f=>f.RelativePath));
        }

        private static void Exit(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}
