using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JaeminPark.DialogManagement
{
    public class DialogVarTypeException : Exception { }
    public class DialogExprParseException : Exception { }

    public class DialogVar
    {
        private enum Type { Nil, Number, Boolean, String };
        private Type type;

        private float numberValue;
        private bool booleanValue;
        private string stringValue;

        public bool IsNumber { get { return type == Type.Number; } }
        public float NumberValue
        {
            get
            {
                if (type == Type.Number)
                    return numberValue;
                throw new DialogVarTypeException();
            }
        }
        public static implicit operator float(DialogVar v) { return v.numberValue; }
        public static implicit operator DialogVar(float v) { return new DialogVar(v); }

        public bool IsBoolean { get { return type == Type.Boolean; } }
        public bool BooleanValue
        {
            get
            {
                if (type == Type.Boolean)
                    return booleanValue;
                throw new DialogVarTypeException();
            }
        }
        public static implicit operator bool(DialogVar v) { return v.BooleanValue; }
        public static implicit operator DialogVar(bool v) { return new DialogVar(v); }

        public bool IsString { get { return type == Type.String; } }
        public string StringValue
        {
            get
            {
                if (type == Type.String)
                    return stringValue;
                throw new DialogVarTypeException();
            }
        }
        public static implicit operator string(DialogVar v) { return v.StringValue; }
        public static implicit operator DialogVar(string v) { return new DialogVar(v); }

        public DialogVar()
        {
            type = Type.Nil;
        }

        public DialogVar(float value)
        {
            type = Type.Number;
            numberValue = value;
        }

        public DialogVar(bool value)
        {
            type = Type.Boolean;
            booleanValue = value;
        }

        public DialogVar(string value)
        {
            type = Type.String;
            stringValue = value;
        }

        public static readonly DialogVar Nil = new DialogVar();

        public static DialogVar operator +(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue + right.numberValue);
            else if (left.type == Type.String && right.type == Type.String)
                return new DialogVar(left.stringValue + right.stringValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator -(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue - right.numberValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator *(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue * right.numberValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator /(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue / right.numberValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator %(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue % right.numberValue);
            else
                return new DialogVar();
        }
        
        public static DialogVar operator ==(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue == right.numberValue);
            else if (left.type == Type.Boolean && right.type == Type.Boolean)
                return new DialogVar(left.booleanValue == right.booleanValue);
            else if (left.type == Type.String && right.type == Type.String)
                return new DialogVar(left.stringValue == right.stringValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator !=(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue != right.numberValue);
            else if (left.type == Type.Boolean && right.type == Type.Boolean)
                return new DialogVar(left.booleanValue != right.booleanValue);
            else if (left.type == Type.String && right.type == Type.String)
                return new DialogVar(left.stringValue != right.stringValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator <=(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue <= right.numberValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator >=(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue >= right.numberValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator <(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue < right.numberValue);
            else
                return new DialogVar();
        }

        public static DialogVar operator >(DialogVar left, DialogVar right)
        {
            if (left.type == Type.Number && right.type == Type.Number)
                return new DialogVar(left.numberValue > right.numberValue);
            else
                return new DialogVar();
        }
    }

    public class ExpressionEvaluator
    {
        private VariableManagerBase manager;
        private string expr;

        private class Token
        {
            public enum Type { Nil, Identifier, Number, Boolean, String, Plus, Minus, Times, Divide, Modulus, OpeningParenthesis, ClosingParenthesis, Equals, NotEquals, Bigger, Smaller, BigEquals, SmallEquals, EOF };
            public Type type { get; private set; }

            public float numberValue { get; private set; }
            public bool booleanValue { get; private set; }
            public string stringValue { get; private set; }

            public Token(Type type)
            {
                this.type = type;
            }

            public Token(Type type, float value)
            {
                this.type = type;
                numberValue = value;
            }

            public Token(Type type, bool value)
            {
                this.type = type;
                booleanValue = value;
            }

            public Token(Type type, string value)
            {
                this.type = type;
                stringValue = value;
            }
        }

        private Token currentToken;
        
        private ExpressionEvaluator(VariableManagerBase manager, string expr)
        {
            this.manager = manager;
            this.expr = expr;
            ForceEat();
        }
        
        private void Eat(Token.Type type)
        {
            if (currentToken.type != type)
                throw new DialogExprParseException();
            else
                ForceEat();
        }

        private void ForceEat()
        {
            if (expr.Length == 0)
            {
                currentToken = new Token(Token.Type.EOF);
            }
            else if (expr.StartsWith(" "))
            {
                expr = expr.Substring(1);
                ForceEat();
            }
            else if (expr.StartsWith("+"))
            {
                currentToken = new Token(Token.Type.Plus);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith("-"))
            {
                currentToken = new Token(Token.Type.Minus);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith("*"))
            {
                currentToken = new Token(Token.Type.Times);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith("/"))
            {
                currentToken = new Token(Token.Type.Divide);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith("%"))
            {
                currentToken = new Token(Token.Type.Modulus);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith("("))
            {
                currentToken = new Token(Token.Type.OpeningParenthesis);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith(")"))
            {
                currentToken = new Token(Token.Type.ClosingParenthesis);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith("=="))
            {
                currentToken = new Token(Token.Type.Equals);
                expr = expr.Substring(2);
            }
            else if (expr.StartsWith("!="))
            {
                currentToken = new Token(Token.Type.NotEquals);
                expr = expr.Substring(2);
            }
            else if (expr.StartsWith("<="))
            {
                currentToken = new Token(Token.Type.SmallEquals);
                expr = expr.Substring(2);
            }
            else if (expr.StartsWith("<"))
            {
                currentToken = new Token(Token.Type.Smaller);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith(">="))
            {
                currentToken = new Token(Token.Type.BigEquals);
                expr = expr.Substring(2);
            }
            else if (expr.StartsWith(">"))
            {
                currentToken = new Token(Token.Type.Bigger);
                expr = expr.Substring(1);
            }
            else if (expr.StartsWith("true"))
            {
                currentToken = new Token(Token.Type.Boolean, true);
                expr = expr.Substring(4);
            }
            else if (expr.StartsWith("false"))
            {
                currentToken = new Token(Token.Type.Boolean, false);
                expr = expr.Substring(5);
            }
            else if (expr[0] == '"')
            {
                expr = expr.Substring(1);
                string substr = new string(expr.TakeWhile(c => c != '"').ToArray());
                currentToken = new Token(Token.Type.String, substr);
                expr = expr.Substring(substr.Length + 1);
            }
            else if (expr[0] >= '0' && expr[0] <= '9')
            {
                bool dot = false;
                string number = new string(expr.TakeWhile(c => {
                    if (!dot)
                        return c >= '0' && c <= '9' || c == '.';
                    else
                        return c >= '0' && c <= '9';
                }).ToArray());
                currentToken = new Token(Token.Type.Number, float.Parse(number));
                expr = expr.Substring(number.Length);
            }
            else if (expr[0] >= 'A' && expr[0] <= 'Z' || expr[0] >= 'a' && expr[0] <= 'z' || expr[0] == '_')
            {
                string substr = new string(expr.TakeWhile(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '_').ToArray());
                currentToken = new Token(Token.Type.Identifier, substr);
                expr = expr.Substring(substr.Length);
            }
        }

        private DialogVar Evaluate()
        {
            DialogVar var = EvaluateSums();

            while (true)
            {
                if (currentToken.type == Token.Type.Equals)
                {
                    Eat(Token.Type.Equals);
                    var = var == EvaluateSums();
                }
                else if (currentToken.type == Token.Type.NotEquals)
                {
                    Eat(Token.Type.NotEquals);
                    var = var != EvaluateSums();
                }
                else if (currentToken.type == Token.Type.Bigger)
                {
                    Eat(Token.Type.Bigger);
                    var = var > EvaluateSums();
                }
                else if (currentToken.type == Token.Type.Smaller)
                {
                    Eat(Token.Type.Smaller);
                    var = var < EvaluateSums();
                }
                else if (currentToken.type == Token.Type.BigEquals)
                {
                    Eat(Token.Type.BigEquals);
                    var = var >= EvaluateSums();
                }
                else if (currentToken.type == Token.Type.SmallEquals)
                {
                    Eat(Token.Type.SmallEquals);
                    var = var <= EvaluateSums();
                }
                else
                {
                    break;
                }
            }

            return var;
        }

        private DialogVar EvaluateSums()
        {
            DialogVar var = EvaluateFactors();

            while (true)
            {
                if (currentToken.type == Token.Type.Plus)
                {
                    Eat(Token.Type.Plus);
                    var = var + EvaluateFactors();
                }
                else if (currentToken.type == Token.Type.Minus)
                {
                    Eat(Token.Type.Minus);
                    var = var - EvaluateFactors();
                }
                else
                {
                    break;
                }
            }

            return var;
        }

        private DialogVar EvaluateFactors()
        {
            DialogVar var = EvaluateValues();

            while (true)
            {
                if (currentToken.type == Token.Type.Times)
                {
                    Eat(Token.Type.Times);
                    var = var * EvaluateValues();
                }
                else if (currentToken.type == Token.Type.Divide)
                {
                    Eat(Token.Type.Divide);
                    var = var / EvaluateValues();
                }
                else if (currentToken.type == Token.Type.Modulus)
                {
                    Eat(Token.Type.Modulus);
                    var = var % EvaluateValues();
                }
                else
                {
                    break;
                }
            }

            return var;
        }

        private DialogVar EvaluateValues()
        {
            if (currentToken.type == Token.Type.OpeningParenthesis)
            {
                Eat(Token.Type.OpeningParenthesis);
                DialogVar var = Evaluate();
                Eat(Token.Type.ClosingParenthesis);
                return var;
            }
            else if (currentToken.type == Token.Type.Identifier)
            {
                string name = currentToken.stringValue;
                Eat(Token.Type.Identifier);
                return manager.GetValue(name);
            }
            else if (currentToken.type == Token.Type.Number)
            {
                float value = currentToken.numberValue;
                Eat(Token.Type.Number);
                return new DialogVar(value);
            }
            else if (currentToken.type == Token.Type.Boolean)
            {
                bool value = currentToken.booleanValue;
                Eat(Token.Type.Boolean);
                return new DialogVar(value);
            }
            else if (currentToken.type == Token.Type.String)
            {
                string value = currentToken.stringValue;
                Eat(Token.Type.String);
                return new DialogVar(value);
            }
            return null;
        }

        public static DialogVar Evaluate(VariableManagerBase manager, string expr)
        {
            return new ExpressionEvaluator(manager, expr).Evaluate();
        }
    }

    public abstract class VariableManagerBase
    {
        public abstract void SetValue(string key, DialogVar value);
        public abstract DialogVar GetValue(string key);

        public void SetValueWithExpr(string key, string expr)
        {
            SetValue(key, Evaluate(expr));
        }

        public DialogVar Evaluate(string expr)
        {
            return ExpressionEvaluator.Evaluate(this, expr);
        }
    }
}
