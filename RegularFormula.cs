using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelChecker {
	abstract class RegularFormula {
		public string Multiplier { get; set; }

		public abstract RegularFormula Clone();
		protected RegularFormula(string multiplier) {
			Multiplier = multiplier;
		}
		public abstract bool Matches(string actionName);

		public static bool WildMatch(string pattern, string test) {
			// wildcard match
			int pIdx = 0;
			int tIdx = 0;

			while (tIdx < test.Length && pIdx < pattern.Length && pattern[pIdx] != '*') {
				if (pattern[pIdx] != test[tIdx] && pattern[pIdx] != '?')
					return false;
				pIdx++;
				tIdx++;
			}

			int mp = 0;
			int cp = 0;
			while (tIdx < test.Length && pIdx < pattern.Length) {
				if (pattern[pIdx] == '*') {
					pIdx++;
					if (pIdx == pattern.Length) return true;
					mp = pIdx;
					cp = tIdx + 1;
				}
				else if (pattern[pIdx] == test[tIdx] || pattern[pIdx] == '?') {
					pIdx++;
					tIdx++;
				}
				else {
					pIdx = mp;
					tIdx = cp++;
				}
			}

			while (pIdx < pattern.Length && pattern[pIdx] == '*')
				pIdx++;

			return pIdx == pattern.Length;

		}
	}

	class SingleAction : RegularFormula {
		public string Action;
		public SingleAction(string action, string multiplier)
			: base(multiplier) {
			Action = action;
		}
		public override string ToString() {
			return Action + Multiplier;
		}
		public override RegularFormula Clone() {
			return new SingleAction(Action, Multiplier);
		}

		public override bool Matches(string actionName) {
			return WildMatch(Action, actionName);
		}

	}

	class NegateAction : RegularFormula {
		public RegularFormula Inner;
		public NegateAction(RegularFormula inner)
			: base("") {
			Inner = inner;
		}
		public override RegularFormula Clone() {
			return new NegateAction(Inner);
		}
		public override bool Matches(string actionName) {
			return !Inner.Matches(actionName);
		}
		public override string ToString() {
			return "not(" + Inner + ")";
		}
	}

	class NestedFormula : RegularFormula {
		public RegularFormula Inner;
		public NestedFormula(RegularFormula inner, string multiplier)
			: base(multiplier) {
			Inner = inner;
		}
		public override string ToString() {
			return "(" + Inner + ")" + Multiplier;
		}
		public override RegularFormula Clone() {
			return new NestedFormula(Inner, Multiplier);
		}

		public override bool Matches(string actionName) {
			throw new InvalidOperationException("this shouldn't happen due to the rewriter");
		}
	}

	class SequenceFormula : RegularFormula {
		public RegularFormula First;
		public RegularFormula Sequence;
		public SequenceFormula(RegularFormula first, RegularFormula seq)
			: base("") { /* sequences cannot have multiplier, need to be nested for that */
			First = first;
			Sequence = seq;
		}
		public override string ToString() {
			return First + "." + Sequence;
		}
		public override RegularFormula Clone() {
			return new SequenceFormula(First.Clone(), Sequence.Clone());
		}

		public override bool Matches(string actionName) {
			throw new InvalidOperationException("this shouldn't happen due to the rewriter");
		}
	}

	class PlusFormula : RegularFormula {
		public RegularFormula Left, Right;
		public PlusFormula(RegularFormula left, RegularFormula right, string multiplier)
			: base(multiplier) {
			Left = left;
			Right = right;
		}
		public override string ToString() {
			return "(" + Left + "+" + Right + ")" + Multiplier;
		}
		public override RegularFormula Clone() {
			return new PlusFormula(Left.Clone(), Right.Clone(), Multiplier);
		}

		public override bool Matches(string actionName) {
			return Left.Matches(actionName) || Right.Matches(actionName);
		}
	}

}
