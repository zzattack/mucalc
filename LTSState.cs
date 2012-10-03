using System.Collections.Generic;
using System.Linq;

namespace ModelChecker {
	class LTSState {
		public readonly string Name;
		public LTS LTS;
		private readonly Dictionary<string, List<LTSTransition>> _outTransitions = new Dictionary<string, List<LTSTransition>>();
		private readonly Dictionary<string, List<LTSTransition>> _inTransitions = new Dictionary<string, List<LTSTransition>>();
		
		public bool Tag { get; set; }

		public LTSState(string name, LTS lts) {
			Name = name;
			LTS = lts;
		}

		public override string ToString() {
			return Name;
		}

		public List<LTSTransition> GetInTransitions(RegularFormula formula) {
			var matchingActions = LTS.Actions.Where(formula.Matches);
			var ret = new List<LTSTransition>();
			foreach (var action in matchingActions) {
				List<LTSTransition> list;
				if (!_inTransitions.TryGetValue(action, out list))
					list = _inTransitions[action] = new List<LTSTransition>();
				ret.AddRange(list);
			}
			return ret;
		}

		public List<LTSTransition> GetOutTransitions(RegularFormula formula) {
			var matchingActions = LTS.Actions.Where(formula.Matches);
			var ret = new List<LTSTransition>();
			foreach (var action in matchingActions) {
				List<LTSTransition> list;
				if (!_outTransitions.TryGetValue(action, out list))
					list = _outTransitions[action] = new List<LTSTransition>();
				ret.AddRange(list);
			}
			return ret;
		}

		public List<LTSTransition> Transitions {
			get {
				return LTS.Transitions.Where(tr => tr.Left == this || tr.Right == this).ToList();
			}
		}

		public override bool Equals(object obj) {
			return ((LTSState)obj).Name == Name;
		}

		public override int GetHashCode() {
			return Name.GetHashCode();
		}

		internal void AddInTransition(LTSTransition trans) {
			List<LTSTransition> list;
			if (!_inTransitions.TryGetValue(trans.Action, out list)) {
				list = _inTransitions[trans.Action] = new List<LTSTransition>();
			}
			list.Add(trans);
		}

		internal void AddOutTransition(LTSTransition trans) {
			List<LTSTransition> list;
			if (!_outTransitions.TryGetValue(trans.Action, out list)) {
				list = _outTransitions[trans.Action] = new List<LTSTransition>();
			}
			list.Add(trans);
		}

	}
}
