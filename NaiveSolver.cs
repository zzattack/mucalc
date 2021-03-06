﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModelChecker {
	class NaiveSolver {
		public static HashSet<LTSState> Solve(MuFormula formula, LTS lts, Environment env) {

			if (formula is Proposition) {
				var prop = formula as Proposition;
				return bool.Parse(prop.Value) ? lts.States : new HashSet<LTSState>();
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
					state => state.GetOutTransitions(box.RegularFormula).All(tr => fe.Contains(tr.Right))
				));
			}

			else if (formula is Diamond) {
				var diamond = formula as Diamond;
				var shorthand = new Negation(
					// <a>f == [a](not f)
					new Box(diamond.RegularFormula, new Negation(diamond.Formula))
					);
				return Solve(shorthand, lts, env);
			}

			else if (formula is Mu) {
				var mu = formula as Mu;
				env[mu.Variable] = new HashSet<LTSState>();

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
				env[nu.Variable] = lts.States;

				return FixedPoint.GFP(delegate(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
					// repeats tau: X := solve(f)
					var X = tuple.Item1;
					X = Solve(nu.Formula, lts, env);
					env[nu.Variable] = X;

					return Tuple.Create(X, lts, env);
				}, lts, env);

			}
			
			throw new InvalidDataException("not a valid formula in our grammar");
		}


	}
}
