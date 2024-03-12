using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using static Assets.Scripts.Ops.Ops;
using static Assets.Scripts.Ops.ComplexityExt;
using Assets.Scripts.LSC;
using Assets.Scripts.Ops;

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

public class ProgramBank
{
    static readonly List<float> STD_ANGLES = new() { /*90.0f,*/ 180.0f /*, 270.0f*/ };
    static readonly List<Vector3> STD_ENV = new() { /*Vector3.forward, Vector3.up, Vector3.right*/ };

    public List<Vector3> randomEnv;
    public List<List<AST>> programs;

    public ProgramBank(int var_cnt)
    {
        randomEnv = (from _ in Enumerable.Range(1, var_cnt)
                    select UnityEngine.Random.insideUnitSphere).ToList();

        programs = new List<List<AST>>
        {
            randomEnv.ConvertAll(v => new AST(v))
        };
    }

    public void growTo(int size)
    {
        foreach (int currentSize in Enumerable.Range(programs.Count, size))
        {
            List<(int complexity, Func<AST, AST> constructor)> unaryOps = new()
            {
                (1, ast => new AST(Op.Mag, new() { ast }))
            };

            Type floatTy = typeof(float);
            Type vecTy = typeof(Vector3);
            List<(int complexity, Type t1, Type t2, Func<AST, AST, AST> constructor)> binaryOps = new() {
                (1, vecTy, vecTy, (a, b) => new AST(Op.Add, new() { a, b })),
                (1, vecTy, vecTy, (a, b) => new AST(Op.Sub, new() { a, b })),
                (1, vecTy, vecTy, (a, b) => new AST(Op.Sub, new() { b, a })),
                (1, vecTy, vecTy, (a, b) => new AST(Op.Dot, new() { a, b })),
                (2, vecTy, vecTy, (a, b) => new AST(Op.Cro, new() { a, b })),
                (3, vecTy, vecTy, (a, b) => new AST(Op.Rot, new() { a, b, new(STD_ANGLES[0]) })),
                (3, vecTy, vecTy, (a, b) => new AST(Op.Rot, new() { b, a, new(STD_ANGLES[0]) })),
                (1, vecTy, floatTy, (a, b) => new AST(Op.ScM, new() { a, b })),
                (1, vecTy, floatTy, (a, b) => new AST(Op.ScD, new() { a, b })),
                (1, floatTy, floatTy, (a, b) => new AST(Op.FlM, new() { a, b })),
                (1, floatTy, floatTy, (a, b) => new AST(Op.FlD, new() { a, b })),
                (1, floatTy, floatTy, (a, b) => new AST(Op.FlA, new() { a, b })),
                (1, floatTy, floatTy, (a, b) => new AST(Op.FlS, new() { a, b }))
            };


            var newUnaryASTs = from op in unaryOps
                              from ast in programs[currentSize - op.complexity]
                              select op.constructor(ast);

            var newBinaryASTs = from op in binaryOps
                                from lsize in Enumerable.Range(1, currentSize - op.complexity)
                                from rsize in new List<int> { currentSize - lsize - op.complexity }
                                from l in programs[lsize - 1]
                                from r in programs[rsize - 1]
                                select op.constructor(l, r);

            programs.Add(newUnaryASTs.Concat(newBinaryASTs).ToList());
        }
    }

    public List<AST> findASTs(List<object> targets, List<object> userEnv, int maxComplexity)
    {
        while (programs.Count <= maxComplexity)
        {
            growTo(programs.Count + 1);
            // find good programs
            // return matching 
        }

        return new List<AST>();
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