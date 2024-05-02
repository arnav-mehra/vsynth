using System;
using System.Collections.Generic;

public static class Ctors {
    static readonly List<float> STD_ANGLES = new() { 180.0f };
    
    public static List<AST> MakeStdAsts(int env_cnt) => (
        STD_ANGLES.ConvertAll(a => new AST(
            Utils.Range(1, env_cnt)
                 .ConvertAll(_ => (object)a)
        ))
    );

    public static List<(UnOp op, Func<AST, AST> fn)> MakeUnaryCtors(int env_cnt) => (
        new() {
            (Ops.Mag, a => new AST(Ops.Mag, new() { a }, env_cnt)),
            (Ops.Neg, a => new AST(Ops.Neg, new() { a }, env_cnt)),
            (Ops.FlI, a => new AST(Ops.FlI, new() { a }, env_cnt)),
            (Ops.FlN, a => new AST(Ops.FlN, new() { a }, env_cnt)),
        }
    );

    public static List<(BinOp op, Func<AST, AST, AST> fn)> MakeBinaryCtors(int env_cnt) => (
        new() {
            (Ops.Add, (a, b) => new AST(Ops.Add, new() { a, b }, env_cnt)),
            (Ops.Cro, (a, b) => new AST(Ops.Cro, new() { a, b }, env_cnt)),
            (Ops.ScM, (a, b) => new AST(Ops.ScM, new() { a, b }, env_cnt)),
            (Ops.Dot, (a, b) => new AST(Ops.Dot, new() { a, b }, env_cnt)),
            (Ops.FlM, (a, b) => new AST(Ops.FlM, new() { a, b }, env_cnt)),
            (Ops.FlA, (a, b) => new AST(Ops.FlA, new() { a, b }, env_cnt)),

            /*
            (Ops.FlD, (a, b) => new AST(Ops.FlD, new() { a, b }, env_cnt)),
            (Ops.FlS, (a, b) => new AST(Ops.FlS, new() { a, b }, env_cnt)),
            (Ops.ScD, (a, b) => new AST(Ops.ScD, new() { a, b }, env_cnt)),
            (Ops.Sub, (a, b) => new AST(Ops.Sub, new() { a, b }, env_cnt)),
            (Ops.Sub, (a, b) => new AST(Ops.Sub, new() { b, a }, env_cnt)),
            */
        }
    );
}