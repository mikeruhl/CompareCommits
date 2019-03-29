# CompareCommits
Compares Text of all changed files between 2 commits using 2 checked out branches/commits

## Overview
This tool will take 2 directories of the same repository [source and target] and compare the diff between the 2 commits.  Since git diff compares binary, this tool is helpful because it will guess the charset encoding via [nchardet](https://github.com/bibaoke/NChardet) and compare the actual string values via [Diff Patch and Match](https://github.com/google/diff-match-patch).  Background on this is that our team needed to convert our repository's charset from windows-1252 to UTF-8.  It touched roughly 2000 files and we did not want to spot-check every one of them.  So, I wrote this to highlight the files with actual text changes.  Out of those ~2000 files, only 5 had a difference in content, so this saved us a lot of time.

## Setup
You will need the [.NET Core Runtime](https://dotnet.microsoft.com/download).

Build this project after cloning master.  To do that, you can refer to [this link](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build?tabs=netcore2x).

## Running
To run, enter `CompareBranches [sourceDir] [targetDir]` on the command line in the directory where CompareBranches.exe resides.

The tool will compare the 2 branches and output a file with the format of `output-[longtime].txt` in the assembly's executing directory.   This file is straightforward to read with each file separated by a line of equal signs and each difference noted.

## Special Thanks
[nchardet](https://github.com/bibaoke/NChardet)
[Diff Patch and Match](https://github.com/google/diff-match-patch)