using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelChecker {
	public class Environment : Dictionary<Variable, HashSet<LTSState>> {

		public Environment Clone() {
			var ret = new Environment();
			foreach (var key in this.Keys)
				ret[key] = this[key];
			return ret;
		}
		public HashSet<LTSState> GetVariable(Variable var) {
			if (!ContainsKey(var))
				this[var] = new HashSet<LTSState>();
			return this[var];
		}

		/*		public LTS LTS;
				
				public void Replace(MuFormula variable, HashSet<LTSState> X) {
					Variables[((Variable)variable).Name] = new HashSet<LTSState>(X);
				}*/
	}
}
