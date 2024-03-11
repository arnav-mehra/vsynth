using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Test {
    public class Timer {
        public static System.Diagnostics.Stopwatch sw;

        public static void Start() {
            sw = System.Diagnostics.Stopwatch.StartNew();
        }

        public static long End() {
            return sw.ElapsedMilliseconds;
        }
    }

	public class Test {
		public static void TestFind(Synth synth, int complexity) {
            Timer.Start();
            var (res, ls) = synth.FindAST(complexity);
            float t = Timer.End() / 1000.0f;

            var (err, best_ast) = ls
                .FindAll(a => a.val is Vector3 && synth.target is Vector3)
                .ConvertAll(a => (Vector3.Distance((Vector3)a.val, (Vector3)synth.target), a))
                .Max();

            Debug.Log(
                "Found: " + (res != null)
                + "\nASTs searched: " + ls.Count
                + "\nBest AST: " + synth.StringifyAST(best_ast)
                + "\nComplexity: " + best_ast.complexity
                + "\nErr: " + err
                + "\n"
                + "\nElapsed seconds: " + t
                + "\nPerformance: " + (ls.Count / t) + " ASTs/s"
                + "\n"
            );
        }

        public static int TestGen(Synth synth, int complexity) {
            Timer.Start();
            var res = synth.GenASTs(complexity);
            float t = Timer.End() / 1000.0f;

            int vec_ret_cnt = res.FindAll(a => a.val is Vector3).Count;
            int flt_ret_cnt = res.Count - vec_ret_cnt;

            Debug.Log(
                "\nAST Count: " + res.Count
                + "\n"
                + "\nVecRetCnt: " + vec_ret_cnt
                + "\nFltRetCnt: " + flt_ret_cnt
                + "\n"
                + "\nVecCnt: "    + synth.VecCnt
                + "\nVecVecCnt: " + synth.VecVecCnt
                + "\nVecFltCnt: " + synth.VecFltCnt
                + "\nFltFltCnt: " + synth.FltFltCnt
                + "\n"
                + "\nElapsed seconds: " + t
                + "\nPerformance: " + (res.Count / t) + " ASTs/s"
                + "\n"
            );

            //str += "\nExample ASTs:\n";
            //for (int i = 0; i < res.Count; i++) {
            //    AST a = res[(int)Math.Floor(UnityEngine.Random.value * res.Count)];
            //    AST a = res[i];
            //    str += a.complexity + ": " + synth.StringifyAST(a) + "\n";
            //}
            return res.Count;
        }

        public void PrintVec(Vector3 v) {
            Debug.Log("new Vector3(" + v.x + "f, " + v.y + "f, " + v.z + "f)");
        }
	}
}