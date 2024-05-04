using System;
using System.Collections.Generic;
using UnityEngine;

namespace System.Runtime.CompilerServices {
    internal static class IsExternalInit {}
}

public record Op(
    int Complexity,
    Func<List<object>, object> Eval,
    Func<int, List<AST>, AST, int, Derivative> Diff,
    Type RetType,
    string Str
);

public record UnOp(
    int Complexity,
    Func<List<object>, object> Eval,
    Func<int, List<AST>, AST, int, Derivative> Diff,
    Type InputType,
    Type RetType,
    string Str
) : Op(Complexity, Eval, Diff, RetType, Str);

public record BinOp(
    int Complexity,
    Func<List<object>, object> Eval,
    Func<int, List<AST>, AST, int, Derivative> Diff,
    (Type _1, Type _2) InputTypes,
    Type RetType,
    string Str
) : Op(Complexity, Eval, Diff, RetType, Str);

public record NoOp() : Op(0, null, null, null, null);

public class Ops {
    public static NoOp None = new();

    public static BinOp Add = new(1, Eval.Add, Derivative.FV.Add, (Types.VEC_TYPE, Types.VEC_TYPE), Types.VEC_TYPE, "+");
    public static BinOp Sub = new(1, Eval.Sub, Derivative.FV.Add, (Types.VEC_TYPE, Types.VEC_TYPE), Types.VEC_TYPE, "-");
    public static BinOp Cro = new(2, Eval.Cro, Derivative.FV.Cro, (Types.VEC_TYPE, Types.VEC_TYPE), Types.VEC_TYPE, "^");
    public static BinOp ScM = new(1, Eval.ScM, Derivative.FV.ScM, (Types.VEC_TYPE, Types.FLT_TYPE), Types.VEC_TYPE, "×");
    public static BinOp ScD = new(1, Eval.ScD, Derivative.FV.ScD, (Types.VEC_TYPE, Types.FLT_TYPE), Types.VEC_TYPE, "÷");
    public static BinOp Dot = new(1, Eval.Dot, Derivative.FF.Dot, (Types.VEC_TYPE, Types.VEC_TYPE), Types.FLT_TYPE, "·");
    public static BinOp Dst = new(2, Eval.Dst, null,              (Types.VEC_TYPE, Types.VEC_TYPE), Types.FLT_TYPE, "to");
    public static BinOp FlA = new(1, Eval.FlA, Derivative.FF.FlA, (Types.FLT_TYPE, Types.FLT_TYPE), Types.FLT_TYPE, "+");
    public static BinOp FlS = new(1, Eval.FlS, Derivative.FF.FlS, (Types.FLT_TYPE, Types.FLT_TYPE), Types.FLT_TYPE, "-");
    public static BinOp FlM = new(1, Eval.FlM, Derivative.FF.FlM, (Types.FLT_TYPE, Types.FLT_TYPE), Types.FLT_TYPE, "×");
    public static BinOp FlD = new(1, Eval.FlD, Derivative.FF.FlD, (Types.FLT_TYPE, Types.FLT_TYPE), Types.FLT_TYPE, "÷");

    public static UnOp Neg = new(1, Eval.Neg, Derivative.FV.Neg, Types.VEC_TYPE, Types.VEC_TYPE, "-");
    public static UnOp Mag = new(2, Eval.Mag, Derivative.FF.Mag, Types.VEC_TYPE, Types.FLT_TYPE, "mag");
    public static UnOp FlN = new(1, Eval.FlN, Derivative.FF.FlN, Types.FLT_TYPE, Types.FLT_TYPE, "-");
    public static UnOp FlI = new(1, Eval.FlI, Derivative.FF.FlI, Types.FLT_TYPE, Types.FLT_TYPE, "1.0 /");
}

public static class Eval {
    // core ops
    public static object Add(List<object> args) => (Vector3)args[0] + (Vector3)args[1];
    public static object Cro(List<object> args) => Vector3.Cross((Vector3)args[0], (Vector3)args[1]);
    public static object ScM(List<object> args) => (Vector3)args[0] * (float)args[1];
    public static object Dot(List<object> args) => Vector3.Dot((Vector3)args[0], (Vector3)args[1]);
    public static object Mag(List<object> args) => Vector3.Magnitude((Vector3)args[0]);
    public static object Neg(List<object> args) => -(Vector3)args[0];
    public static object FlM(List<object> args) => (float)args[0] * (float)args[1];
    public static object FlA(List<object> args) => (float)args[0] + (float)args[1];
    public static object FlI(List<object> args) => 1.0f / (float)args[0];
    public static object FlN(List<object> args) => -(float)args[0];

    // redundant ops (compositions of core ops)
    public static object Dst(List<object> args) => Vector3.Distance((Vector3)args[0], (Vector3)args[1]);
    public static object Sub(List<object> args) => (Vector3)args[0] - (Vector3)args[1];
    public static object FlD(List<object> args) => (float)args[0] / (float)args[1];
    public static object ScD(List<object> args) => (Vector3)args[0] * (1.0f / (float)args[1]);
    public static object FlS(List<object> args) => (float)args[0] - (float)args[1];
}

public static class Types {
    static readonly public Type FLT_TYPE = typeof(float);
    static readonly public Type VEC_TYPE = typeof(Vector3);
}