using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultBuffer : List<(float out_err, float h_err, AST ast)> {
    public int size;

    public ResultBuffer(int s) : base() { size = s; }

    const int C = 2;
    const float OCCAM_RATIO = 1.5f;

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

    public void DiffSort(List<Example> exs, int target_idx, ProgramBank pb) {
        for (int i = 0; i < Count; i++) {
            var (out_err, _, ast) = this[i];
            var total_h_err = 0.0f;

            exs.ForEach(ex => {
                var target = (Vector3)ex.targets[target_idx];
                var h_err = GradientDescent.Run(ex.env.type, ast, target, pb.var_asts);
                total_h_err += h_err;
            });

            this[i] = (out_err, total_h_err, ast);
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

    public void OutputSort()
    {
        Sort((p1, p2) => p1.out_err.CompareTo(p2.out_err));
        if (Count > size) {
            RemoveRange(size, Count - size);
        }
    }
}

public class Example {
    public EnvType env_type;
    public Env env => Envs.envs[(int)env_type];
    public List<object> targets;
    
    public Example(EnvType et, List<object> t) {
        env_type = et;
        targets = t;
    }
}

public class Search {
    public List<Example> examples;
    public List<ResultBuffer> results; // size = number of targets.
    public int max_results;
    public int max_complexity;

    public Search(List<Example> exs, int max_r = 10, int max_c = 6) {
        max_results = max_r;
        max_complexity = max_c;

        if (exs.Count == 0) {
            examples = new();
            results = new();
            return;
        }

        var inp_cnt = exs[0].env.vars.Count;
        var target_cnt = exs[0].targets.Count;
        examples = exs.FindAll(ex => ex.targets.Count == target_cnt && ex.env.vars.Count == inp_cnt).ToList();
        results = Utils.Range(1, target_cnt).ConvertAll(_ => new ResultBuffer(max_r));
    }

    // find all target asts generating up to max complexity.
    public void FindAllASTs(ProgramGen generator) {
        generator.GenRows(max_complexity);
        Transpose(generator);
    }

    // fill in env search using generator program bank.
    void Transpose(ProgramGen generator) {
        examples.ForEach(e => {
            var env_map = generator.seed.CreateMapping(e.env);

            generator.prg_bank.var_asts.ForEach(a => {
                a.vals[e.env.type] = env_map[a.vals[generator.seed.type]];
                AddAST(a);
            });
        });

        Utils.Range(1, generator.GenComplexity)
             .ForEach(c => TransposeRow(generator, c));
    }

    void TransposeRow(ProgramGen generator, int complexity) {
        generator.prg_bank[complexity].ForEach(a => {
            examples.ForEach(e => a.Eval(e.env.type));
            AddAST(a);
        });
    }

    void AddAST(AST a) {
        for (int i = 0; i < results.Count; i++) {
            var buff = results[i];
            var total_error = 0.0f;

            examples.ForEach(ex => {
                var target = ex.targets[i];
                var key = a.vals[ex.env.type];

                float err = (key, target) switch {
                    (Vector3 v1, Vector3 v2) => Vector3.SqrMagnitude(v1 - v2),
                    (float f1, float f2) => (f1 - f2) * (f1 - f2),
                    _ => float.PositiveInfinity
                };
                err = float.IsFinite(err) ? err : float.PositiveInfinity;
                total_error += err;
            });

            buff.Add(total_error, a);
        }
    }

    public void SortResults(ProgramGen generator) {
        for (int i = 0; i < results.Count; i++) {
            var result = results[i];
            var target_idx = i;
            if (VSynthManager.use_output_error) result.OutputSort();
            else result.DiffSort(examples, target_idx, generator.prg_bank);
        }
    }
}