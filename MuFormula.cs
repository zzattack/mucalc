using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelChecker {
    using PredicateTransformer =
        Func<Tuple<HashSet<LTSState>, Environment>,
        Tuple<HashSet<LTSState>>, Environment>;

    public abstract class MuFormula {
        public abstract HashSet<LTSState> Evaluate(Environment env);
    }

    public class Proposition : MuFormula {
        public string Value;
        public Proposition(string value) {
            Value = value;
        }
        public override HashSet<LTSState> Evaluate(Environment env) {
            return bool.Parse(Value) ? new HashSet<LTSState>(env.LTS.States) : new HashSet<LTSState>();
        }
        public override string ToString() {
            return Value;
        }
    }

    public class Variable : MuFormula {
        public string Name;
        public Variable(string name) {
            Name = name;
        }

        public override HashSet<LTSState> Evaluate(Environment env) {
            return env.GetVariable(Name);
        }

        public override string ToString() {
            return Name;
        }
    }

    public class Negation : MuFormula {
        public MuFormula Formula;
        public Negation(MuFormula formula) {
            Formula = formula;
        }

        public override HashSet<LTSState> Evaluate(Environment env) {
            return new HashSet<LTSState>(env.LTS.States.Except(Formula.Evaluate(env)));
        }

        public override string ToString() {
            return "not(" + Formula.ToString() + ")";
        }
    }

    public class Conjunction : MuFormula {
        public MuFormula Left, Right;
        public Conjunction(MuFormula left, MuFormula right) {
            Left = left;
            Right = right;
        }

        public override HashSet<LTSState> Evaluate(Environment env) {
            var leftStates = Left.Evaluate(env);
            var rightStates = Right.Evaluate(env);
            return new HashSet<LTSState>(leftStates.Intersect(rightStates));
        }

        public override string ToString() {
            return "(" + Left + " && " + Right + ")";
        }
    }

    public class Disjunction : MuFormula {
        public MuFormula Left, Right;
        public Disjunction(MuFormula left, MuFormula right) {
            Left = left;
            Right = right;
        }

        public override HashSet<LTSState> Evaluate(Environment env) {
            var leftStates = Left.Evaluate(env);
            var rightStates = Right.Evaluate(env);
            return new HashSet<LTSState>(leftStates.Union(rightStates));
        }
        public override string ToString() {
            return "(" + Left + " || " + Right + ")";
        }
    }

    public class Box : MuFormula {
        public string Action;
        public MuFormula Formula;
        public Box(string action, MuFormula formula) {
            Action = action;
            Formula = formula;
        }
        public override HashSet<LTSState> Evaluate(Environment env) {
            // box a f = { s | forall t. s -a-> t ==> t in [[f]]e }
            // i.e. the set of states for which all a-transitions go to a state in which f holds
            var fe = Formula.Evaluate(env);
            
            return new HashSet<LTSState>(env.LTS.States.Where(
                // states where, for all outtransitions with action a, the Formula holds in the direct successor 
                state => state.OutTransitions.All(tr => tr.Action != Action || fe.Contains(tr.Right))
            ));
        }
        public override string ToString() {
            return "[" + Action + "]" + Formula;
        }
    }

    public class Diamond : MuFormula {
        public string Action;
        public MuFormula Formula;
        public Diamond(string action, MuFormula formula) {
            Action = action;
            Formula = formula;
        }
        public override HashSet<LTSState> Evaluate(Environment env) {
            // <a>f := not([a](not f))
            // not
            var shorthand = new Negation(
                //[a](not f)
                new Box(Action, new Negation(Formula))
            );
            return shorthand.Evaluate(env);
        }
        public override string ToString() {
            return "<" + Action + ">" + Formula;
        }
    }

    public class Mu : MuFormula {
        public MuFormula variable;
        public MuFormula predicate;

        public Mu(MuFormula var, MuFormula pred) {
            variable = var;
            predicate = pred;
        }

        public override HashSet<LTSState> Evaluate(Environment env) {
            return FixedPoint.LFP(Tau, env);
        }

        private Tuple<HashSet<LTSState>, Environment> Tau(Tuple<HashSet<LTSState>, Environment> tuple) {
            var X = tuple.Item1;
            var env = tuple.Item2;

            X = predicate.Evaluate(env);
            env.Replace(variable, X);

            return Tuple.Create(X, env);
        }
        public override string ToString() {
            return "mu" + variable + "." + predicate;
        }
    }


    public class Nu : MuFormula {
        public MuFormula variable;
        public MuFormula predicate;

        public Nu(MuFormula var, MuFormula pred) {
            variable = var;
            predicate = pred;
        }

        public override HashSet<LTSState> Evaluate(Environment env) {
            return FixedPoint.GFP(Tau, env);
        }

        private Tuple<HashSet<LTSState>, Environment> Tau(Tuple<HashSet<LTSState>, Environment> tuple) {
            var X = tuple.Item1;
            var env = tuple.Item2;

            X = predicate.Evaluate(env);
            env.Replace(variable, X);

            return Tuple.Create(X, env);
        }

        public override string ToString() {
            return "nu" + variable + "." + predicate;
        }
    }
}
