using System.Collections.Generic;

namespace ModelChecker {
	class Environment : Dictionary<Variable, HashSet<LTSState>> {
		public HashSet<LTSState> GetVariable(Variable var) {
			if (!ContainsKey(var))
				this[var] = new HashSet<LTSState>();
			return this[var];
		}
	}
}
