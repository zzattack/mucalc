using System.Collections.Generic;
using System.Linq;

namespace ModelChecker {
    class LTSState {
        public readonly string Name;
        public LTS LTS;

        public LTSState(string name, LTS lts) {
            Name = name;
            LTS = lts;
        }

        public override string ToString() {
            return Name;
        }

        public List<LTSTransition> InTransitions {
            get {
                return LTS.Transitions.Where(tr => tr.Right == this).ToList();
            }
        }

        public List<LTSTransition> OutTransitions {
            get {
                return LTS.Transitions.Where(tr => tr.Left == this).ToList();
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
