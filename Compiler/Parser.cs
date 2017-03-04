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

            if (this.tokenStream.PeekNextIsKeyword(KeywordToken.Keywords.Function))
            {
                return this.ParseFunction();
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
            NameToken func = this.tokenStream.NextName();

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LParenthese);
            List<Expression> parameters = this.ParseParantheseCommaList<Expression>(this.tokenStream.TokenPosition, this.ParseExpression);
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);

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

            NameDefStatement nameDef = this.ParseNameDef();
            this.tokenStream.EnsureNextIsOp(BinaryOpToken.Ops.Ass);
            Expression value = this.ParseExpression();

            return new VarStatement(nameDef, value);
        }

        private ReturnStatement ParseReturn()
        {
            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Return);
            Expression value = this.ParseExpression();
            return new ReturnStatement(value);
        }

        private FunctionStatement ParseFunction()
        {
            this.tokenStream.EnsureNextIsKeyword(KeywordToken.Keywords.Function);

            NameToken name = this.tokenStream.NextName();
            
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.LParenthese);
            List<NameDefStatement> arguments = this.ParseParantheseCommaList<NameDefStatement>(this.tokenStream.TokenPosition, this.ParseNameDef);
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.RParenthese);

            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Colon);
            NameToken retType = this.tokenStream.NextName();

            Statement body = this.ParseNext();

            return new FunctionStatement(name, retType, arguments, body);
        }

        private NameDefStatement ParseNameDef()
        {
            NameToken name = this.tokenStream.NextName();
            this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Colon);
            NameToken type = this.tokenStream.NextName();

            return new NameDefStatement(name, type);
        }

        private List<T> ParseParantheseCommaList<T>(int startPosition, Func<T> cb)
        {
            List<T> list = new List<T>();
            bool first = true;
            while (!this.tokenStream.Ended())
            {
                if (this.tokenStream.PeekNextIsPunct(PunctToken.Puncts.RParenthese))
                {
                    return list;
                }
                
                if (!first)
                {
                    this.tokenStream.EnsureNextIsPunct(PunctToken.Puncts.Comma);
                }
                first = false;

                list.Add(cb());
            }

            throw new CompilerException("Unfinished function call.", startPosition);
        }
    }
}