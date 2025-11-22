using System.Diagnostics;

class CalcCs
{
	/// <summary>
	/// 式を文字列として受け取って計算し、結果をint型で返します
	/// </summary>
	/// <param name="input">計算式</param>
	/// <returns></returns>
	public static int Calc(string input)
	{
		var tokens = Tokenize(input);
		var tree = Parse(tokens);
		var result = Eval(tree);
		return result;
	}

	static List<Token> Tokenize(string input)
	{
		var tokens = new List<Token>();
		var chars = new Peekable<char>([.. input.ToCharArray()]);
		while (chars.TryNext(out var letter))
		{
			switch (letter)
			{
				case >= '0' and <= '9':
					var num = letter - '0';
					while (chars.TryNextIf(letter => letter >= '0' && letter <= '9', out var c))
					{
						num = num * 10 + (c - '0');
					}
					tokens.Add(new NumberToken(num));
					break;
				case '(' or ')':
					tokens.Add(new ParenToken());
					break;
				case '+':
					tokens.Add(new PlusToken());
					break;
				case '-':
					tokens.Add(new MinusToken());
					break;
				case '*':
					tokens.Add(new MulToken());
					break;
				case '/':
					tokens.Add(new DivToken());
					break;
				default:
					break;
			}
		}
		return tokens;
	}

	static Node Parse(List<Token> tokens)
	{
		var scanner = new Peekable<Token>(tokens);
		var expr = ParseExpr(scanner);
		if (scanner.TryNext(out var _))
		{
			throw new Exception();
		}
		else
		{
			return expr;
		}
	}

	static Node ParseExpr(Peekable<Token> scanner)
	{
		var expr = ParseTerm(scanner);
		while (scanner.TryNextIf(token => token is PlusToken || token is MinusToken, out var token))
		{
			expr = new BinaryNode(expr, token switch
			{
				PlusToken => BinaryOp.Add,
				MinusToken => BinaryOp.Sub,
				_ => throw new NotImplementedException(),
			}, ParseTerm(scanner));
		}
		return expr;
	}

	static Node ParseTerm(Peekable<Token> scanner)
	{
		var term = ParseFactor(scanner);
		while (scanner.TryNextIf(token => token is MulToken || token is DivToken, out var token))
		{
			term = new BinaryNode(term, token switch
			{
				MulToken => BinaryOp.Mul,
				DivToken => BinaryOp.Div,
				_ => throw new NotImplementedException(),
			}, ParseFactor(scanner));
		}
		return term;
	}

	static Node ParseFactor(Peekable<Token> scanner)
	{
		if (scanner.TryNext(out var token))
		{
			switch (token)
			{
				case NumberToken number:
					return new NumberNode(number.Num);
				case ParenToken:
					var factor = ParseExpr(scanner);
					if (scanner.TryNextIf(token => token is ParenToken, out var paren))
					{
						return factor;
					}
					else
					{
						throw new Exception();
					}
				case MinusToken:
					return new UnaryNode(ParseFactor(scanner), UnaryOp.Minus);
				default:
					throw new Exception();
			}
		}
		else
		{
			throw new Exception();
		}
	}

	/// <summary>
	/// 構文木を解釈して計算します
	/// </summary>
	/// <param name="node">構文木</param>
	/// <returns></returns>
	/// <exception cref="UnreachableException"></exception>
	static int Eval(Node node)
	{
		return node switch
		{
			NumberNode number => number.Num,
			BinaryNode binaryNode => binaryNode.Op switch
			{
				BinaryOp.Add => Eval(binaryNode.Lhs) + Eval(binaryNode.Rhs),
				BinaryOp.Sub => Eval(binaryNode.Lhs) - Eval(binaryNode.Rhs),
				BinaryOp.Mul => Eval(binaryNode.Lhs) * Eval(binaryNode.Rhs),
				BinaryOp.Div => Eval(binaryNode.Lhs) / Eval(binaryNode.Rhs),
				_ => throw new UnreachableException(),
			},
			UnaryNode unaryNode => unaryNode.Op switch
			{
				UnaryOp.Minus => -Eval(unaryNode.Operand),
				_ => throw new UnreachableException(),
			},
			_ => throw new UnreachableException(),
		};
	}
}

abstract record Token;
sealed record NumberToken(int Num) : Token;
sealed record ParenToken : Token;
sealed record PlusToken : Token;
sealed record MinusToken : Token;
sealed record MulToken : Token;
sealed record DivToken : Token;

abstract record Node;
sealed record NumberNode(int Num) : Node;
sealed record BinaryNode(Node Lhs, BinaryOp Op, Node Rhs) : Node;
sealed record UnaryNode(Node Operand, UnaryOp Op) : Node;

enum BinaryOp
{
	Add,
	Sub,
	Mul,
	Div,
}

enum UnaryOp
{
	Minus,
}


class Peekable<T>(List<T> iter)
{
	private int ptr = -1;

	public bool TryNext(out T item)
	{
		if (++ptr < iter.Count)
		{
			item = iter[ptr];
			return true;
		}
		else
		{
			item = default;
			return false;
		}
	}
	public bool TryNextIf(Predicate<T> predicate, out T item)
	{
		if (ptr + 1 < iter.Count && predicate(iter[ptr + 1]))
		{
			item = iter[++ptr];
			return true;
		}
		else
		{
			item = default;
			return false;
		}
	}
}