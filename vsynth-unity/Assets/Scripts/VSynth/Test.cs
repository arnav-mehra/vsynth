using System.Collections.Generic;
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
		public static void TestFind(Synth synth, List<object> targets, List<object> env, int complexity) {
            Timer.Start();
            var ls = synth.FindAST(targets, env, complexity);
            float t = Timer.End() / 1000.0f;

            string s = "";
            targets.Zip(ls, (a, b) => (a, b)).ToList().ForEach(p => {
                s += "\nTarget: " + p.a
                   + "\nFound: " + (p.b == null ? "Nope." :  synth.StringifyAST(p.b))
                   + "\n";
            });
            s += "\nElapsed seconds: " + t
               + "\nPerformance: " + (ls.Count / t) + " ASTs/s"
               + "\n";

            Debug.Log(s);
        }

        public static int TestGen(Synth synth, int complexity) {
            Timer.Start();
            synth.GenASTs(complexity);
            float t = Timer.End() / 1000.0f;

            var res = synth.res.table.Aggregate((acc, ls) => acc.Concat(ls).ToList());
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
                + "\nList: " + synth.res.table.Aggregate("", (acc, ls) => acc + ls.Count + ", ")
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