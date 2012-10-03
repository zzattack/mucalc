using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

			Benchmark(parser.formulas);
			/*
			LTS lts = LTS.Parse(ltsPath);
			foreach (MuFormula f in parser.formulas) {
				Log("----------------------------------");
				OutputResult(f, lts);
			}*/

			logWriter.Flush();
			logWriter.Close();
			Console.ReadKey();
		}

		private static void Benchmark(List<MuFormula> list) {
			int num_runs = 1;

			Log("Formula; LTS Size; Algorithm; Time");

			foreach (var formula in list) {
				for (int i = 7; i <= 11; i++) {
					var lts = LTS.Parse("../dining/dining_" + i.ToString(CultureInfo.InvariantCulture) + ".aut");

					var sw = Stopwatch.StartNew();
					for (int j = 0; j < num_runs; j++)
						NaiveSolver.Solve(formula, lts, new Environment());
					Log("{0}\t{1}\t{2}\t{3}", list.IndexOf(formula) + 1, i, "Naive      ", sw.ElapsedMilliseconds * 20);

					sw.Reset(); sw.Start();
					for (int j = 0; j < num_runs; j++)
						EmersonLei.Solve(formula, lts, new Environment());
					Log("{0}\t{1}\t{2}\t{3}", list.IndexOf(formula) + 1, i, "Emerson-Lei", sw.ElapsedMilliseconds * 20);

					logWriter.Flush();
					lts = null;
				}
			}
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
			string logstr = string.Format(format, p);
			Console.WriteLine(string.Format("[{0:00000000}]\t\t{1}", sw.ElapsedMilliseconds, logstr));
			if (logWriter == null) logWriter = new StreamWriter("console_" + DateTime.Now.ToString("ddMMyy-HHmmss") + ".log");
			logWriter.WriteLine(logstr);
		}

		static TextWriter logWriter;

	}
}
