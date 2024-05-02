using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class AST {
    public Op op;
    protected List<AST> args;
    readonly public int complexity;
    public object[] vals;

    public AST(Op o, List<AST> a, int env_cnt) {
        op = o;
        args = a;
        vals = new object[env_cnt];
        complexity = a.Aggregate(o.Complexity(), (t, ch) => t + ch.complexity);
    }

    public AST(object val, int env_id, int env_cnt) {
        op = Op.None;
        vals[env_id] = val;
        vals = new object[env_cnt];
        complexity = 0;
    }

    public AST(List<object> _vals) {
        op = Op.None;
        vals = _vals.ToArray();
        complexity = 0;
    }

    public object Eval(int env_id) => vals[env_id] = op switch {
        Op.None => vals[env_id],
        Op op => op.Eval(args.ConvertAll(a => a.vals[env_id]))
    };

    public object ReEval(int env_id) => vals[env_id] = op switch {
        Op.None => vals[env_id],
        Op op => op.Eval(args.ConvertAll(a => a.ReEval(env_id)))
    };

    public Derivative Diff(int env_id, AST wrt, int coord) => op switch {
        Op.None when vals[env_id] is Vector3 && wrt != this => new Derivative.FV(Vector3.zero),
        Op.None when vals[env_id] is Vector3 && coord == 0  => new Derivative.FV(new(1.0f, 0.0f, 0.0f)),
        Op.None when vals[env_id] is Vector3 && coord == 1  => new Derivative.FV(new(0.0f, 1.0f, 0.0f)),
        Op.None when vals[env_id] is Vector3 && coord == 2  => new Derivative.FV(new(0.0f, 0.0f, 1.0f)),
        Op op => op.Diff(env_id, args, wrt, coord)
    };

    public bool IsValid(int env_id) => vals[env_id] switch {
        Vector3 v => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.y),
        float f => float.IsFinite(f),
        _ => false
    };

    public override string ToString() => op switch {
        Op.None => vals.First().ToString().Replace('(', '<').Replace(')', '>'),
        Op.Mag => "|" + args[0] + "|",
        Op.FlI => "(1 / " + args[0] + ")",
        Op.FlN => "(-" + args[0] + ")",
        Op.Neg => "(-" + args[0] + ")",
        Op.Dst => "|"  + args[0] + " - " + args[1] + "|",
        Op op  => "("  + args[0] + " " + op.Str() + " " + args[1] + ")",
	};

    public string ToCode() => op switch {
        Op.None => vals.First().ToString().Replace('(', '<').Replace(')', '>'),
        Op.Mag => "Vector3.Magnitude(" + args[0].ToCode() + ")",
        Op.Dst => "Vector3.Distance("  + args[0].ToCode() + ", " + args[1].ToCode() + ")",
        Op.Cro => "Vector3.Cross("     + args[0].ToCode() + ", " + args[1].ToCode() + ")",
        Op.Dot => "Vector3.Dot("       + args[0].ToCode() + ", " + args[1].ToCode() + ")",
        Op.FlI => "(1.0f / " + args[0].ToCode() + ")",
        Op.FlN => "(-" + args[0].ToCode() + ")",
        Op.FlM => "(" + args[0].ToCode() + " * " + args[1].ToCode() + ")",
        Op.FlD => "(" + args[0].ToCode() + " / " + args[1].ToCode() + ")",
        Op.Neg => "(-" + args[0].ToCode() + ")",
        Op.ScM => "(" + args[0].ToCode() + " * " + args[1].ToCode() + ")",
        Op.ScD => "(" + args[0].ToCode() + " / " + args[1].ToCode() + ")",
        Op op => "(" + args[0].ToCode() + " " + op.Str() + " " + args[1].ToCode() + ")"
	};
}