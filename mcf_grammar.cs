﻿//Generated by the GOLD Parser Builder

using System.IO;
using System.Windows.Forms;

class MyParser
{
    private GOLD.Parser parser = new GOLD.Parser(); 

    private enum SymbolIndex
    {
        @Eof = 0,                                  // (EOF)
        @Error = 1,                                // (Error)
        @Whitespace = 2,                           // Whitespace
        @Ampamp = 3,                               // '&&'
        @Lparen = 4,                               // '('
        @Rparen = 5,                               // ')'
        @Dot = 6,                                  // '.'
        @Lbracket = 7,                             // '['
        @Rbracket = 8,                             // ']'
        @Pipepipe = 9,                             // '||'
        @Lt = 10,                                  // '<'
        @Gt = 11,                                  // '>'
        @Action = 12,                              // Action
        @Commentl = 13,                            // CommentL
        @Mu = 14,                                  // mu
        @Newline = 15,                             // NewLine
        @Not = 16,                                 // not
        @Nu = 17,                                  // nu
        @Proposition = 18,                         // Proposition
        @Variable = 19,                            // Variable
        @Line = 20,                                // <Line>
        @Lines = 21,                               // <Lines>
        @Muform = 22,                              // <muForm>
        @Nl = 23,                                  // <nl>
        @Nlopt = 24                                // <nl Opt>
    }

    private enum ProductionIndex
    {
        @Nl_Newline = 0,                           // <nl> ::= NewLine <nl>
        @Nl_Newline2 = 1,                          // <nl> ::= NewLine
        @Nlopt_Newline = 2,                        // <nl Opt> ::= NewLine <nl Opt>
        @Muform_Proposition = 3,                   // <muForm> ::= Proposition
        @Muform_Variable = 4,                      // <muForm> ::= Variable
        @Muform_Lparen_Pipepipe_Rparen = 5,        // <muForm> ::= '(' <muForm> '||' <muForm> ')'
        @Muform_Lparen_Ampamp_Rparen = 6,          // <muForm> ::= '(' <muForm> '&&' <muForm> ')'
        @Muform_Lt_Action_Gt = 7,                  // <muForm> ::= '<' Action '>' <muForm>
        @Muform_Lbracket_Action_Rbracket = 8,      // <muForm> ::= '[' Action ']' <muForm>
        @Muform_Mu_Variable_Dot = 9,               // <muForm> ::= mu Variable '.' <muForm>
        @Muform_Nu_Variable_Dot = 10,              // <muForm> ::= nu Variable '.' <muForm>
        @Muform_Not = 11,                          // <muForm> ::= not <muForm>
        @Line_Commentl = 12,                       // <Line> ::= CommentL <nl>
        @Line = 13,                                // <Line> ::= <muForm> <nl>
        @Line2 = 14,                               // <Line> ::= <nl>
        @Lines = 15,                               // <Lines> ::= <Line> <Lines>
        @Lines2 = 16                               // <Lines> ::= 
    }

    public object program;     //You might derive a specific object

    public void Setup()
    {
        //This procedure can be called to load the parse tables. The class can
        //read tables using a BinaryReader.
        
        parser.LoadTables(Path.Combine(Application.StartupPath, "grammar.egt"));
    }
    
    public bool Parse(TextReader reader)
    {
        //This procedure starts the GOLD Parser Engine and handles each of the
        //messages it returns. Each time a reduction is made, you can create new
        //custom object and reassign the .CurrentReduction property. Otherwise, 
        //the system will use the Reduction object that was returned.
        //
        //The resulting tree will be a pure representation of the language 
        //and will be ready to implement.

        GOLD.ParseMessage response; 
        bool done;                      //Controls when we leave the loop
        bool accepted = false;          //Was the parse successful?

        parser.Open(reader);
        parser.TrimReductions = false;  //Please read about this feature before enabling  

        done = false;
        while (!done)
        {
            response = parser.Parse();

            switch (response)
            {
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
    
    private object CreateNewObject(GOLD.Reduction r)
    { 
        object result = null;
        
        switch( (ProductionIndex) r.Parent.TableIndex)
        {
            case ProductionIndex.Nl_Newline:                 
                // <nl> ::= NewLine <nl>
                break;

            case ProductionIndex.Nl_Newline2:                 
                // <nl> ::= NewLine
                break;

            case ProductionIndex.Nlopt_Newline:                 
                // <nl Opt> ::= NewLine <nl Opt>
                break;

            case ProductionIndex.Muform_Proposition:                 
                // <muForm> ::= Proposition
                break;

            case ProductionIndex.Muform_Variable:                 
                // <muForm> ::= Variable
                break;

            case ProductionIndex.Muform_Lparen_Pipepipe_Rparen:                 
                // <muForm> ::= '(' <muForm> '||' <muForm> ')'
                break;

            case ProductionIndex.Muform_Lparen_Ampamp_Rparen:                 
                // <muForm> ::= '(' <muForm> '&&' <muForm> ')'
                break;

            case ProductionIndex.Muform_Lt_Action_Gt:                 
                // <muForm> ::= '<' Action '>' <muForm>
                break;

            case ProductionIndex.Muform_Lbracket_Action_Rbracket:                 
                // <muForm> ::= '[' Action ']' <muForm>
                break;

            case ProductionIndex.Muform_Mu_Variable_Dot:                 
                // <muForm> ::= mu Variable '.' <muForm>
                break;

            case ProductionIndex.Muform_Nu_Variable_Dot:                 
                // <muForm> ::= nu Variable '.' <muForm>
                break;

            case ProductionIndex.Muform_Not:                 
                // <muForm> ::= not <muForm>
                break;

            case ProductionIndex.Line_Commentl:                 
                // <Line> ::= CommentL <nl>
                break;

            case ProductionIndex.Line:                 
                // <Line> ::= <muForm> <nl>
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

        }  //switch

        return result;
    }
    
}; //MyParser
