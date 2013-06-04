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
			var p = new MuCalculusParser();
			p.Setup();
			bool b = p.Parse(new StringReader("nu X.((mu Y . [b]Y) && nu Z.((<b> X) && (<a>(Z && <a> X))))\r\n"));

			string ltsPath = args[0];
			string formulasPath = args[1];
			Log("LTS: {0}", ltsPath); 
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
			Log();
			logWriter.Flush();

			logWriter.Close();
			Console.ReadKey();
		}

		static LTS ChessBoard(int boardSize) {
			// this procedure outputs a board of size boardSize x boardSize in aldebaran format
			var lts = new LTS();
			var board = new LTSState[boardSize, boardSize];
			int stateNum = 0;
			var initial = new LTSState(stateNum++.ToString(), lts);
			lts.InitialState = initial;
			for (int i = 0; i < boardSize; i++) {
				for (int j = 0; j < boardSize; j++) {
					var cell = new LTSState(stateNum++.ToString(), lts);
					board[i, j] = cell;
					lts.States.Add(cell);
				}
			}

			lts.States.Add(initial);
			for (int i = 0; i < boardSize; i++) {
				lts.Transitions.Add(new LTSTransition(initial, board[boardSize - 1, i], "start(" + (boardSize - 1) + "," + i + ")"));
				if (i != boardSize - 1) // don't want 2 transitions to same init
					lts.Transitions.Add(new LTSTransition(initial, board[i, boardSize - 1], "start(" + i + "," + (boardSize - 1) + ")"));
			}

			for (int y = boardSize - 1; y >= 0; y--) {
				for (int x = boardSize - 1; x >= 0; x--) {
					var thisCell = board[x, y];
					// horizontal moves
					for (int i = x - 1; i >= 0; i--)
						lts.Transitions.Add(new LTSTransition(thisCell, string.Format("to({0},{1})", i, y), board[i, y]));
					// vertical moves
					for (int j = y - 1; j >= 0; j--)
						lts.Transitions.Add(new LTSTransition(thisCell, string.Format("to({0},{1})", x, j), board[x, j]));
					// diagonal moves
					int k = x - 1, l = y - 1;
					while (k >= 0 && l >= 0)
						lts.Transitions.Add(new LTSTransition(thisCell, string.Format("to({0},{1})", k, l), board[k--, l--]));
				}
			}

			return lts;
		}

		private static void Benchmark(List<MuFormula> list) {
			Log("Formula; LTS Size; Algorithm; Time");

			double[,] results = new double[list.Count * 2, 13];
			bool lastResult = false;

			foreach (var formula in list) {
				int num_runs = 4;
				for (int i = 2; i <= 13; i++) {
					if (i >= 11) num_runs = 1;
					var sw = Stopwatch.StartNew();
					var lts = LTS.Parse("../demanding/demanding_" + i.ToString(CultureInfo.InvariantCulture) + ".aut");
					Log("Read input set in {0}ms", sw.ElapsedMilliseconds);

					sw.Reset(); sw.Start();
					for (int j = 0; j < num_runs; j++)
						lastResult = NaiveSolver.Solve(formula, lts, new Environment()).Contains(lts.InitialState);
					results[list.IndexOf(formula) * 2 + 0, i - 2] = (double)sw.ElapsedMilliseconds / num_runs;
					Log("{0}\t{1}\t{2}\t{3}\t{4}", list.IndexOf(formula) + 1, i, "Naive      ", (double)sw.ElapsedMilliseconds / (double)num_runs, lastResult);

					sw.Reset(); sw.Start();
					for (int j = 0; j < num_runs; j++)
						lastResult = EmersonLei.Solve(formula, lts, new Environment()).Contains(lts.InitialState);
					results[list.IndexOf(formula) * 2 + 1, i - 2] = (double)sw.ElapsedMilliseconds / num_runs;
					Log("{0}\t{1}\t{2}\t{3}\t{4}", list.IndexOf(formula) + 1, i, "Emerson-Lei", (double)sw.ElapsedMilliseconds / (double)num_runs, lastResult);

					logWriter.Flush();
					lts = null;
				}
			}

			Log("");
			Log("");
			Log("Results table:");
			Log("lts size\tformula1 naive\tformula1 emers\tformula2 naive\tformula2 emers\tformula3 naive\tformula3 emers\tformula4 naive\tformula4 emers");
			for (int j = 0; j < results.GetLength(1); j++) {
				var sb = new StringBuilder();
				sb.Append((j + 2).ToString());
				for (int i = 0; i < results.GetLength(0); i++)
					sb.AppendFormat("\t{0}", results[i, j]);
				Log(sb.ToString());
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
