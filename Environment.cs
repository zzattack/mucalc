using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelChecker {
    public class Environment {
        private Dictionary<string, HashSet<LTSState>> Variables = new Dictionary<string, HashSet<LTSState>>();
        public LTS LTS;

        public Environment Clone() {
            var ret = new Environment();
            ret.LTS = this.LTS.Clone();
            foreach (var e in Variables)
                ret.Variables[e.Key] = e.Value;
            return ret;
        }

        public HashSet<LTSState> GetVariable(string varName) {
            if (!Variables.ContainsKey(varName))
                Variables[varName] = new HashSet<LTSState>();
            return Variables[varName];
        }

        internal void Replace(MuFormula variable, HashSet<LTSState> X) {
            Variables[((Variable)variable).Name] = X;
        }
    }
}
