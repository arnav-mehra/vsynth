using System;
using System.Collections.Generic;

using static Ops;
using static TypeExt;

public static class ASTCtors {
    static readonly List<float> STD_ANGLES = new() { 180.0f };
    public static List<AST> STD_ASTS = STD_ANGLES.ConvertAll(a => new AST(new object[] { a, a }));

    public static readonly List<(Op op, Type t, Func<AST, AST> fn)> UNARY_CTORS = new() {
        (Op.Mag, VEC_TYPE, a => new AST(Op.Mag, new() { a }))
    };

    public static readonly List<(Op op, Type t1, Type t2, Func<AST, AST, AST> fn)> BINARY_CTORS = new() {
        (Op.Add, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Add, new() { a, b })),
        (Op.Sub, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Sub, new() { a, b })),
        (Op.Sub, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Sub, new() { b, a })),
        (Op.Cro, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Cro, new() { a, b })),
        (Op.Cro, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Cro, new() { b, a })),
        (Op.Dot, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Dot, new() { a, b })),
        (Op.ScM, VEC_TYPE, FLT_TYPE, (a, b) => new AST(Op.ScM, new() { a, b })),
        (Op.ScD, VEC_TYPE, FLT_TYPE, (a, b) => new AST(Op.ScD, new() { a, b })),
        (Op.FlM, FLT_TYPE, FLT_TYPE, (a, b) => new AST(Op.FlM, new() { a, b })),
        (Op.FlD, FLT_TYPE, FLT_TYPE, (a, b) => new AST(Op.FlD, new() { a, b })),
        (Op.FlA, FLT_TYPE, FLT_TYPE, (a, b) => new AST(Op.FlA, new() { a, b })),
        (Op.FlS, FLT_TYPE, FLT_TYPE, (a, b) => new AST(Op.FlS, new() { a, b })),
    };
}