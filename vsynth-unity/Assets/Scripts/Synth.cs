using System;
using System.Collections;
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
    object val;
    Ops.Op op;
    List<AST> args;
    public int height;

    public AST(object v) {
        val = v;
        op = Ops.Op.None;
        height = 1;
    }

    public AST(Ops.Op o, List<AST> a) {
        op = o;
        args = a;

        height = 0;
        foreach (AST p in a) height = Math.Max(height, p.height);
        height += 1;
    }

    public object Eval() {
        return op switch {
            Ops.Op.None => val,
            Ops.Op.Add => Ops.Add((Vector3)args[0].Eval(), (Vector3)args[1].Eval()),
            Ops.Op.Sub => Ops.Sub((Vector3)args[0].Eval(), (Vector3)args[1].Eval()),
            Ops.Op.Cro => Ops.Cro((Vector3)args[0].Eval(), (Vector3)args[1].Eval()),
            Ops.Op.Dot => Ops.Dot((Vector3)args[0].Eval(), (Vector3)args[1].Eval()),
            Ops.Op.Dst => Ops.Dst((Vector3)args[0].Eval(), (Vector3)args[1].Eval()),
            Ops.Op.Mag => Ops.Mag((Vector3)args[0].Eval()),
            Ops.Op.Rot => Ops.Rot((Vector3)args[0].Eval(), (Vector3)args[1].Eval(), (float)args[2].Eval()),
            Ops.Op.ScM => Ops.ScM((Vector3)args[0].Eval(), (float)args[1].Eval()),
            Ops.Op.ScD => Ops.ScD((Vector3)args[0].Eval(), (float)args[1].Eval()),
            _ => null
        };
    }

    public Types.Type RetType() => op < Ops.Op.Dst ? Types.Type.Vec : Types.Type.Flt;

    public override string ToString() {
        if (op == Ops.Op.None) {
            return "AST { val: " + val.ToString() + " }";
        } else {
            string s = "AST { op: " + op.ToString() + ", args: [\n";
            foreach (AST a in args) s += "\t" + a.ToString() + "\n";
            return s + "\n] }";
        }
    }
}

public class Synthesizer {
    static List<float> STD_ANGLES = new() { 90.0f, 180.0f, 270.0f };
    static List<Vector3> STD_VECS = new() { Vector3.forward, Vector3.up, Vector3.right };

    public List<Vector3> env;

    public Synthesizer(List<Vector3> e) => env = e;

    public List<AST> GenASTs(int depth) {
        List<AST> ls = GenBaseASTs();
        for (int d = 1; d <= depth; d++) {
            int len = ls.Count;
            for (int i = 0; i < len; i++) {
                PushUniOps(ls, d, ls[i]);
                for (int j = i + 1; j < len; j++) {
                    PushBinOps(ls, d, ls[i], ls[j]);
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
            _ => 0
        };
    }

    int PushVecOps(List<AST> ls, AST a1) {
        ls.Add(new(Ops.Op.Mag, new() { a1 }));
        return 0;
    }

    int PushVecVecOps(List<AST> ls, AST a1, AST a2) {
        ls.Add(new(Ops.Op.Add, new() { a1, a2 }));
        ls.Add(new(Ops.Op.Sub, new() { a1, a2 }));
        ls.Add(new(Ops.Op.Sub, new() { a2, a1 }));
        ls.Add(new(Ops.Op.Cro, new() { a1, a2 }));
        ls.Add(new(Ops.Op.Cro, new() { a2, a1 }));

        foreach (float a in STD_ANGLES) {
            ls.Add(new(Ops.Op.Rot, new() { a1, a2, new(a) }));
            ls.Add(new(Ops.Op.Rot, new() { a2, a1, new(a) }));
		}
        return 0;
    }

    int PushVecFltOps(List<AST> ls, AST a1, AST a2) {
        ls.Add(new(Ops.Op.ScM, new() { a1, a2 }));
        ls.Add(new(Ops.Op.ScD, new() { a1, a2 }));
        return 0;
    }

    int PushFltFltOps(List<AST> ls, AST a1, AST a2) {
        ls.Add(new(Ops.Op.FlM, new() { a1, a2 }));
        ls.Add(new(Ops.Op.FlD, new() { a1, a2 }));
        ls.Add(new(Ops.Op.FlD, new() { a2, a1 }));
        ls.Add(new(Ops.Op.FlA, new() { a1, a2 }));
        ls.Add(new(Ops.Op.FlS, new() { a1, a2 }));
        ls.Add(new(Ops.Op.FlS, new() { a2, a1 }));
        return 0;
    }
} 

public class Synth : MonoBehaviour {
	private void Start() {
        //AST a = new(Vector3.one);
        //AST b = new(Vector3.one);
        //AST c = new(Ops.Op.Add, new() { a, b });
        //object res = c.Eval().ToString();
        //Debug.Log(res);

        var s = new Synthesizer(new() { Vector3.one, Vector3.one });
        var res = s.GenASTs(2);
        Debug.Log(res.Count);   
        foreach (AST a in res) {
            Debug.Log(a.Eval());
        }
	}
}