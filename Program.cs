using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ModelChecker {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            var parser = new MuCalculusParser();
            parser.Setup();
            parser.Parse(new StreamReader(@"C:\Users\Frank\Google Drive\TUe\2IW55 - Algorithms for Model Checking\dining\invariantly_inevitably_eat.mcf"));

            Environment env = new Environment();
            env.LTS = LTS.Parse(@"C:\Users\Frank\Google Drive\tue\2IW55 - Algorithms for Model Checking\dining\dining_2.aut");

            foreach (MuFormula f in parser.formulas)
                OutputResult(f, f.Evaluate(env));

            Console.ReadKey();
        }

        private static void OutputResult(MuFormula f, HashSet<LTSState> hashSet) {
            Console.WriteLine("Formula {0} holds in: ", f);
            foreach (var state in hashSet)
                Console.WriteLine("\t - {0}", state.Name);
            if (hashSet.Count == 0)
                Console.WriteLine("\t no states");
        }
    }
}
