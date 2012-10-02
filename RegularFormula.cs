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
	}

}
