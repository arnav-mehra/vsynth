using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Test {
	public static void Find(List<object> targets, List<object> user_env, int complexity) {
        Envs.InitRand(user_env.Count);
        Envs.InitUser(user_env);

        ProgramGen generator = new(Envs.Rand);
        Search search = new(Envs.User, targets, 1, complexity);

        Utils.Timer.Start();
        search.FindAllASTs(generator);
        float t = Utils.Timer.End() / 1000.0f;

        var res = search.results;
        string s = "";
        /*targets.Zip(res.asts, (target, asts) => (target, asts)).ToList().ForEach(p => {
            var asts_str = (
                p.asts == null
                    ? "None."
                    : p.asts.ConvertAll(ast => ast.complexity + ": " + Utils.StringifyAST(search, ast))
                            .Aggregate((acc, ast) => acc + ",\n\t" + ast)
            );
            s += "\nTarget: " + p.target
                + "\nASTs Found:\n\t" + asts_str
                + "\n";
        });
        s += "\nElapsed seconds: " + t
            + "\nPerformance: " + (res.asts.Count / t) + " ASTs/s"
            + "\n";*/

        Debug.Log(s);
    }

    public static int Gen(int complexity) {
        Envs.InitRand(2);

        ProgramGen generator = new(Envs.Rand);

        Utils.Timer.Start();
        generator.GenRows(complexity);
        float t = Utils.Timer.End() / 1000.0f;

        var res = generator.GetAll();
        int vec_ret_cnt = res.FindAll(a => a.vals[EnvType.User] is Vector3).Count;
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