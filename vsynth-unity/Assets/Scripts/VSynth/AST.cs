using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using static Ops;

public abstract class ASTCore {
    public Op op;
    protected List<AST> args;

    public ASTCore(Op o, List<AST> a) {
        op = o;
        args = a;
    }
}

public class ASTValues {
    public object[] vals = { null, null };

    public object this[EnvType et] {
        get { return vals[(int)et]; }
        set { vals[(int)et] = value; }
    }
}

public abstract class ASTValued : ASTCore {
    public ASTValues vals = new();

    public ASTValued(EnvType et, object v) : base(Op.None, null) => vals[et] = v;

    public ASTValued(object[] ls) : base(Op.None, null) => vals.vals = ls;

    public ASTValued(Op o, List<AST> a) : base(o, a) => Eval(EnvType.Rand);

    public Type RetType => vals[EnvType.Rand].GetType();
    
    public object Eval(EnvType e) => vals[e] = op switch {
        Op.None => vals[e],
        Op op => op.Eval(args.ConvertAll(a => a.vals[e]))
    };

    public object ReEval(EnvType e) => vals[e] = op switch {
        Op.None => vals[e],
        Op op => op.Eval(args.ConvertAll(a => a.ReEval(e)))
    };

    public Derivative Diff(EnvType et, AST wrt, int coord) => op switch {
        Op.None when vals[et] is Vector3 && wrt != this => new Derivative.FV(Vector3.zero),
        Op.None when vals[et] is Vector3 && coord == 0  => new Derivative.FV(new(1.0f, 0.0f, 0.0f)),
        Op.None when vals[et] is Vector3 && coord == 1  => new Derivative.FV(new(0.0f, 1.0f, 0.0f)),
        Op.None when vals[et] is Vector3 && coord == 2  => new Derivative.FV(new(0.0f, 0.0f, 1.0f)),
        Op op => op.Diff(et, args, wrt, coord)
    };
}

public class AST : ASTValued {
    readonly public int complexity = 0;

    public AST(EnvType et, object v) : base(et, v) {}

    public AST(object[] ls) : base(ls) {}

    public AST(Op o, List<AST> a) : base(o, a) {
        complexity = a.Aggregate(o.Complexity(), (t, ch) => t + ch.complexity);
    }

    public override string ToString() => op switch {
        Op.None => vals[EnvType.User].ToString().Replace('(', '<').Replace(')', '>'),
        Op.Dst => "|" + args[0] + " - " + args[1] + "|",
        Op.Mag => "|" + args[0] + "|",
        Op op => "(" + args[0] + " " + op.Str() + " " + args[1] + ")",
	};
}