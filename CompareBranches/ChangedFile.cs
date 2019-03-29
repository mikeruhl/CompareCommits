using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffMatchPatch;

namespace CompareBranches
{
    public class ChangedFile
    {
        public FileObject FileObject { get; set; }
        public List<Diff> ChangedItems { get; set; }
    }
}
