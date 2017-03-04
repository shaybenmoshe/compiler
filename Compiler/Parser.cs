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

        public ScopeStatement ParseAll()
        {
            ScopeStatement scope = new ScopeStatement();

            while (!this.tokenStream.Ended())
            {
                scope.Add(this.ParseNext());
            }

            return scope;
        }

        private Statement ParseNext()
        {
            Statement statement;

            if (this.tokenStream.PeekNextIsPunct(PunctToken.Puncts.LBraces))
            {
                return this.ParseScope();
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Var))
            {
                statement = this.ParseVar();
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Semicolon);
                return statement;
            }

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.If))
            {
                statement = this.ParseIfElse();
                return statement;
            }

            Expression expression = this.TryParseExpression();
            if (expression != null)
            {
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Semicolon);
                return new ExpressionStatement(expression);
            }

            throw new CompilerException("Couldn't parse anything.", this.tokenStream.TokenPosition);
        }

        private ScopeStatement ParseScope()
        {
            int startPosition = this.tokenStream.TokenPosition;

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LBraces);

            ScopeStatement scope = new ScopeStatement();
            while (!this.tokenStream.Ended())
            {
                if (this.tokenStream.PeekNextIsPunct(PunctToken.Puncts.RBraces))
                {
                    this.tokenStream.Next();
                    return scope;
                }
                scope.Add(this.ParseNext());
            }

            throw new CompilerException("Expected to find end of scope but ended", startPosition);
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
            Expression expr2 = this.ParseExpression();

            return new BinaryOpExpression(binaryOpToken, expr1, expr2);
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
            if (this.tokenStream.PeekNextIsPunct(PunctToken.Puncts.LParenthese))
            {
                this.tokenStream.Next();
                Expression expr = this.ParseExpression();
                this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);
                return new ParentheseExpression(expr);
            }

            if (this.PeekNextIsCall())
            {
                return this.ParseCall();
            }

            if (this.PeekNextIsImmediate())
            {
                return new ImmediateExpression(this.tokenStream.Next());
            }

            return null;
        }

        private CallExpression ParseCall()
        {
            int startPosition = this.tokenStream.TokenPosition;

            NameToken func = this.tokenStream.NextName();
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LParenthese);

            List<Expression> parameters = new List<Expression>();
            bool first = true;
            while (!this.tokenStream.Ended())
            {
                // If first, check for ).
                if (first)
                {
                    if (this.tokenStream.PeekNextIsType(Token.Types.Punct))
                    {
                        this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);
                        goto createCallStatement;
                    }
                }
                // If not first, ensure , or ).
                else
                {
                    int punctPosition = this.tokenStream.TokenPosition;
                    if (!this.tokenStream.PeekNextIsType(Token.Types.Punct))
                    {
                        throw new CompilerException("Expected punct", punctPosition);
                    }

                    PunctToken token = this.tokenStream.Next() as PunctToken;

                    if (token.Value == PunctToken.Puncts.RParenthese)
                    {
                        goto createCallStatement;
                    }

                    if (!first && token.Value != PunctToken.Puncts.Comma)
                    {
                        throw new CompilerException("Expected either , or (.", punctPosition);
                    }
                }

                parameters.Add(this.ParseExpression());
                first = false;
            }

            throw new CompilerException("Unfinished function call.", startPosition);

        createCallStatement:
            return new CallExpression(func, parameters);
        }

        private IfElseStatement ParseIfElse()
        {
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

            return new IfElseStatement(cond, body, elseBody);
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
            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Var);

            NameToken name = this.tokenStream.NextName();
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Colon);
            NameToken type = this.tokenStream.NextName();
            this.tokenStream.EnsureNextIsOp(BinaryOpToken.Ops.Ass);
            Expression value = this.ParseExpression();

            return new VarStatement(name, type, value);
        }
    }
}