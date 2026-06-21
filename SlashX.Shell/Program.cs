using System;
using System.Collections.Generic;
using System.Text;

namespace SlashX.Shell
{
    internal static class Program
    {
        public static Task<int> Main(string[] args)
        {
            return SlashX.Program.Main(args);
        }
    }
}
