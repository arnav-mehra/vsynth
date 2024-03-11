using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Assets.Scripts.Ops.Ops;
using static Assets.Scripts.Ops.ComplexityExt;

public abstract class ASTCore {
    protected Op op = Op.None;
    protected List<AST> args = null;
    public object val = null;

    public ASTCore(object v) => val = v;

    public ASTCore(Op o, List<AST> a) {
        op = o;
        args = a;
        val = Eval();
    }

    public object Eval() => op switch {
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

    public object TransposedEval(Dictionary<object, object> env_map) => (
        val = op switch {
            Op.None => env_map.ContainsKey(val) ? env_map[val] : val,
            _ => Eval()
        }
    );

    public override string ToString() => op switch {
        Op.None => val.ToString().Replace('(', '<').Replace(')', '>'),
        _ => op + "(" + args.Select(a => a.ToString()).Aggregate((a, b) => a + ", " + b) + ")"
	};
}

public class AST : ASTCore {
    readonly public int complexity = 0;

    public AST(object v) : base(v) {}

    public AST(Op o, List<AST> a) : base(o, a) {
        complexity = a.Aggregate(
            o.Complexity(),
            (acc, p) => acc + p.complexity
        );
    }
}