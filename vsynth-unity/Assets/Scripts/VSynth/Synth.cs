using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using static Assets.Scripts.Ops.Ops;
using static Assets.Scripts.Ops.ComplexityExt;
using static Assets.Scripts.Ops.Types;

public class Synth {
    static readonly List<float> STD_ANGLES = new() { /*90.0f,*/ 180.0f /*, 270.0f*/ };
    static readonly List<Vector3> STD_VECS = new() { /*Vector3.forward, Vector3.up, Vector3.right*/ };

    public int VecVecCnt = 0;
    public int VecFltCnt = 0;
    public int FltFltCnt = 0;
    public int VecCnt = 0;

    public List<Vector3> env;
    public object target;

    public Synth(List<Vector3> e, object t = null) {
        target = t;
        env = e;
    }

    public ValueTuple<AST, List<AST>> FindAST(int complexity) {
        List<AST> ls = GenASTs(complexity);
        bool lastIsCorrect = (ls.Count != 0) && ls.Last().val.Equals(target);
        AST res = lastIsCorrect ? ls.Last() : null;
        return (res, ls);
    }

    public List<AST> GenASTs(int complexity) {
        List<AST> ls = GenBaseASTs();
        HashSet<object> seen = new();
        List<int> c_idx = new() { 0, ls.Count };

        for (int c = 1; c <= complexity; c++) {
            List<AST> new_ls = new();

            // push all ops that achieve complexity c.
            for (int op_c = MIN_OP_C; op_c <= MAX_OP_C; op_c++) {
                int ch_c = c - op_c; // children complexity.
                if (ch_c < 0) continue;

                for (int i = c_idx[ch_c]; i < c_idx[ch_c + 1]; i++) {
                    PushUniOps(new_ls, ls[i], op_c);
                }
                for (int i = c_idx[0]; i < c_idx[ch_c + 1]; i++) {
                    int j_ch_c = ch_c - ls[i].complexity;
                    if (j_ch_c < 0) continue;
                    for (int j = c_idx[j_ch_c]; j < c_idx[j_ch_c + 1]; j++) {
                        PushBinOps(new_ls, ls[i], ls[j], op_c);
                    }
                }
            }

            // filter out repeats, search for result.
            foreach (AST a in new_ls) {
                if (a.val.Equals(target)) {
                    ls.Add(a);
                    return ls;
                }
                if (!seen.Contains(a.val)) {
                    seen.Add(a.val);
                    ls.Add(a);
                }
            }

            c_idx.Add(ls.Count);
        }

        return ls;
    }

    List<AST> GenBaseASTs() {
        List<AST> ls = new();
        foreach (Vector3 v in env) ls.Add(new(v));
        foreach (Vector3 v in STD_VECS) ls.Add(new(v));
        return ls;
    }

    void PushUniOps(List<AST> ls, AST a1, int ch_c) {
        var _ = a1.RetType() switch {
            OpType.Vec => PushVecOps(ls, a1, ch_c),
            _ => null
        };
    }

    void PushBinOps(List<AST> ls, AST a1, AST a2, int ch_c) {
        var _ = (a1.RetType(), a2.RetType()) switch {
            (OpType.Vec, OpType.Vec) => PushVecVecOps(ls, a1, a2, ch_c),
            (OpType.Vec, OpType.Flt) => PushVecFltOps(ls, a1, a2, ch_c),
            (OpType.Flt, OpType.Vec) => PushVecFltOps(ls, a2, a1, ch_c),
            (OpType.Flt, OpType.Flt) => PushFltFltOps(ls, a1, a2, ch_c),
            _ => null
        };
    }

    object PushVecOps(List<AST> ls, AST a1, int ch_c) {
        switch (ch_c) {
            case 1: {
                VecCnt += 1;
                ls.Add(new(Op.Mag, new() { a1 }));
                break;
            }
        }
        return null;
    }

    object PushVecVecOps(List<AST> ls, AST a1, AST a2, int ch_c) {
        switch (ch_c) {
            case 1: {
                VecVecCnt += 4;
                ls.Add(new(Op.Add, new() { a1, a2 }));
                ls.Add(new(Op.Sub, new() { a1, a2 }));
                ls.Add(new(Op.Sub, new() { a2, a1 }));
                ls.Add(new(Op.Dot, new() { a1, a2 }));
                break;
            }
            case 2: {
                VecVecCnt += 2;
                ls.Add(new(Op.Cro, new() { a1, a2 }));
                ls.Add(new(Op.Cro, new() { a2, a1 }));
                break;
            }
            case 3: {
                VecVecCnt += STD_ANGLES.Count * 2;
                foreach (float a in STD_ANGLES) {
                    ls.Add(new(Op.Rot, new() { a1, a2, new(a) }));
                    ls.Add(new(Op.Rot, new() { a2, a1, new(a) }));
		        }
                break;
            }
        }
        return null;
    }

    object PushVecFltOps(List<AST> ls, AST a1, AST a2, int ch_c) {
        switch (ch_c) {
            case 1: {
                VecFltCnt += 1;
                ls.Add(new(Op.ScM, new() { a1, a2 }));
                break;
            }
        }
        return null;
    }

    object PushFltFltOps(List<AST> ls, AST a1, AST a2, int ch_c) {
        float v1 = (float)a1.val;
        float v2 = (float)a2.val;
        switch (ch_c) {
            case 1: {
                FltFltCnt += 3;
                ls.Add(new(Op.FlM, new() { a1, a2 }));
                if (v1 != 0) ls.Add(new(Op.FlD, new() { a2, a1 }));
                if (v2 != 0) ls.Add(new(Op.FlD, new() { a1, a2 }));
                break;
            }
        }
        return null;
    }

    public string StringifyAST(AST a) {
        string s = a.ToString();
        for (int i = 0; i < env.Count(); i++) {
            char c = (char)(i + 'a');
            string v = env[i].ToString().Replace('(', '<').Replace(')', '>');
            s = s.Replace(v, c.ToString());
        }
        return s;
    }
}