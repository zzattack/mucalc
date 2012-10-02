using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ModelChecker {
    class LTS {

        public HashSet<LTSState> States = new HashSet<LTSState>();
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

                    ret.Transitions.Add(new LTSTransition(startState, label, endState));
                }
            }

			ret.InitialState = ret.States.FirstOrDefault(s => s.Name == initial);
            return ret;
        }

        private static LTSState GetState(string stateName, LTS lts) {
            var state = lts.States.FirstOrDefault(s => s.Name == stateName);
            if (state == null) {
                state = new LTSState(stateName, lts);
                lts.States.Add(state);
            }
            return state;
        }
        
        internal LTS Clone() {
            var ret = new LTS();
            foreach (var s in States)
                ret.States.Add(s);
            foreach (var t in Transitions)
                ret.Transitions.Add(t);
            return ret;
        }

		public LTSState InitialState { get; private set; }
	}
}
