using System.Collections.Generic;
using UnityEngine;

public class ResultBuffer : List<(float err, AST a)> {
    public int size;

    public ResultBuffer(int s) : base() { size = s; }

    public void Add(float err, AST a) {
        Add((err, a));

        for (int i = size - 1; i >= 1; i--) {
            if (this[i].err >= this[i - 1].err) break;

            var temp = this[i];
            this[i] = this[i - 1];
            this[i - 1] = temp;
        }

        if (Count > size) RemoveAt(Count - 1);
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
        Transpose(generator);
    }

    // fill in env search using generator program bank.
    void Transpose(ProgramGen generator) {
        var env_map = generator.seed.CreateMapping(env);

        generator.prg_bank[0].ForEach(a => {
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
        var key = a.vals[env.type];

        for (int i = 0; i < targets.Count; i++) {
            var target = targets[i];
            var buff = results[i];

            float err = (key, target) switch {
                (Vector3 v1, Vector3 v2) => Vector3.SqrMagnitude(v1 - v2),
                (float   f1, float   f2) => (f1 - f2) * (f1 - f2),
                _ => float.PositiveInfinity
            };
            buff.Add(err, a);
        }
    }
}