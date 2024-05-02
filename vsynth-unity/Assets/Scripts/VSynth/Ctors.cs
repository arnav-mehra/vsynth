using System;
using System.Collections.Generic;

using static TypeExt;

public static class Ctors {
    static readonly List<float> STD_ANGLES = new() { 180.0f };
    
    public static List<AST> MakeStdAsts(int env_cnt) => (
        STD_ANGLES.ConvertAll(a => new AST(
            Utils.Range(1, env_cnt)
                 .ConvertAll(_ => (object)a)
        ))
    );

    public static List<(Op op, Type t, Func<AST, AST> fn)> MakeUnaryCtors(int env_cnt) => (
        new() {
            (Op.Mag, VEC_TYPE, a => new AST(Op.Mag, new() { a }, env_cnt)),
            (Op.Neg, VEC_TYPE, a => new AST(Op.Neg, new() { a }, env_cnt)),
            (Op.FlI, FLT_TYPE, a => new AST(Op.FlI, new() { a }, env_cnt)),
            (Op.FlN, FLT_TYPE, a => new AST(Op.FlN, new() { a }, env_cnt)),
        }
    );

    public static List<(Op op, Type t1, Type t2, Func<AST, AST, AST> fn)> MakeBinaryCtors(int env_cnt) => (
        new() {
            (Op.Add, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Add, new() { a, b }, env_cnt)),
            (Op.Cro, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Cro, new() { a, b }, env_cnt)),
            (Op.ScM, VEC_TYPE, FLT_TYPE, (a, b) => new AST(Op.ScM, new() { a, b }, env_cnt)),
            (Op.Dot, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Dot, new() { a, b }, env_cnt)),
            (Op.FlM, FLT_TYPE, FLT_TYPE, (a, b) => new AST(Op.FlM, new() { a, b }, env_cnt)),
            (Op.FlA, FLT_TYPE, FLT_TYPE, (a, b) => new AST(Op.FlA, new() { a, b }, env_cnt)),

            /*
            (Op.FlD, FLT_TYPE, FLT_TYPE, (a, b) => new AST(Op.FlD, new() { a, b }, env_cnt)),
            (Op.FlS, FLT_TYPE, FLT_TYPE, (a, b) => new AST(Op.FlS, new() { a, b }, env_cnt)),
            (Op.ScD, VEC_TYPE, FLT_TYPE, (a, b) => new AST(Op.ScD, new() { a, b }, env_cnt)),
            (Op.Sub, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Sub, new() { a, b }, env_cnt)),
            (Op.Sub, VEC_TYPE, VEC_TYPE, (a, b) => new AST(Op.Sub, new() { b, a }, env_cnt)),
            */
        }
    );
}