using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ModelChecker {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            string ltsPath = args[0];
            string formulasPath = args[1];

            var parser = new MuCalculusParser();
            parser.Setup();
            var fileStream = new FileStream(formulasPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            bool parseResult = parser.Parse(new StreamReader(fileStream, Encoding.Default));
            fileStream.Close();
            if (!parseResult)
                Console.WriteLine("Failed to parse!");

            Environment env = new Environment();
            LTS lts = LTS.Parse(ltsPath);

            foreach (MuFormula f in parser.formulas) {
                Console.WriteLine("----------------------------------");
                OutputResult(f, f.Evaluate(env.Clone(), lts));
            }

            Console.WriteLine();
            Console.ReadKey();
        }

        private static void OutputResult(MuFormula f, HashSet<LTSState> hashSet) {
            Console.WriteLine("Formula {0}");
            Console.Write("\t ND: {0}, AD: {1}, DAD: {2}", f.NestingDepth, f.AlternationDepth, f.DependentAlternationDepth);
            if (hashSet.Count == 0)
                Console.WriteLine("\tHolds in no states");
            else
                Console.WriteLine("\tHolds in {0}", string.Join(", ", hashSet));
        }
    }
}
