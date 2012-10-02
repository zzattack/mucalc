using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelChecker {
	using PredicateTransformer =
		Func<Tuple<HashSet<LTSState>, LTS, Environment>,
		Tuple<HashSet<LTSState>>, LTS, Environment>;

	public abstract class MuFormula {
		//public abstract HashSet<LTSState> Evaluate(Environment env, LTS lts);
		public abstract int NestingDepth { get; }
		public abstract int AlternationDepth { get; }
		public abstract int DependentAlternationDepth { get; }
		public abstract List<MuFormula> SubFormulas { get; }
		public MuFormula Parent { get; private set; }

		internal void SetParents(MuFormula parent) {
			Parent = parent;
			foreach (var f in SubFormulas)
				f.Parent = this;
		}
	}

	class Proposition : MuFormula {
		public string Value;
		public Proposition(string value) {
			Value = value;
		}
		public override string ToString() {
			return Value;
		}

		public override int NestingDepth {
			get { return 0; }
		}

		public override int AlternationDepth {
			get { return 0; }
		}

		public override int DependentAlternationDepth {
			get { return 0; }
		}

		public override List<MuFormula> SubFormulas {
			get { return new List<MuFormula>(); }
		}

	}

	class Variable : MuFormula {
		public readonly string Name;
		public Variable(string name) {
			Name = name;
		}

		public override string ToString() {
			return Name;
		}

		public override int NestingDepth {
			get { return 0; }
		}

		public override int AlternationDepth {
			get { return 0; }
		}

		public override int DependentAlternationDepth {
			get { return 0; }
		}

		public override List<MuFormula> SubFormulas {
			get { return new List<MuFormula>(); }
		}

		public override bool Equals(object obj) {
			return Name.Equals((obj as Variable).Name);
		}

		public override int GetHashCode() {
			return Name.GetHashCode();
		}

		public bool IsBound(MuFormula subFormula) {
			// returns whether this variable is bound in the givne subformula
			var parent = Parent;
			do {
				if (parent is Mu && ((Mu)parent).Formula.Equals(this)) return true;
				else if (parent is Nu && ((Nu)parent).Formula.Equals(this)) return true;
			} while (parent != subFormula);
			return false;
		}
	}

	class Negation : MuFormula {
		public MuFormula Formula;
		public Negation(MuFormula formula) {
			Formula = formula;
		}

		public override string ToString() {
			return "not(" + Formula + ")";
		}

		public override int NestingDepth {
			get { return 0; }
		}

		public override int AlternationDepth {
			get { return 0; }
		}

		public override int DependentAlternationDepth {
			get { return 0; }
		}

		public override List<MuFormula> SubFormulas {
			get { return new List<MuFormula>(); }
		}
	}

	class Conjunction : MuFormula {
		public MuFormula Left, Right;
		public Conjunction(MuFormula left, MuFormula right) {
			Left = left;
			Right = right;
		}

		public override string ToString() {
			return "(" + Left + " && " + Right + ")";
		}

		public override int NestingDepth {
			get { return Math.Max(Left.NestingDepth, Right.NestingDepth); }
		}

		public override int AlternationDepth {
			get { return Math.Max(Left.AlternationDepth, Right.AlternationDepth); }
		}

		public override int DependentAlternationDepth {
			get { return Math.Max(Left.DependentAlternationDepth, Right.DependentAlternationDepth); }
		}

		public override List<MuFormula> SubFormulas {
			get {
				var ret = new List<MuFormula> {Left};
				ret.AddRange(Left.SubFormulas);
				ret.Add(Right);
				ret.AddRange(Right.SubFormulas);
				return ret;
			}
		}
	}

	class Disjunction : MuFormula {
		public MuFormula Left, Right;
		public Disjunction(MuFormula left, MuFormula right) {
			Left = left;
			Right = right;
		}

		public override string ToString() {
			return "(" + Left + " || " + Right + ")";
		}

		public override int NestingDepth {
			get { return Math.Max(Left.NestingDepth, Right.NestingDepth); }
		}

		public override int AlternationDepth {
			get { return Math.Max(Left.AlternationDepth, Right.AlternationDepth); }
		}

		public override int DependentAlternationDepth {
			get { return Math.Max(Left.DependentAlternationDepth, Right.DependentAlternationDepth); }
		}

		public override List<MuFormula> SubFormulas {
			get {
				var ret = new List<MuFormula> {Left};
				ret.AddRange(Left.SubFormulas);
				ret.Add(Right);
				ret.AddRange(Right.SubFormulas);
				return ret;
			}
		}
	}

	class Box : MuFormula {
		public string Action;
		public MuFormula Formula;
		public Box(string action, MuFormula formula) {
			Action = action;
			Formula = formula;
		}
		public override string ToString() {
			return "[" + Action + "]" + Formula;
		}

		public override int NestingDepth {
			get { return Formula.NestingDepth; }
		}

		public override int AlternationDepth {
			get { return Formula.AlternationDepth; }
		}

		public override int DependentAlternationDepth {
			get { return Formula.DependentAlternationDepth; }
		}

		public override List<MuFormula> SubFormulas {
			get {
				var ret = new List<MuFormula> {Formula};
				ret.AddRange(Formula.SubFormulas);
				return ret;
			}
		}
	}

	class Diamond : MuFormula {
		public string Action;
		public MuFormula Formula;
		public Diamond(string action, MuFormula formula) {
			Action = action;
			Formula = formula;
		}
		public override string ToString() {
			return "<" + Action + ">" + Formula;
		}

		public override int NestingDepth {
			get { return Formula.NestingDepth; }
		}

		public override int AlternationDepth {
			get { return Formula.AlternationDepth; }
		}

		public override int DependentAlternationDepth {
			get { return Formula.DependentAlternationDepth; }
		}

		public override List<MuFormula> SubFormulas {
			get {
				var ret = new List<MuFormula> {Formula};
				ret.AddRange(Formula.SubFormulas);
				return ret;
			}
		}
	}

	internal class Mu : MuFormula {
		public Variable Variable;
		public MuFormula Formula;

		public Mu(Variable var, MuFormula pred) {
			Variable = var;
			Formula = pred;
		}

		public override string ToString() {
			return "mu" + Variable + "." + Formula;
		}

		public override int NestingDepth {
			get { return 1 + Formula.NestingDepth; }
		}

		public override int AlternationDepth {
			get {
				int max = SubFormulas.OfType<Nu>().Select(v => v.AlternationDepth).Concat(new[] {0}).Max();
				return max + 1;
			}
		}

		public override int DependentAlternationDepth {
			get {
				int max =
					(from v in SubFormulas where v is Nu && v.SubFormulas.Contains(Variable) select v.DependentAlternationDepth).Concat
						(new[] {0}).Max();
				return max + 1;
			}
		}

		public override List<MuFormula> SubFormulas {
			get {
				var ret = new List<MuFormula> {Formula};
				ret.AddRange(Formula.SubFormulas);
				return ret;
			}
		}

		public bool IsBound() {
			return SubFormulas.OfType<Variable>().Any(var => var.IsBound(this));
		}
	}

	class Nu : MuFormula {
		public Variable Variable;
		public MuFormula Formula;

		public Nu(Variable var, MuFormula pred) {
			Variable = var;
			Formula = pred;
		}

		public override string ToString() {
			return "nu" + Variable + "." + Formula;
		}

		public override int NestingDepth {
			get { return 1 + Formula.NestingDepth; }
		}

		public override int AlternationDepth {
			get {
				int max = SubFormulas.OfType<Mu>().Select(v => v.AlternationDepth).Concat(new[] {0}).Max();
				return max + 1;
			}
		}

		public override int DependentAlternationDepth {
			get {
				int max = (from v in SubFormulas where v is Mu && v.SubFormulas.Contains(Variable) select v.DependentAlternationDepth).Concat(new[] {0}).Max();
				return max + 1;
			}
		}

		public override List<MuFormula> SubFormulas {
			get {
				var ret = new List<MuFormula> {Formula};
				ret.AddRange(Formula.SubFormulas);
				return ret;
			}
		}

		public bool IsBound() {
			return SubFormulas.OfType<Variable>().Any(var => var.IsBound(this));
		}

	}
}
