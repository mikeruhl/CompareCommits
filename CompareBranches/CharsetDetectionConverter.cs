using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NChardet;

namespace CompareBranches
{
    public class CharsetDetectionObserver : ICharsetDetectionObserver
    {
        public string Charset { get; private set; }
        public void Notify(string charset)
        {
            Charset = charset;
        }
    }
}
