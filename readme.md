# simple-calc
## 概要
Rust / C# で実装されたシンプルな電卓です。四則演算と括弧が扱えます。
```
cargo run
```
または
```
dotnet run
```
で実行します。

```
-1 * -(2 + 3 / 3)
-> 3
```
Ctrl + D で終了します。

## 実装について
とてもシンプルに実装されています。
``lib.rs`` / ``CalcCS.cs``が電卓の本体です。

### Rust
```Rust
pub fn calc(input: &str) -> Result<i32, Error> {
    
    // まず文字列をトークンのベクタに変換
    let tokens = tokenize(input)?;
    
    // トークンを読み取って構文木を構築
    let tree = parse(tokens)?;
    
    // 構文木を解釈して計算
    let result = eval(tree);
    
    Ok(result)
}
```

(WIP)

### C#
```CSharp
public static int Calc(string input)
{
    
    // まず文字列をトークンのベクタに変換
    var tokens = Tokenize(input);
    
    // トークンを読み取って構文木を構築
    var tree = Parse(tokens);
    
    // 構文木を解釈して計算
    var result = Eval(tree);
    
    return result;
}
```

トークンを1つ先読みできるイテレータのようなものが標準ライブラリに見つからなかったので、トークンを先読みできる``Peekable``というクラスを実装しています。

```CSharp
class Peekable<T>(List<T> iter)
{
	private int ptr;
	public bool TryNext(out T item);
	public bool TryNextIf(Predicate<T> predicate, out T item);
}
```

RustのPeekableと挙動は大体一緒だと思います。

(WIP)

## 最後に
Rustはともかく、C#はほとんど書いたことがないので「命名規則がおかしい」「こういう書き方は普通しない」など不自然なところがあれば気軽にプルリクを送ってください🙇


(送ってもらえても反応結構遅れるかもです、ごめんなさい)
