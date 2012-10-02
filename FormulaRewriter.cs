using System;
using System.Diagnostics;
using System.Globalization;

namespace ModelChecker {
	class FormulaRewriter {

		internal static MuFormula Rewrite(MuFormula form) {
			form = RewriteBoxDia(form);
			form = RewriteRFs(form);
			return form;
		}

		static MuFormula RewriteBoxDia(MuFormula form) {
			// We extended our grammar from the assignment by allowing regular formulas instead of single-letter actions.
			// Here, we rewrite such extended formulas to their more primitive versions for which we impemented evaluators.

			if (form is Diamond) {
				var dia = form as Diamond;
				if (dia.RegularFormula.Multiplier == "+") {
					// <R+>f = <R><R*>f
					var rf2 = dia.RegularFormula.Clone();
					rf2.Multiplier = "*";
					dia.RegularFormula.Multiplier = "";
					dia.Formula = new Diamond(dia.RegularFormula, new Diamond(rf2, dia.Formula));
				}
				else if (dia.RegularFormula.Multiplier == "*") {
					// <R*>f = mu X . (<R> X || f)
					var var = new Variable(GetFreeVar());
					dia.RegularFormula.Multiplier = "";
					return Rewrite(new Mu(var, new Disjunction(new Diamond(dia.RegularFormula, var), dia.Formula)));
				}

			}

			else if (form is Box) {
				var box = form as Box;
				if (box.RegularFormula.Multiplier == "+") {
					// [R+]f = [R][R*]f
					var rf2 = box.RegularFormula.Clone();
					rf2.Multiplier = "*";
					box.RegularFormula.Multiplier = "";
					return new Box(box.RegularFormula, new Box(rf2, box.Formula));
				}
				else if (box.RegularFormula.Multiplier == "*") {
					// [R*]f = nu X . ([R] X && f)
					var var = new Variable(GetFreeVar());
					box.RegularFormula.Multiplier = "";
					return Rewrite(new Nu(var, new Disjunction(new Box(box.RegularFormula, var), box.Formula)));
				}
				box.Formula = Rewrite(box.Formula);
			}

			// we also need to perform this on all subformulas
			else if (form is Disjunction) form = new Disjunction(Rewrite(((Disjunction)form).Left), Rewrite(((Disjunction)form).Right));
			else if (form is Conjunction) form = new Conjunction(Rewrite(((Conjunction)form).Left), Rewrite(((Conjunction)form).Right));
			else if (form is Mu) form = new Mu(((Mu)form).Variable, Rewrite(((Mu)form).Formula));
			else if (form is Nu) form = new Nu(((Nu)form).Variable, Rewrite(((Nu)form).Formula));

			return form;
		}

		// rewrite regular formulas inside box/diamond to multiple boxes/diamonds
		static MuFormula RewriteRFs(MuFormula form) {
			// connectives on regular formulas
			// <a.rf>f = <a><rf>f

			if (form is Box) {
				var box = form as Box;

				// [rf1+rf2]f = [rf1]f || [rf2]f
				if (box.RegularFormula is PlusFormula) {
					var un = box.RegularFormula as PlusFormula;
					return Rewrite(new Disjunction(new Box(un.Left, box.Formula), new Box(un.Right, box.Formula)));
				}

				// [a.rf]f = [a][rf]f
				else if (box.RegularFormula is SequenceFormula) {
					var seq = box.RegularFormula as SequenceFormula;
					return Rewrite(new Box(seq.First, new Box(seq.Sequence, RewriteRFs(box.Formula))));
				}

				// by now all *s should be gone, so we can rewrite
				// [(a.b)] to [a.b]
				else if (box.RegularFormula is NestedFormula) {
					var nested = box.RegularFormula as NestedFormula;
					if (nested.Multiplier != "")
						throw new InvalidProgramException("by now all multipliers should have been removed");
					box.RegularFormula = nested.Inner;
					return Rewrite(box);
				}
			}

			else if (form is Diamond) {
				var dia = form as Diamond;
				// <rf1+rf2>f = <[rf1>f || <rf2>f 
				if (dia.RegularFormula is PlusFormula) {
					var un = dia.RegularFormula as PlusFormula;
					return Rewrite(new Disjunction(new Diamond(un.Left, dia.Formula), new Diamond(un.Right, dia.Formula)));
				}

				// <a.rf>f = <a><rf>f
				else if (dia.RegularFormula is SequenceFormula) {
					var seq = dia.RegularFormula as SequenceFormula;
					dia.RegularFormula = seq.First;
					dia.Formula = new Diamond(seq.Sequence, dia.Formula);
					return Rewrite(dia);
				}

				else if (dia.RegularFormula is NestedFormula) {
					var nested = dia.RegularFormula as NestedFormula;
					if (nested.Multiplier != "")
						throw new InvalidProgramException("by now all multipliers should have been removed");
					dia.RegularFormula = nested.Inner;
					return Rewrite(dia);
				}
			}

			// we also need to perform this on all subformulas
			else if (form is Disjunction) form = new Disjunction(Rewrite(((Disjunction)form).Left), Rewrite(((Disjunction)form).Right));
			else if (form is Conjunction) form = new Conjunction(Rewrite(((Conjunction)form).Left), Rewrite(((Conjunction)form).Right));
			else if (form is Mu) form = new Mu(((Mu)form).Variable, Rewrite(((Mu)form).Formula));
			else if (form is Nu) form = new Nu(((Nu)form).Variable, Rewrite(((Nu)form).Formula));

			return form;
		}

		static int _firstFree;
		public static string GetFreeVar() {
			return string.Format("@{0}@", (char)('A' + _firstFree++));
		}
		internal static void ResetFreeVars() {
			_firstFree = 0;
		}

	}
}
