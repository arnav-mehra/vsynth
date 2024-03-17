using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Assets.Scripts.Ops.Ops;
using static Assets.Scripts.Ops.ComplexityExt;

public abstract class ASTCore {
    protected Op op = Op.None;
    protected List<AST> args = null;
    public object val = null;
    public object error = null;

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
        error = o == Op.None ? null : 0.0;
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


    // we dont rly need a derivative class, delete it if u want but it's not terrible
    // at first I thoguht it was a good idea because we could diff wrt vectors and floats
    // and FF was float wrt vector, FV was vector wrt vector, and VV was vector wrt vector
    // but we only diff wrt floats so the only types for derivatives are float and vector
    // but having the static methods in the derivative class is nice and I think it reduces
    // casting
    public abstract class Derivative
    {
        // float derivatives 
        public class FF : Derivative
        {
            public float v;
            public FF(float v) => this.v = v;

            public static FF Mag(ASTCore v, ASTCore wrt)
            {
                FV dv = (FV)v.D(wrt);
                Vector3 dvec = dv.v;
                // d/dv sqrt(x(v)^2 + y(v)^2 + z(v)^2) -> wolfram alpha
                // (x(v)*x'(v) + y(v)*y'(v) + z(v)*z'(v)) / sqrt(x(v)^2 + y(v)^2 + z(v)^2)

                float xp = dvec.x;
                float yp = dvec.y;
                float zp = dvec.z;
                float x = v.Val.x;
                float y = v.Val.y;
                float z = v.Val.z;
                return new FF((x * xp + y * yp + z * zp) / Mathf.Sqrt(x * x + y * y + z * z));
            }
        }

        public class FV : Derivative
        {
            public Vector3 v;
            public FV(Vector3 v) => this.v = v;
            public static FV Add(ASTCore a, ASTCore b, ASTCore wrt)
            {
                FV d0 = (FV) a.D(wrt);
                FV d1 = (FV) b.D(wrt);
                return new FV(d0.v + d1.v);
            }

            public static FV ScM(ASTCore a, ASTCore b, ASTCore wrt)
            {
                FV d0 = (FV) a.D(wrt);
                FF d1 = (FF) b.D(wrt);
                return new FV(d0.v * d1.v);
            }
        }

        // probably kill
        public class VV : Derivative
        {
            public Vector3[] vs;
        }
    }

    // assume wrt val is a float because we only diff wrt floats
    public Derivative D(ASTCore wrt) => op switch
    {
        Op.None when val is float => new Derivative.FF(wrt == this ? 1.0f : 0.0f),
        Op.Add => Derivative.FV.Add(args[0], args[1], wrt),
        Op.ScM => Derivative.FV.ScM(args[0], args[1], wrt),
        Op.Mag => Derivative.FF.Mag(args[0], wrt),
        _ => null // todo everything else
    };

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