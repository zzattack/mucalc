using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelChecker {
	using PredicateTransformer =
		Func<Tuple<HashSet<LTSState>, LTS, Environment>,
		Tuple<HashSet<LTSState>>, LTS, Environment>;

	abstract class MuFormula {
		//public abstract HashSet<LTSState> Evaluate(Environment env, LTS lts);
		public abstract int NestingDepth { get; }
		public abstract int AlternationDepth { get; }
		public abstract int DependentAlternationDepth { get; }

		public abstract List<MuFormula> SubFormulas { get; }
		public List<MuFormula> AllSubFormulas {
			get {
				var ret = new List<MuFormula> { this };
				foreach (var subf in SubFormulas)
					ret.AddRange(subf.AllSubFormulas);
				return ret;
			}
		}

		public MuFormula Parent { get; private set; }
		public void SetParents(MuFormula parent) {
			Parent = parent;
			foreach (var f in SubFormulas)
				f.SetParents(this);
		}

		public List<Variable> FreeVariables {
			get { return AllSubFormulas.OfType<Variable>().Where(var => !var.IsBound(this)).ToList(); }
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
			// returns whether this variable is bound in the given subformula
			var parent = Parent;
			do {
				if (parent is Mu && ((Mu)parent).Formula.Equals(this)) return true;
				else if (parent is Nu && ((Nu)parent).Formula.Equals(this)) return true;
				parent = parent.Parent;
			} while (parent != subFormula);
			return false;
		}

		public MuFormula Binder {
			get {
				var p = Parent;
				while (p != null) {
					if (p is Mu && ((Mu)p).Variable.Equals(this)) break;
					if (p is Nu && ((Nu)p).Variable.Equals(this)) break;
					p = p.Parent;
				}
				return p;
			}
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
			get { return new List<MuFormula> { Left, Right }; }
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
			get { return new List<MuFormula> { Left, Right }; }
		}
	}

	class Box : MuFormula {
		public RegularFormula RegularFormula;
		public MuFormula Formula;

		public Box(RegularFormula regForm, MuFormula formula) {
			RegularFormula = regForm;
			Formula = formula;
		}
		public override string ToString() {
			return "[" + RegularFormula + "]" + Formula;
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
			get { return new List<MuFormula> { Formula }; }
		}
	}

	internal class Diamond : MuFormula {
		public RegularFormula RegularFormula;
 		public MuFormula Formula;

		public Diamond(RegularFormula regForm, MuFormula formula) {
			RegularFormula = regForm;
			Formula = formula;
		}

		public override string ToString() {
			return "<" + RegularFormula + ">" + Formula;
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
			get { return new List<MuFormula> { Formula }; }
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
			get { return 1 + AllSubFormulas.OfType<Nu>().Select(v => v.AlternationDepth).Concat(new[] { 0 }).Max(); }
		}

		public override int DependentAlternationDepth {
			get {
				int max = 0;
				foreach (var v in AllSubFormulas) {
					if (v is Nu && v.AllSubFormulas.Contains(Variable))
						max = Math.Max(max, v.DependentAlternationDepth);
				}
				return max + 1;
			}
		}

		public override List<MuFormula> SubFormulas {
			get { return new List<MuFormula> { Formula }; }
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
				int max = 0;
				foreach (var v in AllSubFormulas) {
					if (v is Mu)
						max = Math.Max(max, v.AlternationDepth);
				}
				return max + 1;
			}
		}

		public override int DependentAlternationDepth {
			get {
				int max = 0;
				foreach (var v in AllSubFormulas) {
					if (v is Mu && v.AllSubFormulas.Contains(Variable))
						max = Math.Max(max, v.DependentAlternationDepth);
				}
				return max + 1;
			}
		}

		public override List<MuFormula> SubFormulas {
			get { return new List<MuFormula> { Formula }; }
		}
	}
}
