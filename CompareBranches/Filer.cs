using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NChardet;
using ShellProgressBar;

namespace CompareBranches
{
    public class Filer
    {
        public Filer()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public string GetFileContents(string path, ProgressBar pbar)
        {
            var childOptions = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Green,
                BackgroundColor = ConsoleColor.DarkGreen,
                ProgressCharacter = '─'
            };

                var det = new Detector(0);
            var obs = new CharsetDetectionObserver();
            det.Init(obs);
                            
            var fileText = string.Empty;
            var buffer = new byte[1024];

            int len;
            var done = false;
            var isAscii = true;

            using (var reader = new FileStream(path, FileMode.Open))
            using (var cBar = pbar.Spawn((int)(reader.Length/1024), path, childOptions))
            {
                while ((len = reader.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (isAscii)
                        isAscii = det.isAscii(buffer, len);

                    if (!isAscii && !done)
                        done = det.DoIt(buffer, len, false);
                    cBar.Tick();
                }

                det.DataEnd();
            }

            if (isAscii)
                return File.ReadAllText(path, Encoding.ASCII);
            if (obs.Charset != null)
            {
                var encoding = Encoding.GetEncoding(obs.Charset);
                return File.ReadAllText(path, encoding);
            }

            var prob = det.getProbableCharsets();
            try
            {
                if (prob[0] == "nomatch")
                    return null;
                return File.ReadAllText(path, Encoding.GetEncoding(prob[0])); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
