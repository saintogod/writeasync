﻿// <copyright file="Expression.cs" company="Brian Rogers">
// Copyright (c) Brian Rogers. All rights reserved.
// </copyright>

namespace Four4
{
    using System;

    public struct Expression
    {
        private readonly string text;
        private readonly NumberStack operands;
        private readonly int count;

        private Expression(int count, string text, NumberStack operands)
        {
            this.count = count;
            this.text = text;
            this.operands = operands;
        }

        public bool IsValid => this.operands.IsValid;

        public int NumeralCount => this.count;

        public Number Result => this.operands.Result;

        public static Number Eval(string input)
        {
            Expression expr = default(Expression);
            string[] tokens = input.Split(' ');
            foreach (string token in tokens)
            {
                expr = expr.Append(token);
            }

            return expr.Result;
        }

        public override string ToString()
        {
            return this.text ?? string.Empty;
        }

        public Expression Append(string token)
        {
            switch (token)
            {
                case "+":
                    return this.Binary(token, (x, y) => x + y);
                case "-":
                    return this.Binary(token, (x, y) => x - y);
                case "*":
                    return this.Binary(token, (x, y) => x * y);
                case "/":
                    return this.Binary(token, (x, y) => x / y);
                case "!":
                    return this.Unary(token, x => x.Factorial());
                case "R":
                    return this.Unary(token, x => x.SquareRoot());
                case Number.PointFour:
                case Number.PointFourRepeating:
                case "4":
                    return this.Push(1, token, Number.Parse(token));
                case "44":
                    return this.Push(2, token, Number.Parse(token));
                case "444":
                    return this.Push(3, token, Number.Parse(token));
                case "4444":
                    return this.Push(4, token, Number.Parse(token));
                default:
                    throw new ArgumentException("Bad token '" + token + "'", nameof(token));
            }
        }

        private Expression Push(int add, string token, Number number)
        {
            return new Expression(this.count + add, this.Join(token), this.operands.Push(number));
        }

        private Expression Binary(string token, Func<Number, Number, Number> op)
        {
            return new Expression(this.count, this.Join(token), this.operands.Apply2(op));
        }

        private Expression Unary(string token, Func<Number, Number> op)
        {
            return new Expression(this.count, this.Join(token), this.operands.Apply1(op));
        }

        private string Join(string token)
        {
            if (this.text == null)
            {
                return token;
            }

            return this.text + " " + token;
        }

        private struct NumberStack
        {
            private static readonly Number X = default(Number);

            private readonly Number n1;
            private readonly Number n2;
            private readonly Number n3;
            private readonly Number n4;
            private readonly int size;

            private NumberStack(Number n1, Number n2, Number n3, Number n4, int size)
            {
                this.n1 = n1;
                this.n2 = n2;
                this.n3 = n3;
                this.n4 = n4;
                this.size = size;
            }

            public bool IsValid
            {
                get
                {
                    switch (this.size)
                    {
                        case 1:
                            return this.n1.IsValid;
                        case 2:
                            return this.n2.IsValid;
                        case 3:
                            return this.n3.IsValid;
                        case 4:
                            return this.n4.IsValid;
                        default:
                            return true;
                    }
                }
            }

            public Number Result
            {
                get
                {
                    switch (this.size)
                    {
                        case 1:
                            return this.n1;
                        default:
                            return default(Number);
                    }
                }
            }

            public NumberStack Push(Number n)
            {
                switch (this.size)
                {
                    case 0:
                        return new NumberStack(n, X, X, X, 1);
                    case 1:
                        return new NumberStack(this.n1, n, X, X, 2);
                    case 2:
                        return new NumberStack(this.n1, this.n2, n, X, 3);
                    case 3:
                        return new NumberStack(this.n1, this.n2, this.n3, n, 4);
                    default:
                        throw new InvalidOperationException("Stack full!");
                }
            }

            public NumberStack Apply1(Func<Number, Number> op)
            {
                switch (this.size)
                {
                    case 0:
                        return new NumberStack(X, X, X, X, 1);
                    case 1:
                        return new NumberStack(op(this.n1), X, X, X, 1);
                    case 2:
                        return new NumberStack(this.n1, op(this.n2), X, X, 2);
                    case 3:
                        return new NumberStack(this.n1, this.n2, op(this.n3), X, 3);
                    default:
                        return new NumberStack(this.n1, this.n2, this.n3, op(this.n4), 4);
                }
            }

            public NumberStack Apply2(Func<Number, Number, Number> op)
            {
                switch (this.size)
                {
                    case 0:
                    case 1:
                        return new NumberStack(X, X, X, X, 1);
                    case 2:
                        return new NumberStack(op(this.n1, this.n2), X, X, X, 1);
                    case 3:
                        return new NumberStack(this.n1, op(this.n2, this.n3), X, X, 2);
                    default:
                        return new NumberStack(this.n1, this.n2, op(this.n3, this.n4), X, 3);
                }
            }
        }
    }
}
