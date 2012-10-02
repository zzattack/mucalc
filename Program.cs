using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ModelChecker {
	static class Program {
		static Stopwatch sw = Stopwatch.StartNew();
		
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
                Log("Failed to parse!");

            LTS lts = LTS.Parse(ltsPath);

			foreach (MuFormula f in parser.formulas) {
				Log("----------------------------------");
				OutputResult(f, lts);
            }

            Log();
            Console.ReadKey();
        }

		private static void OutputResult(MuFormula f, LTS lts) {
			Log("Formula {0}", f);
			Log("\t ND: {0}, AD: {1}, DAD: {2}", f.NestingDepth, f.AlternationDepth, f.DependentAlternationDepth);
			
			var naiveSol = NaiveSolver.Solve(f, lts, new Environment());
			Log("\tNAIVE: {0}", naiveSol.Contains(lts.InitialState));
			
			var emersonLeiSol = EmersonLei.Solve(f, lts, new Environment());
			Log("\tEMLEI: {0}", emersonLeiSol.Contains(lts.InitialState));

		}

		static void Log(string format = "", params object[] p) {
			Console.WriteLine(string.Format("[{0:00000}] -- {1}", sw.ElapsedMilliseconds, string.Format(format, p)));
		}
	}
}
