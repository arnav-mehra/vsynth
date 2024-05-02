using System;
using System.Collections.Generic;
using UnityEngine;

public enum Op {
    None, // None,
    Add, Cro, ScM, // Add, Cro, ScM,
    Dot, Mag, Neg, // Dot, Mag, Neg,
    FlM, FlA, FlI, FlN, // FlM, FlA, FlI, FlN
    Sub, Rot, ScD, Dst, FlD, FlS // Sub, Rot, ScD, Dst, FlD, FlS
}

public static class ComplexityExt {
    readonly static int[] COMPLEXITIES = {
        0, // None,
        1, 2, 1, // Add, Cro, ScM,
        1, 2, 1, // Dot, Mag, Neg,
        1, 1, 1, 1, // FlM, FlA, FlI, FlN
        1, 5, 1, 1, 1, 1 // Sub, Rot, ScD, Dst, FlD, FlS
    };

    public static int Complexity(this Op op) => COMPLEXITIES[(int)op];
}

public static class EvalExt {
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
    public static object Rot(List<object> args) => Quaternion.AngleAxis((float)args[2], (Vector3)args[1]) * (Vector3)args[0];
    public static object Sub(List<object> args) => (Vector3)args[0] - (Vector3)args[1];
    public static object FlD(List<object> args) => (float)args[0] / (float)args[1];
    public static object ScD(List<object> args) => (Vector3)args[0] * (1.0f / (float)args[1]);
    public static object FlS(List<object> args) => (float)args[0] - (float)args[1];

    readonly static Func<List<object>, object>[] EVAL_FNS = {
        null, // None,
        Add, Cro, ScM, // Add, Cro, ScM,
        Dot, Mag, Neg, // Dot, Mag, Neg,
        FlM, FlA, FlI, FlN, // FlM, FlA, FlI, FlN
        Sub, Rot, ScD, Dst, FlD, FlS // Sub, Rot, ScD, Dst, FlD, FlS
    };

    public static object Eval(this Op op, List<object> args) => EVAL_FNS[(int)op](args);
}

public static class DiffExt {
    readonly static Func<int, List<AST>, AST, int, Derivative>[] DIFF_FNS = {
        null, // None,
        Derivative.FV.Add, Derivative.FV.Cro, Derivative.FV.ScM, // Add, Cro, ScM,
        Derivative.FF.Dot, Derivative.FF.Mag, Derivative.FV.Neg, // Dot, Mag, Neg,
        Derivative.FF.FlM, Derivative.FF.FlA, Derivative.FF.FlI, Derivative.FF.FlN, // FlM, FlA, FlI, FlN
        Derivative.FV.Sub, null, Derivative.FV.ScD, null, Derivative.FF.FlD, Derivative.FF.FlS // Sub, Rot, ScD, Dst, FlD, FlS
    };

    public static Derivative Diff(this Op op, int et, List<AST> args, AST wrt, int coord) => DIFF_FNS[(int)op](et, args, wrt, coord);
}

public static class TypeExt {
    static readonly public Type FLT_TYPE = typeof(float);
    static readonly public Type VEC_TYPE = typeof(Vector3);

    readonly static Type[] RET_TYPES = {
        VEC_TYPE, // None (variable),
        VEC_TYPE, VEC_TYPE, VEC_TYPE, // Add, Cro, ScM,
        FLT_TYPE, FLT_TYPE, VEC_TYPE, // Dot, Mag, Neg,
        FLT_TYPE, FLT_TYPE, FLT_TYPE, FLT_TYPE, // FlM, FlA, FlI, FlN
        VEC_TYPE, VEC_TYPE, VEC_TYPE, FLT_TYPE, FLT_TYPE, FLT_TYPE // Sub, Rot, ScD, Dst, FlD, FlS
    };

    public static Type RetType(this Op op) => RET_TYPES[(int)op];
}

public static class StrExt {
    readonly static string[] STRS = {
        null, // None,
        "+", "^", "×", // Add, Cro, ScM,
        "·", null, "-", // Dot, Mag, Neg,
        "×", "+", "÷", "-", // FlM, FlA, FlI, FlN
        "-", "r", "÷", null, "÷", "-" // Sub, Rot, ScD, Dst, FlD, FlS
    };

    public static string Str(this Op op) => STRS[(int)op];
}