using System;
using UnityEngine;

using static Assets.Scripts.Ops.Types;

namespace Assets.Scripts.Test {
	public class Test {
		public static void TestFind(Synth synth, int complexity) {
            var (res, ls) = synth.FindAST(complexity);

            ValueTuple<float, AST> best = (float.PositiveInfinity, new(null));
            ls.ForEach(a => {
                if (a.val.GetType().Equals(synth.target.GetType())
                    && a.RetType() == OpType.Vec) {
                    float terr = Vector3.Distance((Vector3)a.val, (Vector3)synth.target);
                    if (terr < best.Item1) best = (terr, a);
                }
            });

            Debug.Log(
                "Found: " + (res != null)
                + "\nASTs searched: " + ls.Count
                + "\nBest AST: " + synth.StringifyAST(best.Item2)
                + "\nComplexity: " + best.Item2.complexity
                + "\nErr: " + best.Item1
                + "\n"
            );
        }

        public static void TestGen(Synth synth, int complexity) {
            var res = synth.GenASTs(complexity);
            int vec_ret_cnt = res.FindAll(a => a.RetType() == OpType.Vec).Count;
            int flt_ret_cnt = res.Count - vec_ret_cnt;

            string str = (
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
                + "\nExample ASTs:\n"
            );
            for (int i = 0; i < res.Count; i++) {
                //AST a = res[(int)Math.Floor(UnityEngine.Random.value * res.Count)];
                AST a = res[i];
                str += a.complexity + ": " + synth.StringifyAST(a) + "\n";
            }
            Debug.Log(str);
        }
	}
}