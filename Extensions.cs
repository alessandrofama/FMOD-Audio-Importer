using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FMODAudioImporter
{
    static class Extensions
    {
        public static string Multiply(this string source, int multiplier)
        {
            StringBuilder sb = new StringBuilder(multiplier * source.Length);
            for (int i = 0; i < multiplier; i++)
            {
                sb.Append(source);
            }

            return sb.ToString();
        }

        public static void Sleep(double msec)
        {
            for (var since = DateTime.Now; (DateTime.Now - since).TotalMilliseconds < msec;)
                Thread.Sleep(TimeSpan.FromTicks(10));
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
            var uri = new Uri(relativeTo);
            var rel = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(path)).ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (rel.Contains(Path.DirectorySeparatorChar.ToString()) == false)
            {
                rel = $".{Path.DirectorySeparatorChar }{ rel }";
            }
            return rel;
        }

    }
}
