using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultBuffer : List<(float out_err, float h_err, AST ast)> {
    public int size;

    public ResultBuffer(int s) : base() { size = s; }

    const float C_DIFF_MIN_RATIO = 0.95f;

    public void Add(float err, AST ast) {
        Add((err, float.NaN, ast));
        
        for (int i = Count - 1; i >= 1; i--) {
            // avoid over-replacement of lower complexity programs
            bool c_diff = this[i - 1].ast.complexity < this[i].ast.complexity;
            float min_ratio = c_diff ? C_DIFF_MIN_RATIO : 1.0f;
            if (this[i].out_err >= min_ratio * this[i - 1].out_err) break;

            var temp = this[i];
            this[i] = this[i - 1];
            this[i - 1] = temp;
        }

        if (Count > size) RemoveAt(Count - 1);
    }

    public void DiffSort(EnvType et, Vector3 target, ProgramBank pb) {
        for (int i = 0; i < Count; i++) {
            var (out_err, h_err, ast) = this[i];
            var new_h_err = float.IsNaN(h_err) ? Derivative.GradientDescent(et, ast, target, pb.var_asts) : h_err;
            this[i] = (out_err, new_h_err, ast);
        }
        Sort((p1, p2) => p1.h_err.CompareTo(p2.h_err));
    }
}

public class Search {
    public Env env;
    public List<object> targets;
    public List<ResultBuffer> results;
    public int max_results;
    public int max_complexity;

    public Search(Env e, List<object> t, int max_r = 20, int max_c = 5) {
        env = e;
        targets = t;
        results = t.ConvertAll(_ => new ResultBuffer(max_r));
        max_results = max_r;
        max_complexity = max_c;
    }

    // find all target asts generating up to max complexity.
    public void FindAllASTs(ProgramGen generator) {
        generator.GenRows(max_complexity);
        Debug.Log("Generated complexity " + generator.GenComplexity);
        Debug.Log("Generated programs " + generator.seen.Count);
        Transpose(generator);
    }

    // fill in env search using generator program bank.
    void Transpose(ProgramGen generator) {
        var env_map = generator.seed.CreateMapping(env);

        generator.prg_bank.var_asts.ForEach(a => {
            a.vals[env.type] = env_map[a.vals[generator.seed.type]];
            AddAST(a);
        });

        Utils.Range(1, generator.GenComplexity)
             .ForEach(c => TransposeRow(generator, c));
    }

    void TransposeRow(ProgramGen generator, int complexity) {
        generator.prg_bank[complexity].ForEach(a => {
            a.Eval(env.type);
            AddAST(a);
        });
    }

    void AddAST(AST a) {
        if (!a.IsValid(env.type)) return;

        var key = a.vals[env.type];

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];
            var buff = results[i];

            float err = (key, target) switch {
                (Vector3 v1, Vector3 v2) => Vector3.SqrMagnitude(v1 - v2),
                (float f1, float f2) => (f1 - f2) * (f1 - f2),
                _ => float.PositiveInfinity
            };
            if (!float.IsFinite(err)) continue;

            buff.Add(err, a);
        }
    }

    public void SortResults(ProgramGen generator) {
        results.Zip(targets, (r, t) => (r, t)).ToList()
               .ForEach(p => p.r.DiffSort(env.type, (Vector3) p.t, generator.prg_bank));
    }
}