using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModelChecker {
	class EmersonLei {

		private static void Init(IEnumerable<Variable> variables, LTS lts, Environment env) {
			foreach (var v in variables) {
				if (v.Parent is Mu)
					env[v] = new HashSet<LTSState>();
				else if (v.Parent is Nu)
					env[v] = new HashSet<LTSState>(lts.States);
			}
		}

		public static HashSet<LTSState> Solve(MuFormula formula, LTS lts, Environment env, bool init = true) {

			if (init) {
				var allVariables = new List<Variable>();
				if (formula is Variable) allVariables.Add((Variable)formula);
				allVariables.AddRange(formula.SubFormulas.OfType<Variable>());
				Init(allVariables, lts, env);
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
					Solve(neg.Formula, lts, env)));
			}

			else if (formula is Conjunction) {
				var conj = formula as Conjunction;
				var leftStates = Solve(conj.Left, lts, env);
				var rightStates = Solve(conj.Right, lts, env);
				return new HashSet<LTSState>(leftStates.Intersect(rightStates));
			}


			else if (formula is Disjunction) {
				var disj = formula as Disjunction;
				var leftStates = Solve(disj.Left, lts, env);
				var rightStates = Solve(disj.Right, lts, env);
				return new HashSet<LTSState>(leftStates.Union(rightStates));
			}

			else if (formula is Box) {
				var box = formula as Box;

				// box a f = { s | forall t. s -a-> t ==> t in [[f]]e }
				// i.e. the set of states for which all a-transitions go to a state in which f holds
				var fe = Solve(box.Formula, lts, env);

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
					new Box(diamond.Action, new Negation(diamond.Formula))
					);
				return Solve(shorthand, lts, env);
			}

			else if (formula is Mu) {
				var mu = formula as Mu;
				if (mu.Parent is Nu) {
					// surrounding binder is nu
					// reset open subformulae of form mu Xk.g set env[k]=false
					foreach (var openMu in formula.SubFormulas.OfType<Mu>().Where(f => !f.IsBound())) {
						env[openMu.Variable] = new HashSet<LTSState>();
					}
				}

				return FixedPoint.LFP(delegate(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
					// repeats tau: X := solve(f)
					var X = tuple.Item1;
					X = Solve(mu.Formula, lts, env);
					env[mu.Variable] = X;
					return Tuple.Create(X, lts, env);
				}, lts, env);
			}

			else if (formula is Nu) {
				var nu = formula as Nu;


				if (nu.Parent is Mu) {
					// surrounding binder is mu
					// reset open subformulae of form nu Xk.g set env[k]=true
					foreach (var openNu in formula.SubFormulas.OfType<Nu>().Where(f => !f.IsBound())) {
						env[openNu.Variable] = new HashSet<LTSState>(lts.States);
					}
				}

				return FixedPoint.GFP(delegate(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
					// repeats tau: X := solve(f)
					var X = tuple.Item1;
					X = Solve(nu.Formula, lts, env);
					env[nu.Variable] = X;

					return Tuple.Create(X, lts, env);
				}, lts, env);

			}

			throw new InvalidDataException("formula not valid in our grammar");
		}
	}
}
