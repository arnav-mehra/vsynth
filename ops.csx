using System.Numerics;

delegate int Op(int a, int b);

class Ops {
    public static int Add (int a, int b) => a + b;
    public static int Mul (int a, int b) => a * b;
    public static int Div (int a, int b) => a / b;
}

class AST {
    Op op;
    AST[] children;

    AST() {
        op = Ops.Add;
        children = {};
    }
}

var c = Ops.Add(2, 1);
Console.WriteLine(c.ToString());