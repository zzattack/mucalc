using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelChecker {
    using PredicateTransformer =
        Func<Tuple<HashSet<LTSState>, Environment>,
              Tuple<HashSet<LTSState>, Environment>>;

    public static class FixedPoint {
        public static HashSet<LTSState> LFP(PredicateTransformer tau, Environment env) {
            var Q = Tuple.Create(new HashSet<LTSState>(), env);
            Tuple<HashSet<LTSState>, Environment> QPrime = tau(Q);

            while (QPrime.Item1.Count() != Q.Item1.Count()) {
                Q = QPrime;
                QPrime = tau(QPrime);
            }

            return Q.Item1;
        }

        public static HashSet<LTSState> GFP(PredicateTransformer tau, Environment env) {
            var Q = Tuple.Create(env.LTS.States, env);
            Tuple<HashSet<LTSState>, Environment> QPrime = tau(Q);

            while (QPrime.Item1.Count() != Q.Item1.Count()) {
                Q = QPrime;
                QPrime = tau(QPrime);
            }

            return Q.Item1;
        }

        internal static HashSet<LTSState> GFP(Func<Tuple<HashSet<LTSState>, Environment>, Tuple<HashSet<LTSState>>, Environment> tau, Environment env) {
            throw new NotImplementedException();
        }
    }
}
