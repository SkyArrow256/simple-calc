use std::{iter::Peekable, vec::IntoIter};

/// 式を文字列として受け取って計算し、結果をi32で返します
pub fn calc(input: &str) -> Result<i32, Error> {
    let tokens = tokenize(input)?;
    let tree = parse(tokens)?;
    let result = eval(tree);
    Ok(result)
}

/// 文字列を字句解析してトークン列にします
fn tokenize(input: &str) -> Result<Vec<Token>, Error> {
    let mut tokens = Vec::new();
    let mut chars = input.chars().peekable();
    while let Some(letter) = chars.next() {
        match letter {
            '0'..='9' => {
                let mut num = letter as i32 - '0' as i32;
                while let Some(c) = chars.next_if(|c| c.is_ascii_digit()) {
                    num = num * 10 + (c as i32 - '0' as i32);
                }
                tokens.push(Token::Number(num));
            }
            '(' | ')' => tokens.push(Token::Paren),
            '+' => tokens.push(Token::Plus),
            '-' => tokens.push(Token::Minus),
            '*' => tokens.push(Token::Mul),
            '/' => tokens.push(Token::Div),
            _ if letter.is_whitespace() => (),
            _ => return Err(Error::UnknownToken),
        }
    }
    Ok(tokens)
}

/// トークン
#[derive(PartialEq)]
enum Token {
    /// 数値
    Number(i32),
    /// 括弧
    Paren,
    /// 加算記号
    Plus,
    ///　減算記号
    Minus,
    /// 乗算記号
    Mul,
    /// 除算記号
    Div,
}

/// トークンのイテレータをパースして構文木にします
fn parse(tokens: Vec<Token>) -> Result<Node, Error> {
    let mut tokens = tokens.into_iter().peekable();
    let expr = parse_expr(&mut tokens)?;
    if tokens.next().is_some() {
        Err(Error::UnexpectedToken)
    } else {
        Ok(expr)
    }
}

/// 式をパースして構文木にします
fn parse_expr(tokens: &mut Peekable<IntoIter<Token>>) -> Result<Node, Error> {
    let mut expr = parse_term(tokens)?;
    while let Some(token) = tokens.next_if(|token| *token == Token::Plus || *token == Token::Minus)
    {
        expr = Node::Binary {
            lhs: Box::new(expr),
            op: match token {
                Token::Plus => BinOp::Add,
                Token::Minus => BinOp::Sub,
                _ => unreachable!(),
            },
            rhs: Box::new(parse_term(tokens)?),
        }
    }
    Ok(expr)
}

/// 項をパースして構文木にします
fn parse_term(tokens: &mut Peekable<IntoIter<Token>>) -> Result<Node, Error> {
    let mut term = parse_factor(tokens)?;
    while let Some(token) = tokens.next_if(|token| *token == Token::Mul || *token == Token::Div) {
        term = Node::Binary {
            lhs: Box::new(term),
            op: match token {
                Token::Mul => BinOp::Mul,
                Token::Div => BinOp::Div,
                _ => unreachable!(),
            },
            rhs: Box::new(parse_factor(tokens)?),
        }
    }
    Ok(term)
}

/// 因数をパースして構文木にします
fn parse_factor(tokens: &mut Peekable<IntoIter<Token>>) -> Result<Node, Error> {
    if let Some(token) = tokens.next() {
        match token {
            Token::Number(n) => Ok(Node::Number(n)),
            Token::Paren => {
                let factor = parse_expr(tokens)?;
                if let Some(Token::Paren) = tokens.next() {
                    Ok(factor)
                } else {
                    Err(Error::UnexpectedToken)
                }
            }
            Token::Minus => Ok(Node::Unary {
                operand: Box::new(parse_factor(tokens)?),
                op: UnOp::Minus,
            }),
            Token::Mul | Token::Div | Token::Plus => Err(Error::UnexpectedToken),
        }
    } else {
        Err(Error::UnexpectedToken)
    }
}

enum Node {
    Number(i32),
    Binary {
        lhs: Box<Node>,
        op: BinOp,
        rhs: Box<Node>,
    },
    Unary {
        operand: Box<Node>,
        op: UnOp,
    },
}

enum BinOp {
    Add,
    Sub,
    Mul,
    Div,
}

enum UnOp {
    Minus,
}

/// 構文木を解釈して計算します
fn eval(tree: Node) -> i32 {
    match tree {
        Node::Number(num) => num,
        Node::Binary { lhs, op, rhs } => {
            let lhs = eval(*lhs);
            let rhs = eval(*rhs);
            match op {
                BinOp::Add => lhs + rhs,
                BinOp::Sub => lhs - rhs,
                BinOp::Mul => lhs * rhs,
                BinOp::Div => lhs / rhs,
            }
        }
        Node::Unary { operand, op } => match op {
            UnOp::Minus => -eval(*operand),
        },
    }
}

#[derive(Debug)]
pub enum Error {
    /// 未知のトークンです
    UnknownToken,
    /// 予期されぬトークンです
    UnexpectedToken,
}
