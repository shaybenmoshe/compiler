using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class Parser
    {
        private TokenStream tokenStream;

        public Parser(TokenStream tokenStream)
        {
            this.tokenStream = tokenStream;
        }

        public AST ParseAll()
        {
            CompoundStatement compound = new CompoundStatement(0);

            while (!this.tokenStream.Ended())
            {
                compound.Add(this.ParseNext());
            }

            return new AST(compound);
        }

        private Statement ParseNext()
        {
            Statement statement;

            if (this.tokenStream.PeekNextIsPunct(PunctToken.Puncts.LBraces))
            {
                return this.ParseCompound();
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Var))
            {
                statement = this.ParseVar();
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Semicolon);
                return statement;
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Struct))
            {
                statement = this.ParseStruct();
                return statement;
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Return))
            {
                statement = this.ParseReturn();
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Semicolon);
                return statement;
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.If))
            {
                return this.ParseIfElse();
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.While))
            {
                return this.ParseWhile();
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Function))
            {
                return this.ParseFunction();
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Int3))
            {
                statement = this.ParseInt3();
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Semicolon);
                return statement;
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Import))
            {
                statement = this.ParseImport();
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Semicolon);
                return statement;
            }

            int startPosition = this.tokenStream.TokenPosition;
            Expression expression = this.TryParseExpression();
            if (expression != null)
            {
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Semicolon);
                return new ExpressionStatement(startPosition, expression);
            }

            throw new CompilerException("Couldn't parse anything.", this.tokenStream.TokenPosition);
        }

        private CompoundStatement ParseCompound()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LBraces);

            CompoundStatement compound = new CompoundStatement(startPosition);
            while (!this.tokenStream.Ended())
            {
                if (this.tokenStream.PeekNextIsPunct(PunctToken.Puncts.RBraces))
                {
                    this.tokenStream.Next();
                    return compound;
                }
                compound.Add(this.ParseNext());
            }

            throw new CompilerException("Expected to find end of compound but ended", startPosition);
        }

        private Expression ParseExpression()
        {
            Expression expression = this.TryParseExpression();
            if (expression == null)
            {
                throw new CompilerException("Expected expression.", this.tokenStream.TokenPosition);
            }
            return expression;
        }

        private Expression TryParseExpression()
        {
            int startPosition = this.tokenStream.TokenPosition;

            Expression expr1 = this.TryParseAtomicExpression();

            if (expr1 == null)
            {
                return null;
            }

            if (!this.tokenStream.PeekNextIsType(Token.Types.BinaryOp))
            {
                return expr1;
            }

            BinaryOpToken binaryOpToken = this.tokenStream.Next() as BinaryOpToken;

            if (binaryOpToken.Value == BinaryOpToken.Ops.Dot)
            {
                NameToken member = this.tokenStream.NextName();
                return new MemberAccessExpression(startPosition, expr1, member);
            }

            Expression expr2 = this.ParseExpression();
            return new BinaryOpExpression(startPosition, binaryOpToken, expr1, expr2);
        }

        private Expression ParseAtomicExpression()
        {
            Expression expression = this.TryParseAtomicExpression();
            if (expression == null)
            {
                throw new CompilerException("Expected call or immediate.", this.tokenStream.TokenPosition);
            }
            return expression;
        }

        private Expression TryParseAtomicExpression()
        {
            int startPosition = this.tokenStream.TokenPosition;

            if (this.tokenStream.PeekNextIsPunct(PunctToken.Puncts.LParenthese))
            {
                this.tokenStream.Next();
                Expression expr = this.ParseExpression();
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);
                return new ParentheseExpression(startPosition, expr);
            }

            if (this.PeekNextIsCall())
            {
                return this.ParseCall();
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Sizeof))
            {
                this.tokenStream.Next();
                return new SizeofExpression(startPosition, this.tokenStream.NextName());
            }

            // @todo
            /*if (this.tokenStream.PeekNextIsType(Token.Types.String))
            {
                return new ImmediateExpression(startPosition, this.tokenStream.Next());
            }*/
            if (this.tokenStream.PeekNextIsType(Token.Types.Number))
            {
                return new ImmediateExpression(startPosition, this.tokenStream.Next());
            }
            if (this.tokenStream.PeekNextIsType(Token.Types.Name))
            {
                return new NameExpression(startPosition, this.tokenStream.Next() as NameToken);
            }

            return null;
        }

        private CallExpression ParseCall()
        {
            int startPosition = this.tokenStream.TokenPosition;

            NameToken name = this.tokenStream.NextName();

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LParenthese);
            List<Expression> parameters = this.ParseParantheseCommaList<Expression>(this.tokenStream.TokenPosition, this.ParseExpression, PunctToken.Puncts.RParenthese);
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);

            return new CallExpression(startPosition, name, parameters);
        }

        private IfElseStatement ParseIfElse()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.If);
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LParenthese);

            Expression cond = this.ParseExpression();

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);

            Statement body = this.ParseNext();
            Statement elseBody = null;

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Else))
            {
                this.tokenStream.Next();
                elseBody = this.ParseNext();
            }

            return new IfElseStatement(startPosition, cond, body, elseBody);
        }

        private WhileStatement ParseWhile()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.While);
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LParenthese);

            Expression cond = this.ParseExpression();

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);

            Statement body = this.ParseNext();

            return new WhileStatement(startPosition, cond, body);
        }

        private bool PeekNextIsCall()
        {
            return this.tokenStream.PeekNextIsType(Token.Types.Name)
                && !this.tokenStream.Ended(1)
                && this.tokenStream.PeekNext(1).Type == Token.Types.Punct
                && (this.tokenStream.PeekNext(1) as PunctToken).Value == PunctToken.Puncts.LParenthese;
        }

        private bool PeekNextIsImmediate()
        {
            return this.tokenStream.PeekNextIsType(Token.Types.String)
                || this.tokenStream.PeekNextIsType(Token.Types.Name)
                || this.tokenStream.PeekNextIsType(Token.Types.Number);
        }

        private VarStatement ParseVar()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Var);

            NameDefStatement nameDef = this.ParseNameDef();

            return new VarStatement(startPosition, nameDef);
        }

        private StructStatement ParseStruct()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Struct);

            NameToken name = this.tokenStream.NextName();

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LBraces);
            List<NameDefStatement> members = this.ParseParantheseCommaList<NameDefStatement>(this.tokenStream.TokenPosition, this.ParseNameDef, PunctToken.Puncts.RBraces);
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RBraces);

            return new StructStatement(startPosition, name, members);
        }

        private ReturnStatement ParseReturn()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Return);
            Expression value = this.ParseExpression();
            return new ReturnStatement(startPosition, value);
        }

        private Int3Statement ParseInt3()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Int3);
            return new Int3Statement(startPosition);
        }

        private ImportStatement ParseImport()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Import);

            NameToken dll = this.tokenStream.NextName();
            NameToken function = this.tokenStream.NextName();
            NameToken importedName = this.tokenStream.NextName();

            return new ImportStatement(startPosition, dll, function, importedName);
        }

        private FunctionStatement ParseFunction()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Function);

            NameToken name = this.tokenStream.NextName();
            
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LParenthese);
            List<NameDefStatement> arguments = this.ParseParantheseCommaList<NameDefStatement>(this.tokenStream.TokenPosition, this.ParseNameDef, PunctToken.Puncts.RParenthese);
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Colon);
            NameToken retType = this.tokenStream.NextName();

            Statement body = this.ParseNext();

            return new FunctionStatement(startPosition, name, retType, arguments, body);
        }

        private NameDefStatement ParseNameDef()
        {
            int startPosition = this.tokenStream.TokenPosition;

            NameToken name = this.tokenStream.NextName();
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Colon);
            NameToken type = this.tokenStream.NextName();

            return new NameDefStatement(startPosition, name, type);
        }

        private List<T> ParseParantheseCommaList<T>(int startPosition, Func<T> cb, PunctToken.Puncts endingPunct)
        {
            List<T> list = new List<T>();
            bool first = true;
            while (!this.tokenStream.Ended())
            {
                if (this.tokenStream.PeekNextIsPunct(endingPunct))
                {
                    return list;
                }
                
                if (!first)
                {
                    this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Comma);
                }
                first = false;

                // We allow extra comma.
                if (this.tokenStream.PeekNextIsPunct(endingPunct))
                {
                    return list;
                }

                list.Add(cb());
            }

            throw new CompilerException("Unfinished function call.", startPosition);
        }
    }
}