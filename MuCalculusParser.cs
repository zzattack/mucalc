using System;
using System.Collections.Generic;
using System.IO;
using GOLD;

namespace ModelChecker {
	internal class MuCalculusParser {
		public List<MuFormula> formulas = new List<MuFormula>();
		private readonly Parser parser = new Parser();

		private enum ProductionIndex {
			@Nl_Newline = 0,                           // <nl> ::= NewLine <nl>
			@Nl_Newline2 = 1,                          // <nl> ::= NewLine
			@Multiplier_Plus = 2,                      // <Multiplier> ::= '+'
			@Multiplier_Times = 3,                     // <Multiplier> ::= '*'
			@Multiplier = 4,                           // <Multiplier> ::= 
			@Regform_Plus = 5,                         // <regForm> ::= <regForm> '+' <regFormSeq>
			@Regform = 6,                              // <regForm> ::= <regFormSeq>
			@Regformseq_Dot = 7,                       // <regFormSeq> ::= <regFormSeq> '.' <regForm>
			@Regformseq = 8,                           // <regFormSeq> ::= <regFormValue>
			@Regformvalue_Identifier = 9,              // <regFormValue> ::= Identifier <Multiplier>
			@Regformvalue_Lparen_Rparen = 10,          // <regFormValue> ::= '(' <regForm> ')' <Multiplier>
			@Muform_Identifier = 11,                   // <muForm> ::= Identifier
			@Muform_Variable = 12,                     // <muForm> ::= Variable
			@Muform_Lparen_Pipepipe_Rparen = 13,       // <muForm> ::= '(' <muForm> '||' <muForm> ')'
			@Muform_Lparen_Ampamp_Rparen = 14,         // <muForm> ::= '(' <muForm> '&&' <muForm> ')'
			@Muform_Lt_Gt = 15,                        // <muForm> ::= '<' <regForm> '>' <muForm>
			@Muform_Lbracket_Rbracket = 16,            // <muForm> ::= '[' <regForm> ']' <muForm>
			@Muform_Mu_Variable_Dot = 17,              // <muForm> ::= mu Variable '.' <muForm>
			@Muform_Nu_Variable_Dot = 18,              // <muForm> ::= nu Variable '.' <muForm>
			@Muform_Not = 19,                          // <muForm> ::= not <muForm>
			@Line_Commentl = 20,                       // <Line> ::= CommentL <nl>
			@Line = 21,                                // <Line> ::= <muForm> <nl>
			@Line2 = 22,                               // <Line> ::= <nl>
			@Lines = 23,                               // <Lines> ::= <Line> <Lines>
			@Lines2 = 24                               // <Lines> ::= 
		}

		public void Setup() {
			// This procedure can be called to load the parse tables. The class can
			// read tables using a BinaryReader.
			parser.LoadTables("mcf_grammar.egt");
		}

		public bool Parse(TextReader reader) {
			//This procedure starts the GOLD Parser Engine and handles each of the
			//messages it returns. Each time a reduction is made, you can create new
			//custom object and reassign the .CurrentReduction property. Otherwise, 
			//the system will use the Reduction object that was returned.
			//
			//The resulting tree will be a pure representation of the language 
			//and will be ready to implement.

			GOLD.ParseMessage response;
			bool done; //Controls when we leave the loop
			bool accepted = false; //Was the parse successful?

			parser.Open(reader);
			parser.TrimReductions = false; //Please read about this feature before enabling  


			done = false;
			while (!done) {
				response = parser.Parse();

				switch (response) {
					case GOLD.ParseMessage.LexicalError:
						//Cannot recognize token
						done = true;
						break;

					case GOLD.ParseMessage.SyntaxError:
						//Expecting a different token
						done = true;
						break;

					case GOLD.ParseMessage.Reduction:
						//Create a customized object to store the reduction

						parser.CurrentReduction = CreateNewObject(parser.CurrentReduction as GOLD.Reduction);
						break;

					case GOLD.ParseMessage.Accept:
						//Accepted!
						//program = parser.CurrentReduction   //The root node!                 
						done = true;
						accepted = true;
						break;

					case GOLD.ParseMessage.TokenRead:
						//You don't have to do anything here.
						break;

					case GOLD.ParseMessage.InternalError:
						//INTERNAL ERROR! Something is horribly wrong.
						done = true;
						break;

					case GOLD.ParseMessage.NotLoadedError:
						//This error occurs if the CGT was not loaded.                   
						done = true;
						break;

					case GOLD.ParseMessage.GroupError:
						//GROUP ERROR! Unexpected end of file
						done = true;
						break;
				}
			} //while

			return accepted;
		}

