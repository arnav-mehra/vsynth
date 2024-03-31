using System;
using System.Collections.Generic;
using UnityEngine;

public class Ops {
    public enum Op {
        None, // returns some literal
        Add, Sub, Cro, Rot, ScM, ScD, // returns vector
        Dst, Dot, Mag, FlM, FlD, FlA, FlS // returns float
    }

    public static object Add(List<object> args) => (Vector3)args[0] + (Vector3)args[1];
    public static object Sub(List<object> args) => (Vector3)args[0] - (Vector3)args[1];
    public static object Cro(List<object> args) => Vector3.Cross((Vector3)args[0], (Vector3)args[1]);
    public static object Rot(List<object> args) => Quaternion.AngleAxis((float)args[2], (Vector3)args[1]) * (Vector3)args[0];
    public static object ScM(List<object> args) => (Vector3)args[0] * (float)args[1];
    public static object ScD(List<object> args) => (Vector3)args[0] * (1.0f / (float)args[1]);
    public static object Dst(List<object> args) => Vector3.Distance((Vector3)args[0], (Vector3)args[1]);
    public static object Dot(List<object> args) => Vector3.Dot((Vector3)args[0], (Vector3)args[1]);
    public static object Mag(List<object> args) => Vector3.Magnitude((Vector3)args[0]);
    public static object FlM(List<object> args) => (float)args[0] * (float)args[1];
    public static object FlD(List<object> args) => (float)args[0] / (float)args[1];
    public static object FlA(List<object> args) => (float)args[0] + (float)args[1];
    public static object FlS(List<object> args) => (float)args[0] - (float)args[1];
}

public static class ComplexityExt {
    readonly static int[] COMPLEXITIES = {
        0, // None,
        1, 1, 2, 3, 1, 1, // Add, Sub, Cro, Rot, ScM, ScD,
        2, 1, 1, 1, 1, 1, 1 // Dst, Dot, Mag, FlM, FlD, FlA, FlS
    };

    public static int Complexity(this Ops.Op op) => COMPLEXITIES[(int)op];
}

public static class EvalExt {
    readonly static Func<List<object>, object>[] EVAL_FNS = {
        null, // None,
        Ops.Add, Ops.Sub, Ops.Cro, Ops.Rot, Ops.ScM, Ops.ScD, // Add, Sub, Cro, Rot, ScM, ScD,
        Ops.Dst, Ops.Dot, Ops.Mag, Ops.FlM, Ops.FlD, Ops.FlA, Ops.FlS // Dst, Dot, Mag, FlM, FlD, FlA, FlS
    };

    public static object Eval(this Ops.Op op, List<object> args) => EVAL_FNS[(int)op](args);
}

public static class DiffExt {
    readonly static Func<EnvType, List<AST>, AST, int, Derivative>[] DIFF_FNS = {
        null, // None,
        Derivative.FV.Add, Derivative.FV.Sub, Derivative.FV.Cro, null, Derivative.FV.ScM, Derivative.FV.ScD, // Add, Sub, Cro, Rot, ScM, ScD,
        Derivative.FF.Dst, Derivative.FF.Dot, Derivative.FF.Mag, Derivative.FF.FlM, Derivative.FF.FlD, Derivative.FF.FlA, Derivative.FF.FlS // Dst, Dot, Mag, FlM, FlD, FlA, FlS
    };

    public static Derivative Diff(this Ops.Op op, EnvType et, List<AST> args, AST wrt, int coord) => DIFF_FNS[(int)op](et, args, wrt, coord);
}

public static class TypeExt {
    static readonly public Type FLT_TYPE = typeof(float);
    static readonly public Type VEC_TYPE = typeof(Vector3);

    readonly static Type[] RET_TYPES = {
        null, // None,
        VEC_TYPE, VEC_TYPE, VEC_TYPE, VEC_TYPE, VEC_TYPE, VEC_TYPE, // Add, Sub, Cro, Rot, ScM, ScD,
        FLT_TYPE, FLT_TYPE, FLT_TYPE, FLT_TYPE, FLT_TYPE, FLT_TYPE, FLT_TYPE // Dst, Dot, Mag, FlM, FlD, FlA, FlS
    };

    public static Type RetType(this Ops.Op op) => RET_TYPES[(int)op];
}

public static class StrExt {
    readonly static string[] STRS = {
        null, // None,
        "+", "-", "^", "r", "×", "÷", // Add, Sub, Cro, Rot, ScM, ScD,
        null, "·", null, "×", "÷", "+", "-" // Dst, Dot, Mag, FlM, FlD, FlA, FlS
    };

    public static string Str(this Ops.Op op) => STRS[(int)op];
}