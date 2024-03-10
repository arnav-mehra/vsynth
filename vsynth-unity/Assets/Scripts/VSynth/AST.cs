using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Assets.Scripts.Ops.Ops;
using static Assets.Scripts.Ops.ComplexityExt;
using static Assets.Scripts.Ops.Types;

public class AST {
    readonly Op op = Op.None;
    readonly List<AST> args = null;
    readonly public object val = null;
    readonly public int complexity = 0;

    public AST(object v) => val = v;

    public AST(Op o, List<AST> a) {
        op = o;
        args = a;
        val = Eval();
        complexity = a.Aggregate(
            o.Complexity(),
            (acc, p) => acc + p.complexity
        );
    }

    public object Eval() {
        return op switch {
            Op.None => val,
            Op.Add => Add((Vector3)args[0].val, (Vector3)args[1].val),
            Op.Sub => Sub((Vector3)args[0].val, (Vector3)args[1].val),
            Op.Cro => Cro((Vector3)args[0].val, (Vector3)args[1].val),
            Op.Rot => Rot((Vector3)args[0].val, (Vector3)args[1].val, (float)args[2].val),
            Op.ScM => ScM((Vector3)args[0].val, (float)  args[1].val),
            Op.ScD => ScD((Vector3)args[0].val, (float)  args[1].val),
            Op.Dst => Dst((Vector3)args[0].val, (Vector3)args[1].val),
            Op.Dot => Dot((Vector3)args[0].val, (Vector3)args[1].val),
            Op.Mag => Mag((Vector3)args[0].val),
            Op.FlM => FlM((float)  args[0].val, (float)  args[1].val),
            Op.FlD => FlD((float)  args[0].val, (float)  args[1].val),
            Op.FlA => FlA((float)  args[0].val, (float)  args[1].val),
            Op.FlS => FlS((float)  args[0].val, (float)  args[1].val),
            _ => null
        };
    }

    public OpType RetType() => op < Op.Dst ? OpType.Vec : OpType.Flt;

    public override string ToString() {
        if (op == Op.None) {
            return val.ToString().Replace('(', '<').Replace(')', '>');
        } else {
            string s = op + "(";
            foreach (AST a in args) s += a.ToString() + ",";
            return s.TrimEnd(',') + ")";
        }
    }
}