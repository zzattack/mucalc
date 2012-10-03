using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModelChecker {
	class EmersonLei {

		private static void Init(IEnumerable<Variable> variables, LTS lts, Environment env) {
			foreach (var v in variables) {
				if (v.Binder is Mu)
					env[v] = new HashSet<LTSState>();
				else if (v.Binder is Nu)
					env[v] = new HashSet<LTSState>(lts.States);
			}
		}

		public static HashSet<LTSState> Solve(MuFormula formula, LTS lts, Environment env, bool init = true) {

			if (init) {
				var allVariables = new List<Variable>();
				if (formula is Variable) allVariables.Add((Variable)formula);
				allVariables.AddRange(formula.AllSubFormulas.OfType<Variable>());
				allVariables.AddRange(formula.AllSubFormulas.OfType<Mu>().Select(mu => mu.Variable));
				allVariables.AddRange(formula.AllSubFormulas.OfType<Nu>().Select(nu => nu.Variable));
				Init(allVariables.Distinct(), lts, env);
			}

			if (formula is Proposition) {
				var prop = formula as Proposition;
				return bool.Parse(prop.Value) ? new HashSet<LTSState>(lts.States) : new HashSet<LTSState>();
			}

			else if (formula is Variable) {
				return env.GetVariable(formula as Variable);
			}

			else if (formula is Negation) {
				var neg = formula as Negation;
				return new HashSet<LTSState>(lts.States.Except(
					Solve(neg.Formula, lts, env, false)));
			}

			else if (formula is Conjunction) {
				var conj = formula as Conjunction;
				var leftStates = Solve(conj.Left, lts, env, false);
				var rightStates = Solve(conj.Right, lts, env, false);
				return new HashSet<LTSState>(leftStates.Intersect(rightStates));
			}


			else if (formula is Disjunction) {
				var disj = formula as Disjunction;
				var leftStates = Solve(disj.Left, lts, env, false);
				var rightStates = Solve(disj.Right, lts, env, false);
				return new HashSet<LTSState>(leftStates.Union(rightStates));
			}

			else if (formula is Box) {
				var box = formula as Box;

				// box a f = { s | forall t. s -a-> t ==> t in [[f]]e }
				// i.e. the set of states for which all a-transitions go to a state in which f holds
				var fe = Solve(box.Formula, lts, env, false);

				return new HashSet<LTSState>(lts.States.Where(
					// states where, for all outtransitions with action a, the Formula holds in the direct successor 
					state =>
					state.OutTransitions.All(tr => tr.Action != box.Action || fe.Contains(tr.Right))
					));
			}

			else if (formula is Diamond) {
				var diamond = formula as Diamond;
				var shorthand = new Negation(
					// <a>f == [a](not f)
					new Box(diamond.RegularFormula, new Negation(diamond.Formula))
					);
				return Solve(shorthand, lts, env, false);
			}

			else if (formula is Mu) {
				var mu = formula as Mu;
				if (mu.Parent is Nu) {
					// surrounding binder is nu
					// reset open subformulae of form mu Xk.g set env[k]=false
					foreach (var innerMu in formula.AllSubFormulas.OfType<Mu>().Where(m => m.FreeVariables.Count > 0))
						env[innerMu.Variable] = new HashSet<LTSState>();
				}


				HashSet<LTSState> Xold;
				do {
					Xold = env.GetVariable(mu.Variable);
					env[mu.Variable] = Solve(mu.Formula, lts, env, false);
				} while (Xold.Count != env[mu.Variable].Count);
				return env[mu.Variable];

				/*
				return FixedPoint.LFP(delegate(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
					// repeats tau: X := solve(f)
					var X = tuple.Item1;
					X = Solve(mu.Formula, lts, env);
					env[mu.Variable] = X;
					return Tuple.Create(X, lts, env);
				}, lts, env);*/
			}

			else if (formula is Nu) {
				var nu = formula as Nu;
				if (nu.Parent is Mu) {
					// surrounding binder is mu
					// reset open subformulae of form nu Xk.g set env[k]=true
					foreach (var innerNu in formula.AllSubFormulas.OfType<Nu>().Where(m => m.FreeVariables.Count > 0))
						env[innerNu.Variable] = new HashSet<LTSState>(lts.States);
				}

				HashSet<LTSState> Xold;
				do {
					Xold = env[nu.Variable];
					env[nu.Variable] = Solve(nu.Formula, lts, env, false);
				} while (Xold.Count != env[nu.Variable].Count);
				return env[nu.Variable];

				/*
				return FixedPoint.GFP(delegate(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
					// repeats tau: X := solve(f)
					var X = tuple.Item1;
					X = Solve(nu.Formula, lts, env);
					env[nu.Variable] = X;

					return Tuple.Create(X, lts, env);
				}, lts, env);
				*/
			}

			throw new InvalidDataException("formula not valid in our grammar");
		}
	}
}
