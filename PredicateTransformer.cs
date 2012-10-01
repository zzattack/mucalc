using System;
using System.Collections.Generic;
using System.Linq;
using ModelChecker;
using Environment = ModelChecker.Environment;

namespace ModelChecker {
    using PredicateTransformer =
        Func<Tuple<HashSet<LTSState>, LTS, Environment>,
			  Tuple<HashSet<LTSState>, LTS, Environment>>;

    public static class Transformers {
        static Transformers() { }

		public static PredicateTransformer PreR = delegate(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
			var X = tuple.Item1;
			var lts = tuple.Item2;
            var env = tuple.Item3;
            return Tuple.Create(
                new HashSet<LTSState>(lts.Transitions.Where(tr => X.Contains(tr.Right)).Select(tr => tr.Left))
                , lts, env);
        };


        public static PredicateTransformer PostR = delegate(Tuple<HashSet<LTSState>, LTS, Environment> tuple) {
			var X = tuple.Item1;
			var lts = tuple.Item2;
			var env = tuple.Item3;
            return Tuple.Create(
                new HashSet<LTSState>(lts.Transitions.Where(tr => X.Contains(tr.Left)).Select(tr => tr.Right))
                , lts, env);
        };
    }

}