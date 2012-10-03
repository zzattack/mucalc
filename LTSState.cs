using System.Collections.Generic;
using System.Linq;

namespace ModelChecker {
	class LTSState {
		public readonly string Name;
		public LTS LTS;
		private List<LTSTransition> _outTransitions;
		private List<LTSTransition> _inTransitions;

		public LTSState(string name, LTS lts) {
			Name = name;
			LTS = lts;
		}

		public override string ToString() {
			return Name;
		}

		public List<LTSTransition> InTransitions {
			get {
				if (_inTransitions == null) _inTransitions = LTS.Transitions.Where(tr => tr.Right == this).ToList();
				return _inTransitions;
			}
		}

		public List<LTSTransition> OutTransitions {
			get {
				if (_outTransitions == null) _outTransitions = LTS.Transitions.Where(tr => tr.Left == this).ToList();
				return _outTransitions;
			}
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

	}
}
