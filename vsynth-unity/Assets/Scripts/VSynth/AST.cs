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

    // if val is null, eval and return, otherwise return val
    public object Val
    {
        get
        {
            val ??= Eval();
            return val;
        }
    }
    public ASTCore(Op o, List<AST> a) {
        op = o;
        args = a;
        val = Eval();
    }

    public object Eval() => op switch {
        Op.None => val,
        Op.Add => Add((Vector3)args[0].Val, (Vector3)args[1].Val),
        Op.Sub => Sub((Vector3)args[0].Val, (Vector3)args[1].Val),
        Op.Cro => Cro((Vector3)args[0].Val, (Vector3)args[1].Val),
        Op.Rot => Rot((Vector3)args[0].Val, (Vector3)args[1].Val, (float)args[2].Val),
        Op.ScM => ScM((Vector3)args[0].Val, (float)  args[1].Val),
        Op.ScD => ScD((Vector3)args[0].Val, (float)  args[1].Val),
        Op.Dst => Dst((Vector3)args[0].Val, (Vector3)args[1].Val),
        Op.Dot => Dot((Vector3)args[0].Val, (Vector3)args[1].Val),
        Op.Mag => Mag((Vector3)args[0].Val),
        Op.FlM => FlM((float)  args[0].Val, (float)  args[1].Val),
        Op.FlD => FlD((float)  args[0].Val, (float)  args[1].Val),
        Op.FlA => FlA((float)  args[0].Val, (float)  args[1].Val),
        Op.FlS => FlS((float)  args[0].Val, (float)  args[1].Val),
        _ => null
    };

    public object TransposedEval(Dictionary<object, object> env_map) => op switch
    {
        Op.None => env_map.ContainsKey(val) ? env_map[val] : val,
        Op.Add => Add((Vector3)args[0].TransposedEval(env_map), (Vector3)args[1].TransposedEval(env_map)),
        Op.Sub => Sub((Vector3)args[0].TransposedEval(env_map), (Vector3)args[1].TransposedEval(env_map)),
        Op.Cro => Cro((Vector3)args[0].TransposedEval(env_map), (Vector3)args[1].TransposedEval(env_map)),
        Op.Rot => Rot((Vector3)args[0].TransposedEval(env_map), (Vector3)args[1].TransposedEval(env_map), (float)args[2].TransposedEval(env_map)),
        Op.ScM => ScM((Vector3)args[0].TransposedEval(env_map), (float)args[1].TransposedEval(env_map)),
        Op.ScD => ScD((Vector3)args[0].TransposedEval(env_map), (float)args[1].TransposedEval(env_map)),
        Op.Dst => Dst((Vector3)args[0].TransposedEval(env_map), (Vector3)args[1].TransposedEval(env_map)),
        Op.Dot => Dot((Vector3)args[0].TransposedEval(env_map), (Vector3)args[1].TransposedEval(env_map)),
        Op.Mag => Mag((Vector3)args[0].TransposedEval(env_map)),
        Op.FlM => FlM((float)args[0].TransposedEval(env_map), (float)args[1].TransposedEval(env_map)),
        Op.FlD => FlD((float)args[0].TransposedEval(env_map), (float)args[1].TransposedEval(env_map)),
        Op.FlA => FlA((float)args[0].TransposedEval(env_map), (float)args[1].TransposedEval(env_map)),
        Op.FlS => FlS((float)args[0].TransposedEval(env_map), (float)args[1].TransposedEval(env_map)),
        _ => null
    };

    public object TransposedEvalImmut(Dictionary<object, object> env_map) => (
        null
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