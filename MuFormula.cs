using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelChecker {
	using PredicateTransformer =
		Func<Tuple<HashSet<LTSState>, LTS, Environment>,
		Tuple<HashSet<LTSState>>, LTS, Environment>;

	public abstract class MuFormula {
		public abstract HashSet<LTSState> Evaluate(Environment env, LTS lts);
		public abstract int NestingDepth { get; }
		public abstract int AlternationDepth { get; }
		public abstract int DependentAlternationDepth { get; }
		public abstract List<MuFormula> SubFormulas { get; }
	}

	public class Proposition : MuFormula {
		public string Value;
		public Proposition(string value) {
			Value = value;
		}
		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			return bool.Parse(Value) ? new HashSet<LTSState>(lts.States) : new HashSet<LTSState>();
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

	public class Variable : MuFormula {
		public string Name;
		public Variable(string name) {
			Name = name;
		}

		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			return env.GetVariable(this);
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
	}

	public class Negation : MuFormula {
		public MuFormula Formula;
		public Negation(MuFormula formula) {
			Formula = formula;
		}

		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			return new HashSet<LTSState>(lts.States.Except(Formula.Evaluate(env, lts)));
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

	public class Conjunction : MuFormula {
		public MuFormula Left, Right;
		public Conjunction(MuFormula left, MuFormula right) {
			Left = left;
			Right = right;
		}

		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			var leftStates = Left.Evaluate(env, lts);
			var rightStates = Right.Evaluate(env, lts);
			return new HashSet<LTSState>(leftStates.Intersect(rightStates));
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
				var ret = new List<MuFormula>();
				ret.Add(Left);
				ret.AddRange(Left.SubFormulas);
				ret.Add(Right);
				ret.AddRange(Right.SubFormulas);
				return ret;
			}
		}
	}

	public class Disjunction : MuFormula {
		public MuFormula Left, Right;
		public Disjunction(MuFormula left, MuFormula right) {
			Left = left;
			Right = right;
		}

		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			var leftStates = Left.Evaluate(env, lts);
			var rightStates = Right.Evaluate(env, lts);
			return new HashSet<LTSState>(leftStates.Union(rightStates));
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
				var ret = new List<MuFormula>();
				ret.Add(Left);
				ret.AddRange(Left.SubFormulas);
				ret.Add(Right);
				ret.AddRange(Right.SubFormulas);
				return ret;
			}
		}
	}

	public class Box : MuFormula {
		public string Action;
		public MuFormula Formula;
		public Box(string action, MuFormula formula) {
			Action = action;
			Formula = formula;
		}
		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			// box a f = { s | forall t. s -a-> t ==> t in [[f]]e }
			// i.e. the set of states for which all a-transitions go to a state in which f holds
			var fe = Formula.Evaluate(env, lts);

			return new HashSet<LTSState>(lts.States.Where(
				// states where, for all outtransitions with action a, the Formula holds in the direct successor 
				state => state.OutTransitions.All(tr => tr.Action != Action || fe.Contains(tr.Right))
			));
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
				var ret = new List<MuFormula>();
				ret.Add(Formula);
				ret.AddRange(Formula.SubFormulas);
				return ret;
			}
		}
	}

	public class Diamond : MuFormula {
		public string Action;
		public MuFormula Formula;
		public Diamond(string action, MuFormula formula) {
			Action = action;
			Formula = formula;
		}
		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			// <a>f := not([a](not f))
			// not
			var shorthand = new Negation(
				//[a](not f)
				new Box(Action, new Negation(Formula))
			);
			return shorthand.Evaluate(env, lts);
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
				var ret = new List<MuFormula>();
				ret.Add(Formula);
				ret.AddRange(Formula.SubFormulas);
				return ret;
			}
		}
	}

	public class Mu : MuFormula {
		public Variable Variable;
		public MuFormula Formula;

		public Mu(Variable var, MuFormula pred) {
			Variable = var;
			Formula = pred;
		}

		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			var cloneEnv = env.Clone();
			cloneEnv[Variable] = new HashSet<LTSState>(/* empty set */);
			return FixedPoint.LFP(Tau, lts, cloneEnv);
		}

		private Tuple<HashSet<LTSState>, LTS, Environment> Tau(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
			var X = tuple.Item1;
			var lts = tuple.Item2;
			var env = tuple.Item3;

			X = Formula.Evaluate(env, lts);
			env[Variable] = X;

			return Tuple.Create(X, lts, env);
		}
		public override string ToString() {
			return "mu" + Variable + "." + Formula;
		}

		public override int NestingDepth {
			get { return 1 + Formula.NestingDepth; }
		}

		public override int AlternationDepth {
			get {
				int max = 0;
				foreach (var v in SubFormulas) {
					if (v is Nu)
						max = Math.Max(max, v.AlternationDepth);
				}
				return max + 1;
			}
		}

		public override int DependentAlternationDepth {
			get {
				int max = 0;
				foreach (var v in SubFormulas) {
					if (v is Nu && v.SubFormulas.Contains(Variable))
						max = Math.Max(max, v.DependentAlternationDepth);
				}
				return max + 1;
			}
		}

		public override List<MuFormula> SubFormulas {
			get {
				var ret = new List<MuFormula>();
				ret.Add(Formula);
				ret.AddRange(Formula.SubFormulas);
				return ret;
			}
		}

	}

	public class Nu : MuFormula {
		public Variable Variable;
		public MuFormula Formula;

		public Nu(Variable var, MuFormula pred) {
			Variable = var;
			Formula = pred;
		}

		public override HashSet<LTSState> Evaluate(Environment env, LTS lts) {
			var cloneEnv = env.Clone();
			cloneEnv[Variable] = new HashSet<LTSState>(lts.States);
			return FixedPoint.GFP(Tau, lts, cloneEnv);
		}

		private Tuple<HashSet<LTSState>, LTS, Environment> Tau(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
			var X = tuple.Item1;
			var lts = tuple.Item2;
			var env = tuple.Item3;

			X = Formula.Evaluate(env, lts);
			env[Variable] = X;

			return Tuple.Create(X, lts, env);
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
				foreach (var v in SubFormulas) {
					if (v is Mu)
						max = Math.Max(max, v.AlternationDepth);
				}
				return max + 1;
			}
		}

		public override int DependentAlternationDepth {
			get {
				int max = 0;
				foreach (var v in SubFormulas) {
					if (v is Mu && v.SubFormulas.Contains(Variable))
						max = Math.Max(max, v.DependentAlternationDepth);
				}
				return max + 1;
			}
		}

		public override List<MuFormula> SubFormulas {
			get {
				var ret = new List<MuFormula>();
				ret.Add(Formula);
				ret.AddRange(Formula.SubFormulas);
				return ret;
			}
		}
	}
}
