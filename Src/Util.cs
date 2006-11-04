/// Utils.cs  -  utility functions and classes

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace RT.Util
{
    /// <summary>
    /// This class offers some generic static functions which are hard to categorize
    /// under any more specific classes.
    /// </summary>
    public static class Ut
    {
        /// <summary>
        /// An application-wide random number generator - use this generator if all you
        /// need is a random number. Create a new generator only if you really need to.
        /// </summary>
        public static Random Rnd = new Random();

        /// <summary>
        /// Stores a copy of the value generated by AppPath. This way AppPath
        /// only needs to generate it once.
        /// </summary>
        private static string CachedAppPath = "";

        /// <summary>
        /// Returns the application path with a directory separator char at the end.
        /// The expression 'Ut.AppPath + "FileName"' yields a valid fully qualified
        /// file name. Supports network paths.
        /// </summary>
        public static string AppPath
        {
            get
            {
                if (CachedAppPath == "")
                {
                    CachedAppPath = Application.ExecutablePath;
                    CachedAppPath = CachedAppPath.Remove(
                        CachedAppPath.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1);
                }
                return CachedAppPath;
            }
        }

        /// <summary>
        /// This function returns a fully qualified name for the subpath, relative
        /// to the executable directory. This is for the purist programmers who can't
        /// handle AppPath returning something "invalid" :)
        /// </summary>
        public static string MakeAppSubpath(string subpath)
        {
            return Ut.AppPath + subpath;
        }

        /// <summary>
        /// Converts file size in bytes to a string in bytes, kbytes, Mbytes
        /// or Gbytes accordingly.
        /// </summary>
        /// <param name="size">Size in bytes</param>
        /// <returns>Converted string</returns>
        public static string SizeToString(long size)
        {
            if (size == 0)
            {
                return "0";
            }
            else if (size < 1024)
            {
                return size.ToString("#,###");
            }
            else if (size < 1024 * 1024)
            {
                return (size / 1024d).ToString("#,###.## k");
            }
            else if (size < 1024 * 1024 * 1024)
            {
                return (size / (1024d * 1024d)).ToString("#,###.## M");
            }
            else
            {
                return (size / (1024d * 1024d * 1024d)).ToString("#,###.## G");
            }
        }
    }
}
