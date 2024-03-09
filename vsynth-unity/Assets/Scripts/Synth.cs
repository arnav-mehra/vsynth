using System;
using System.Collections.Generic;
using UnityEngine;

namespace System.Runtime.CompilerServices { // this fixes a bug w/ 2019 vs and records
    internal static class IsExternalInit {}
}

public class Types {
    public enum Type {
        Vec, Flt
    }

    public record Pair(Type T1, Type T2);
}

public class Ops {
    public enum Op {
        None,
        Add, Sub, Cro, Rot, ScM, ScD,
        Dst, Dot, Mag, FlM, FlD, FlA, FlS
    }

    public static Vector3 Add(Vector3 a, Vector3 b) => a + b;
    public static Vector3 Sub(Vector3 a, Vector3 b) => a - b;

    public static Vector3 Cro(Vector3 a, Vector3 b) => Vector3.Cross(a, b);
    public static Vector3 Rot(Vector3 a, Vector3 b, float c) => Quaternion.AngleAxis(c, b) * a;

    public static Vector3 ScM(Vector3 b, float a) => a * b;
    public static Vector3 ScD(Vector3 b, float a) => (1.0f / a) * b;

    public static float Dst(Vector3 a, Vector3 b) => Vector3.Distance(a, b);
    public static float Dot(Vector3 a, Vector3 b) => Vector3.Dot(a, b);
    public static float Mag(Vector3 a) => Vector3.Magnitude(a);

    public static float FlM(float a, float b) => a * b;
    public static float FlD(float a, float b) => a / b;
    public static float FlA(float a, float b) => a + b;
    public static float FlS(float a, float b) => a - b;
}

public class AST {
    readonly Ops.Op op = Ops.Op.None;
    readonly List<AST> args = null;
    readonly public object val = null;
    public int height = 1;

    public AST(object v) {
        val = v;
    }

    public AST(Ops.Op o, List<AST> a) {
        op = o;
        args = a;
        val = Eval();
        a.ForEach(p => height = Math.Max(height, p.height + 1));
    }

    public object Eval() {
        return op switch {
            Ops.Op.None => val,
            Ops.Op.Add => Ops.Add((Vector3)args[0].val, (Vector3)args[1].val),
            Ops.Op.Sub => Ops.Sub((Vector3)args[0].val, (Vector3)args[1].val),
            Ops.Op.Cro => Ops.Cro((Vector3)args[0].val, (Vector3)args[1].val),
            Ops.Op.Rot => Ops.Rot((Vector3)args[0].val, (Vector3)args[1].val, (float)args[2].val),
            Ops.Op.ScM => Ops.ScM((Vector3)args[0].val, (float)  args[1].val),
            Ops.Op.ScD => Ops.ScD((Vector3)args[0].val, (float)  args[1].val),
            Ops.Op.Dst => Ops.Dst((Vector3)args[0].val, (Vector3)args[1].val),
            Ops.Op.Dot => Ops.Dot((Vector3)args[0].val, (Vector3)args[1].val),
            Ops.Op.Mag => Ops.Mag((Vector3)args[0].val),
            Ops.Op.FlM => Ops.FlM((float)  args[0].val, (float)  args[1].val),
            Ops.Op.FlD => Ops.FlD((float)  args[0].val, (float)  args[1].val),
            Ops.Op.FlA => Ops.FlA((float)  args[0].val, (float)  args[1].val),
            Ops.Op.FlS => Ops.FlS((float)  args[0].val, (float)  args[1].val),
            _ => null
        };
    }

    public Types.Type RetType() => op < Ops.Op.Dst ? Types.Type.Vec : Types.Type.Flt;

    public override string ToString() {
        if (op == Ops.Op.None) {
            return val.ToString().Replace('(', '<').Replace(')', '>');
        } else {
            string s = op + "(";
            foreach (AST a in args) s += a.ToString() + ",";
            return s.TrimEnd(',') + ")";
        }
    }
}

public class Synthesizer {
    static readonly List<float> STD_ANGLES = new() { /*90.0f,*/ 180.0f /*, 270.0f*/ };
    static readonly List<Vector3> STD_VECS = new() { /*Vector3.forward, Vector3.up, Vector3.right*/ };

    static public int VecVecCnt = 0;
    static public int VecFltCnt = 0;
    static public int FltFltCnt = 0;
    static public int VecCnt = 0;

    public List<Vector3> env;

    public Synthesizer(List<Vector3> e) => env = e;

