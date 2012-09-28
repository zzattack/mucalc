using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MS.Internal.Xml.XPath;

namespace ModelChecker {
    public class LTS {

        public HashSet<LTSState> States = new HashSet<LTSState>();
        public List<LTSTransition> Transitions = new List<LTSTransition>();

        public static LTS Parse(string file) {
            // parse aldebran format
            var ret = new LTS();
            StreamReader sr = new StreamReader(file, Encoding.Default);
            var lineRE = new Regex(@"\((\w+)\s*,\s*""([^\""]+?)""\s*,\s*(\w+)\s*\)");

            while (!sr.EndOfStream) {
                var line = sr.ReadLine();
                if (line.StartsWith("des")) continue;
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
    }
}
