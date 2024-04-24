using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Test {
    /*public static void GD(List<object> user_env, Vector3 target) {
        Envs.InitRand(user_env.Count);
        Envs.InitUser(user_env);

        AST i1 = new(new object[] { Envs.Rand.vars[0], user_env[0] });
        AST i2 = new(new object[] { Envs.Rand.vars[1], user_env[1] });
        AST t1 = new(Ops.Op.Dot, new() { i1, i2 });
        AST t2 = new(Ops.Op.Mag, new() { i1 });
        AST t3 = new(Ops.Op.FlD, new() { t1, t2 });
        AST ast = new(Ops.Op.ScD, new() { i2, t3 });
        ast.ReEval(EnvType.User);

        // Necessary learning rate for various compositions.
        // 0.1: add, sub, cro, mag + scm
        // 0.01: dot + scm, dot + scd, dot + mag + fla/s + scm
        // 0.001: dot + mag + flm/d + scm

        GradientDescent.Run(EnvType.User, ast, target, new() { i1, i2 });
    }*/

	public static void Find(List<Example> examples, int complexity) {
        ProgramGen generator = new(Envs.Rand);
        Search search = new(examples, 20, complexity);

        Utils.Timer.Start();
        search.FindAllASTs(generator);
        search.SortResults(generator);
        float t = Utils.Timer.End() / 1000.0f;

        Debug.Log(
            search.results.Aggregate("", (acc, r) =>
                acc
                + "\nASTs Found:" + r.ToString(search)
                + "\nErr Inversions: " + r.CountInversions()
            )
            + "\nElapsed seconds: " + t
            + "\nASTs Generated: " + generator.seen.Count
            + "\nPerformance: " + generator.seen.Count / t + " ASTs/s"
            + "\n"
        );
    }

    public static int Gen(int complexity) {
        Envs.InitRand(2);

        ProgramGen generator = new(Envs.Rand);

        Utils.Timer.Start();
        generator.GenRows(complexity);
        float t = Utils.Timer.End() / 1000.0f;

        var res = generator.GetAll();
        int vec_ret_cnt = res.FindAll(a => a.vals[EnvType.User1] is Vector3).Count;
        int flt_ret_cnt = res.Count - vec_ret_cnt;
        string ls_str = generator.prg_bank.Aggregate("", (acc, ls) => acc + ", " + ls.Count.ToString());

        Debug.Log(
            "\nAST Count: " + res.Count
            + "\n"
            + "\nVecRetCnt: " + vec_ret_cnt
            + "\nFltRetCnt: " + flt_ret_cnt
            + "\n"
            + "\nList: " + ls_str
            + "\n"
            + "\nElapsed seconds: " + t
            + "\nPerformance: " + (res.Count / t) + " ASTs/s"
            + "\n"
        );

        return res.Count;
    }

    public void PrintVec(Vector3 v) {
        Debug.Log("new Vector3(" + v.x + "f, " + v.y + "f, " + v.z + "f)");
    }
}