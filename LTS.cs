using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ModelChecker {
	class LTS {

		private Dictionary<string, LTSState> StatesMap = new Dictionary<string, LTSState>();
		public HashSet<LTSState> States = new HashSet<LTSState>();
		public HashSet<string> Actions = new HashSet<string>();

		public List<LTSTransition> Transitions = new List<LTSTransition>();

		public static LTS Parse(string file) {
			// parse aldebran format
			var ret = new LTS();
			var sr = new StreamReader(file, Encoding.Default);
			var lineRE = new Regex(@"\((\w+)\s*,\s*""([^\""]+?)""\s*,\s*(\w+)\s*\)");
			string initial = string.Empty;

			while (!sr.EndOfStream) {
				var line = sr.ReadLine();
				if (line.StartsWith("des")) {
					var m = Regex.Match(line, @"des\s*\((\w+?),(\w+?),(\w+?)\)");
					if (m.Success) {
						initial = m.Groups[1].Captures[0].Value;
					}
					continue;
				}

				var match = lineRE.Match(line);
				if (match.Success) {
					string start = match.Groups[1].Captures[0].Value;
					string label = match.Groups[2].Captures[0].Value;
					string end = match.Groups[3].Captures[0].Value;

					LTSState startState = GetState(start, ret);
					LTSState endState = GetState(end, ret);
					LTSTransition trans = new LTSTransition(startState, label, endState);

					ret.Actions.Add(label);
					ret.Transitions.Add(trans);
					startState.AddOutTransition(trans);
					endState.AddInTransition(trans);
				}
			}

			ret.InitialState = GetState(initial, ret);
			return ret;
		}

		private static LTSState GetState(string stateName, LTS lts) {
			LTSState state;
			if (lts.StatesMap.TryGetValue(stateName, out state))
				return state;
			else {
				state = new LTSState(stateName, lts);
				lts.StatesMap[stateName] = state;
				lts.States.Add(state);
			}
			return state;
		}

		public LTSState InitialState { get; private set; }

		internal void SaveTo(string path) {
			var sw = new StreamWriter(path);
			sw.WriteLine(string.Format("des ({0},{1},{2})", 0, Transitions.Count, States.Count));
			foreach (var tr in Transitions) {
				sw.WriteLine(string.Format("({0},\"{1}\",{2})", tr.Left, tr.Action, tr.Right));
			}
			sw.Flush();
			sw.Close();
		}
	}
}
