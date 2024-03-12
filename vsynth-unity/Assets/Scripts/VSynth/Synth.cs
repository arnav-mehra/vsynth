using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using static Assets.Scripts.Ops.Ops;
using static Assets.Scripts.Ops.ComplexityExt;
using Assets.Scripts.LSC;

public class SynthResults {
    public List<List<AST>> table = new();
    public Dictionary<object, AST> seen = new(new LSC());

    public SynthResults() {}

    public List<int> RowLens {
        get => table.ConvertAll(row => row.Count);
    }

    public void AddRow(List<AST> ls) {
        table.Add(ls.Where(a => seen.TryAdd(a.val, a)).ToList());
    }

    public int Rows {
        get => table.Count;
    }

    public void Transpose(List<object> old_env, List<object> new_env) {
        Dictionary<object, object> env_map = old_env
            .Zip(new_env, (a, b) => (a, b))
            .ToDictionary(v => v.a, v => v.b);

        seen.Clear();
        table.ForEach(row =>
            row.ForEach(a =>
                seen.TryAdd(a.TransposedEval(env_map), a)
            )
        );
    }
}

public class Synth {
    static readonly List<float> STD_ANGLES = new() { /*90.0f,*/ 180.0f /*, 270.0f*/ };
    static readonly List<object> STD_ENV = new() { /*Vector3.forward, Vector3.up, Vector3.right*/ };

    // stats
    public int VecVecCnt = 0;
    public int VecFltCnt = 0;
    public int FltFltCnt = 0;
    public int VecCnt = 0;

    public List<object> env = new();
    public SynthResults res = new();

    public Synth(int var_cnt) {
        env = Enumerable.Range(1, var_cnt).ToList()
            .ConvertAll(v => (object)UnityEngine.Random.insideUnitSphere);
    }

    public List<AST> FindAST(List<object> targets, List<object> user_env, int complexity) {
        GenASTs(complexity);
        res.Transpose(env, user_env);
        return targets.ConvertAll(target =>
            res.seen.ContainsKey(target) ? res.seen[target] : null
        );
    }

    public void GenASTs(int complexity) {
        for (int c = res.Rows; c <= complexity; c++) {
            if (c == 0) {
                res.AddRow(GenBaseASTs());
                continue;
            }

            List<AST> new_row = new();

            // push all ops that achieve complexity c.
            for (int op_c = MIN_OP_C; op_c <= Math.Min(MAX_OP_C, c); op_c++) {
                int children_c = c - op_c; // children complexity.
                
                res.table[children_c].ForEach(a => PushUniOps(new_row, a, op_c));
                res.table.Take(children_c + 1).ToList().ForEach(row => {
                    row.ForEach(a => {
                        int rem_child_c = children_c - a.complexity;
                        res.table[rem_child_c].ForEach(b => PushBinOps(new_row, a, b, op_c));
                    });
                });
            }

            res.AddRow(new_row);
        }
    }

    List<AST> GenBaseASTs() => (
        env.Concat(STD_ENV).ToList().ConvertAll(v => new AST(v))
    );

    void PushUniOps(List<AST> ls, AST a1, int ch_c) {
        var _ = a1.val switch {
            Vector3 => PushVecOps(ls, a1, ch_c),
            _ => null
        };
    }

    void PushBinOps(List<AST> ls, AST a1, AST a2, int ch_c) {
        var _ = (a1.val, a2.val) switch {
            (Vector3, Vector3) => PushVecVecOps(ls, a1, a2, ch_c),
            (Vector3, float  ) => PushVecFltOps(ls, a1, a2, ch_c),
            (float,   Vector3) => PushVecFltOps(ls, a2, a1, ch_c),
            (float,   float  ) => PushFltFltOps(ls, a1, a2, ch_c),
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