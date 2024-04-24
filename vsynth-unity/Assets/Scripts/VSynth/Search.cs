using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultBuffer : List<(float out_err, float h_err, AST ast)> {
    public int size;

    public ResultBuffer(int s) : base() { size = s; }

    const int C = 2;
    const float OCCAM_RATIO = 2.0f;

    public void Add(float err, AST ast) {
        Add((err, float.NaN, ast));
        
        for (int i = Count - 1; i >= 1; i--) {
            // avoid over-replacement of lower complexity programs
            float adj_curr_err = this[i].out_err * Mathf.Pow(OCCAM_RATIO, this[i].ast.complexity);
            float adj_prev_err = this[i - 1].out_err * Mathf.Pow(OCCAM_RATIO, this[i - 1].ast.complexity);
            if (adj_prev_err <= adj_curr_err) break;

            var temp = this[i];
            this[i] = this[i - 1];
            this[i - 1] = temp;
        }

        if (Count > size * C) RemoveAt(Count - 1);
    }

    public void DiffSort(EnvType et, Vector3 target, ProgramBank pb) {
        for (int i = 0; i < Count; i++) {
            var (out_err, h_err, ast) = this[i];
            var new_h_err = float.IsNaN(h_err) ? GradientDescent.Run(et, ast, target, pb.var_asts) : h_err;
            this[i] = (out_err, new_h_err, ast);
        }
        Sort((p1, p2) => {
            float adj_p1_err = p1.h_err * Mathf.Pow(OCCAM_RATIO, p1.ast.complexity);
            float adj_p2_err = p2.h_err * Mathf.Pow(OCCAM_RATIO, p2.ast.complexity);
            return adj_p1_err.CompareTo(adj_p2_err);
        });
        if (Count > size) {
            RemoveRange(size, Count - size);
        }
    }
}

public class Search {
    public Env env;
    public List<object> targets;
    public List<ResultBuffer> results;
    public int max_results;
    public int max_complexity;

    public Search(Env e, List<object> t, int max_r = 10, int max_c = 6) {
        env = e;
        targets = t;
        results = t.ConvertAll(_ => new ResultBuffer(max_r));
        max_results = max_r;
        max_complexity = max_c;
    }

    // find all target asts generating up to max complexity.
    public void FindAllASTs(ProgramGen generator) {
        generator.GenRows(max_complexity);
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

    public override string ToString()
    {
        return "Env: " + env;
    }
}