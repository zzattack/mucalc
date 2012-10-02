using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelChecker {
    using PredicateTransformer =
        Func<Tuple<HashSet<LTSState>, LTS, Environment>,
			  Tuple<HashSet<LTSState>, LTS, Environment>>;

    static class FixedPoint {
		public static HashSet<LTSState> LFP(PredicateTransformer tau, LTS lts, Environment env) {
            var Q = Tuple.Create(new HashSet<LTSState>(), lts, env);
            var QPrime = tau(Q);

            while (QPrime.Item1.Count() != Q.Item1.Count()) {
                Q = QPrime;
                QPrime = tau(QPrime);
            }

            return Q.Item1;
        }

        public static HashSet<LTSState> GFP(PredicateTransformer tau, LTS lts, Environment env) {
            var Q = Tuple.Create(new HashSet<LTSState>(lts.States), lts, env);
            var QPrime = tau(Q);

            while (QPrime.Item1.Count() != Q.Item1.Count()) {
                Q = QPrime;
                QPrime = tau(QPrime);
            }

            return Q.Item1;
        }
    }
}
