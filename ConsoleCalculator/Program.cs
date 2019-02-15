using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApp12
{
    class Program
    {
        static void Main(string[] args)
        {
            //read equation from console
            string equation = Console.ReadLine();

            //rearrange equation into array of tokens
            string[] eq = Rearange(equation);
            
            //change array of strings into tokens with more properties
            Token[] tokens = Tokenize(eq);

            //create quee and rearrange tokens into reversed polish notation
            Queue<Token> tokensAfter = new Queue<Token>(ShuntingYard(tokens));
            
            //show RPN
            foreach (Token t in tokensAfter)
            {
                Console.Write(t.GetSign);
            }

            //final calculation
            double result = CalculateExpression(tokensAfter);

            Console.WriteLine($"result is {result}");
        }

        static string[] Rearange(string eq)
        {
            //add spaces to be able to split
            eq = eq.Replace("+", " + ");
            eq = eq.Replace("-", " - ");
            eq = eq.Replace("*", " * ");
            eq = eq.Replace("/", " / ");
            eq = eq.Replace(")", " ) ");
            eq = eq.Replace("(", " ( ");
            //remove excess spaces
            eq = eq.Replace("  ", " ");
            //trim spaces at begiinng and end
            eq = eq.Trim();
            //split into separate tokens
            return eq.Split(' ');
        }

        //change array of strings to array of tokens
        public static Token[] Tokenize(string[] eq)
        {
            Token[] tokens = new Token[eq.Length];
            for (int i = 0; i < eq.Length; i++)
            {
                //check if unitary minus
                if((i == 0 && eq[i] == "-"))
                {
                    eq[i] = "_";
                }
                //create token from char
                tokens[i] = Token.ToToken(eq[i]);
            }
            return tokens;
        }
        //shunting yard allgorithm for RPN
        public static Queue<Token> ShuntingYard(Token[] tokens)
        {
            //create queue and stack
            Queue<Token> output = new Queue<Token>();
            Stack<Token> operators = new Stack<Token>();

            //go over all input tokens
            for (int i = 0; i < tokens.Length; i++)
            {
                //read token
                Token t = tokens[i];

                //if token is a number
                if (t.GetTokenType == Token.Type.number)
                {
                    output.Enqueue(t);
                }

                //if token is an operator
                else if (t.GetTokenType == Token.Type.oper)
                {
                    //check precedense
                    while (operators.Count > 0 && (operators.Peek().GetPrecedence > t.GetPrecedence ||
                        (operators.Peek().GetPrecedence == t.GetPrecedence && operators.Peek().GetAssoc == Token.Associativity.left)) 
                        && operators.Peek().GetTokenType != Token.Type.leftBra)
                    {
                        output.Enqueue(operators.Pop());
                    }
                    operators.Push(t);
                }
                //left bracket
                else if (t.GetTokenType == Token.Type.leftBra)
                {
                    operators.Push(t);
                }
                //right bracket
                else if (t.GetTokenType == Token.Type.rightBra)
                {
                    bool end = true;
                    do
                    {
                        //if no more operators then must be mismatched brackets
                        if (operators.Count == 0)
                        {
                            throw new SystemException("Unmatched Parenthesis!");
                        }
                        
                        else if (operators.Peek().GetTokenType != Token.Type.leftBra)
                        {
                            output.Enqueue(operators.Pop());
                        }
                        //left bracket found
                        else if (operators.Peek().GetTokenType == Token.Type.leftBra)
                        {
                            operators.Pop();
                            end = false;
                        }
                        
                    }
                    while (end);
                }
            }
            //if operators left on stack add them to queue
            if (operators.Count > 0)
            {
                int tmp = operators.Count;
                for (int i = 0; i < tmp; i++)
                {
                    output.Enqueue(operators.Pop());
                }
            }
            return output;
        }

        //calculatin result from RPN
        public static double CalculateExpression(Queue<Token> queue)
        {
            //stack for result
            Stack<double> awaiting = new Stack<double>();
            while (queue.Count > 0)
            {
                //check if operand or operator, operand push to stack, operator perform
                if (queue.Peek().GetTokenType == Token.Type.number)
                {
                    awaiting.Push(double.Parse(queue.Dequeue().GetSign));
                }
                //check if operator using 2 operands
                else if (queue.Peek().GetTokenType == Token.Type.oper && queue.Peek().NumberOfParams == 2)
                {
                    double op2 = awaiting.Pop();
                    double op1 = awaiting.Pop();
                    double resultTmp = 0;

                    //different cases
                    switch (queue.Dequeue().GetSign)
                    {
                        case "-":
                            resultTmp = op1 - op2;
                            break;
                        case "+":
                            resultTmp = op1 + op2;
                            break;
                        case "*":
                            resultTmp = op1 * op2;
                            break;
                        case "/":
                            resultTmp = op1 / op2;
                            break;
                    }
                    awaiting.Push(resultTmp);
                }
                //if operator with one parameter
                else if (queue.Peek().GetTokenType == Token.Type.oper && queue.Peek().NumberOfParams == 1)
                {
                    double op1 = awaiting.Pop();
                    double resultTmp = 0;
                    switch (queue.Dequeue().GetSign)
                    {
                        case "_":
                            resultTmp = -op1;
                            break;
                    }
                    awaiting.Push(resultTmp);
                }
            }
            return awaiting.Pop();
        }
    }

    public class Token
    {
        //properties of token
        double value = 0;
        int numberOfParams = 2;
        int precedence = 10;
        char symbol = ' ';
        public enum Associativity { left, right };
        Associativity assoc = Associativity.left;
        public enum Type { number, oper, leftBra, rightBra, notAToken }
        Type typeOf { get; set; } = Type.notAToken;

        //access to properties
        public string GetSign
        {
            get
            {
                //check if number or operator when returning
                if (typeOf == Type.number)
                {
                    return value.ToString();
                }
                else
                {
                    return symbol.ToString();
                }
            }
        }

        public int NumberOfParams
        {
            get
            {
                return numberOfParams;
            }
        }

        public Associativity GetAssoc
        {
            get
            {
                return assoc;
            }
        }

        public int GetPrecedence
        {
            get
            {
                return precedence;
            }
        }
        public Type GetTokenType
        {
            get
            {
                return typeOf;
            }
        }
        
        //change string into token
        public static Token ToToken(string num)
        {
            Token t = new Token();
            //list of possible tokens
            if (double.TryParse(num, out t.value))
            {
                t.typeOf = Type.number;
                return t;
            }
            else if (num == "+")
            {
                t.symbol = '+';
                t.precedence = 10;
                t.typeOf = Type.oper;
                return t;
            }
            else if (num == "-")
            {
                t.symbol = '-';
                t.precedence = 10;
                t.typeOf = Type.oper;
                return t;
            }
            else if (num == "*")
            {
                t.symbol = '*';
                t.precedence = 20;
                t.typeOf = Type.oper;
                return t;
            }
            else if (num == "/")
            {
                t.symbol = '/';
                t.precedence = 20;
                t.typeOf = Type.oper;
                return t;
            }
            else if (num == "(")
            {
                t.symbol = '(';
                t.precedence = 30;
                t.typeOf = Type.leftBra;
                return t;
            }
            else if (num == ")")
            {
                t.symbol = ')';
                t.precedence = 30;
                t.typeOf = Type.rightBra;
                return t;
            }
            else if (num == "_")
            {
                t.symbol = '_';
                t.precedence = 40;
                t.typeOf = Type.oper;
                t.numberOfParams = 1;
                t.assoc = Associativity.right;
                return t;
            }
            //if unknown operator
            else
            {
                throw new SystemException("Detected wrong sign!");
            }
        }



    }
}