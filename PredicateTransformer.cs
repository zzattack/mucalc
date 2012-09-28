using System;
using System.Collections.Generic;
using System.Linq;
using ModelChecker;
using Environment = ModelChecker.Environment;

namespace ModelChecker {
    using PredicateTransformer =
        Func<Tuple<HashSet<LTSState>, Environment>,
              Tuple<HashSet<LTSState>, Environment>>;

    public static class Transformers {
        static Transformers() { }

        public static PredicateTransformer PreR = delegate(Tuple<HashSet<LTSState>, Environment> tuple) {  
            var X = tuple.Item1;
            var env = tuple.Item2;
            return Tuple.Create(
                new HashSet<LTSState>(env.LTS.Transitions.Where(tr => X.Contains(tr.Right)).Select(tr => tr.Left))
                , env);
        };


        public static PredicateTransformer PostR = delegate(Tuple<HashSet<LTSState>, Environment> tuple) { 
            var X = tuple.Item1;
            var env = tuple.Item2;
            return Tuple.Create(
                new HashSet<LTSState>(env.LTS.Transitions.Where(tr => X.Contains(tr.Left)).Select(tr => tr.Right))
                , env);
        };
    }

}