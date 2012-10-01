using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelChecker {
    class EmersonLei {
        Dictionary<Variable, HashSet<LTSState>> Approximations = new Dictionary<Variable, HashSet<LTSState>>();
        LTS LTS;

        public EmersonLei(LTS lts) {
            LTS = lts;
        }

        void Init(IEnumerable<Variable> variables) {
            foreach (var v in variables) {

            }
        }
        public HashSet<LTSState> Evaluate(MuFormula formula, bool init = true) {
            if (init) {
                var allVariables = new List<Variable>();
                if (formula is Variable) allVariables.Add((Variable)formula);
                allVariables.AddRange(formula.SubFormulas.Where(sf => sf is Variable).Cast<Variable>());
                Init(allVariables);
            }

            if (formula is Variable)
                return Approximations[formula as Variable];

            else if (formula is Disjunction)
                return new HashSet<LTSState>(
                    Evaluate(((Disjunction)formula).Left)
                 .Union(
                    Evaluate(((Disjunction)formula).Right)));

            else if (formula is Conjunction)
                return new HashSet<LTSState>(
                    Evaluate(((Conjunction)formula).Left)
                 .Intersect(
                    Evaluate(((Conjunction)formula).Right)));



            return null;
        }

    }
}