		private object CreateNewObject(GOLD.Reduction r) {
			object result = null;

			switch ((ProductionIndex)r.Parent.TableIndex()) {
				case ProductionIndex.Nl_Newline:
					// <nl> ::= NewLine <nl>
					break;

				case ProductionIndex.Nl_Newline2:
					// <nl> ::= NewLine
					break;

				case ProductionIndex.Regform_Plus:
					// <regForm> ::= <regForm> '+' <regFormSeq>
					result = new PlusFormula((RegularFormula)r[0].Data, (RegularFormula)r[2].Data, (string)r[3].Data);
					break;

				case ProductionIndex.Regform:
					// <regForm> ::= <regFormSeq>
					result = r[0].Data;
					break;

				case ProductionIndex.Regformseq_Dot:
					// <regFormSeq> ::= <regFormSeq> '.' <regFormValue>
					result = new SequenceFormula((RegularFormula)r[0].Data, (RegularFormula)r[2].Data);
					break;

				case ProductionIndex.Regformseq:
					// <regFormSeq> ::= <regFormValue>
					result = r[0].Data;
					break;

				case ProductionIndex.Regformvalue_Identifier:
					// <regFormValue> ::= Identifier <Multiplier>
					result = new SingleAction((string)r[0].Data, (string)r[1].Data);
					break;

				case ProductionIndex.Regformvalue_Lparen_Rparen:
					// <regFormValue> ::= '(' <regForm> ')' <Multiplier>
					if ((string)r[3].Data != "")
						return new NestedFormula((RegularFormula)r[1].Data, (string)r[3].Data);
					else // small optimization
						result = r[1].Data;
					break;

				case ProductionIndex.Multiplier_Plus:
					// <Multiplier> ::= '+'
					result = "+";
					break;

				case ProductionIndex.Multiplier_Times:
					// <Multiplier> ::= '*'
					result = "*";
					break;

				case ProductionIndex.Multiplier:
					// <Multiplier> ::= 
					result = "";
					break;

				case ProductionIndex.Muform_Identifier:
					// <muForm> ::= Identifier
					result = new Proposition((string)r[0].Data);
					break;

				case ProductionIndex.Muform_Variable:
					// <muForm> ::= Variable
					result = new Variable((string)r[0].Data);
					break;

				case ProductionIndex.Muform_Lparen_Pipepipe_Rparen:
					// <muForm> ::= '(' <muForm> '||' <muForm> ')'
					result = new Disjunction((MuFormula)r[1].Data, (MuFormula)r[3].Data);
					break;

				case ProductionIndex.Muform_Lparen_Ampamp_Rparen:
					// <muForm> ::= '(' <muForm> '&&' <muForm> ')'
					result = new Conjunction((MuFormula)r[1].Data, (MuFormula)r[3].Data);
					break;

				case ProductionIndex.Muform_Lt_Gt:
					// <muForm> ::= '<' <regForm> '>' <muForm>
					result = new Diamond((RegularFormula)r[1].Data, (MuFormula)r[3].Data);
					break;

				case ProductionIndex.Muform_Lbracket_Rbracket:
					// <muForm> ::= '[' <regForm> ']' <muForm>
					result = new Box((RegularFormula)r[1].Data, (MuFormula)r[3].Data);
					break;

				case ProductionIndex.Muform_Mu_Variable_Dot:
					// <muForm> ::= mu Variable '.' <muForm>
					result = new Mu(new Variable((string)r[1].Data), (MuFormula)r[3].Data);
					break;

				case ProductionIndex.Muform_Nu_Variable_Dot:
					// <muForm> ::= nu Variable '.' <muForm>
					result = new Nu(new Variable((string)r[1].Data), (MuFormula)r[3].Data);
					break;

				case ProductionIndex.Muform_Not:
					// <muForm> ::= not <muForm>
					result = new Negation((MuFormula)r[1].Data);
					break;
					
				case ProductionIndex.Line_Commentl:
					// <Line> ::= CommentL <nl>
					break;

				case ProductionIndex.Line:
					// <Line> ::= <muForm> <nl>
					var form = (MuFormula)r[0].Data;
					FormulaRewriter.ResetFreeVars();
					form = FormulaRewriter.Rewrite(form);

					form.SetParents(null /* root has no parent */);
					formulas.Add(form);
					break;

				case ProductionIndex.Line2:
					// <Line> ::= <nl>
					break;

				case ProductionIndex.Lines:
					// <Lines> ::= <Line> <Lines>
					break;

				case ProductionIndex.Lines2:
					// <Lines> ::= 
					break;

			} //switch

			return result;
		}

	}
}