    public List<AST> GenASTs(int depth) {
        List<AST> ls = GenBaseASTs();
        HashSet<object> hs = new();

        for (int d = 2; d <= depth; d++) {
            List<AST> new_ls = new();
            for (int i = 0; i < ls.Count; i++) {
                PushUniOps(new_ls, d, ls[i]);
                for (int j = i + 1; j < ls.Count; j++) {
                    PushBinOps(new_ls, d, ls[i], ls[j]);
                }
            }
            foreach (AST a in new_ls) {
                if (!hs.Contains(a.val)) {
                    hs.Add(a.val);
                    ls.Add(a);
                }
            }
        }

        return ls;
    }

    public List<AST> GenBaseASTs() {
        List<AST> ls = new();
        foreach (Vector3 v in env) ls.Add(new(v));
        foreach (Vector3 v in STD_VECS) ls.Add(new(v));
        return ls;
    }

    public void PushUniOps(List<AST> ls, int depth, AST a1) {
        if (a1.height + 1 != depth) return;

        var _ = a1.RetType() switch {
            (Types.Type.Vec) => PushVecOps(ls, a1),
            _ => 0
        };
    }

    public void PushBinOps(List<AST> ls, int depth, AST a1, AST a2) {
        if (Math.Max(a1.height, a2.height) + 1 != depth) return;

        Types.Pair tp = new(a1.RetType(), a2.RetType());
        var _ = tp switch {
            (Types.Type.Vec, Types.Type.Vec) => PushVecVecOps(ls, a1, a2),
            (Types.Type.Vec, Types.Type.Flt) => PushVecFltOps(ls, a1, a2),
            (Types.Type.Flt, Types.Type.Vec) => PushVecFltOps(ls, a2, a1),
            (Types.Type.Flt, Types.Type.Flt) => PushFltFltOps(ls, a1, a2),
            _ => null
        };
    }

    object PushVecOps(List<AST> ls, AST a1) {
        VecCnt += 1;
        ls.Add(new(Ops.Op.Mag, new() { a1 }));
        return null;
    }

    object PushVecVecOps(List<AST> ls, AST a1, AST a2) {
        VecVecCnt += 5 + STD_ANGLES.Count * 2;
        ls.Add(new(Ops.Op.Add, new() { a1, a2 }));
        ls.Add(new(Ops.Op.Sub, new() { a1, a2 }));
        ls.Add(new(Ops.Op.Sub, new() { a2, a1 }));
        ls.Add(new(Ops.Op.Cro, new() { a1, a2 }));
        ls.Add(new(Ops.Op.Cro, new() { a2, a1 }));

        foreach (float a in STD_ANGLES) {
            ls.Add(new(Ops.Op.Rot, new() { a1, a2, new(a) }));
            ls.Add(new(Ops.Op.Rot, new() { a2, a1, new(a) }));
		}
        return null;
    }

    object PushVecFltOps(List<AST> ls, AST a1, AST a2) {
        VecFltCnt += 1;
        ls.Add(new(Ops.Op.ScM, new() { a1, a2 }));
        //ls.Add(new(Ops.Op.ScD, new() { a1, a2 }));
        return null;
    }

    object PushFltFltOps(List<AST> ls, AST a1, AST a2) {
        FltFltCnt += 3;
        ls.Add(new(Ops.Op.FlM, new() { a1, a2 }));
        ls.Add(new(Ops.Op.FlD, new() { a1, a2 }));
        ls.Add(new(Ops.Op.FlD, new() { a2, a1 }));
        //ls.Add(new(Ops.Op.FlA, new() { a1, a2 }));
        //ls.Add(new(Ops.Op.FlS, new() { a1, a2 }));
        //ls.Add(new(Ops.Op.FlS, new() { a2, a1 }));
        return null;
    }
} 

public class Synth : MonoBehaviour {
	private void Start() {
        var synth = new Synthesizer(new() {
            UnityEngine.Random.insideUnitSphere,
            UnityEngine.Random.insideUnitSphere,
            UnityEngine.Random.insideUnitSphere
        });
        var res = synth.GenASTs(4);

        int vec_ret_cnt = res.FindAll(a => a.RetType() == Types.Type.Vec).Count;
        int flt_ret_cnt = res.Count - vec_ret_cnt;

        string str = (
            "\nAST Count: " + res.Count
            + "\n"
            + "\nVecRetCnt: " + vec_ret_cnt
            + "\nFltRetCnt: " + flt_ret_cnt
            + "\n"
            + "\nVecCnt: "    + Synthesizer.VecCnt
            + "\nVecVecCnt: " + Synthesizer.VecVecCnt
            + "\nVecFltCnt: " + Synthesizer.VecFltCnt
            + "\nFltFltCnt: " + Synthesizer.FltFltCnt
            + "\n"
            + "\nExample ASTs:\n"
        );
        for (int i = 0; i < 3; i++) {
            AST a = res[(int)Math.Floor(UnityEngine.Random.value * res.Count)];
            str += a.ToString() + "\n";
        }
        Debug.Log(str);
    }
}