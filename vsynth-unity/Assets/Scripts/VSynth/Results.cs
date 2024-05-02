using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultBuffer : List<(float out_err, float h_err, AST ast)> {
    const int BUFFER_SCALAR = 10;
    const float OCCAM_RATIO = 1.5f;
    
    public int max_results;
    public bool hypothesize_computed = false;

    public ResultBuffer(int s) : base() => max_results = s;

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

        if (Count > max_results * BUFFER_SCALAR) RemoveAt(Count - 1);
    }

    public void ComputeHypothesizedErr(Envs ex_envs, Generator generator, int target_idx) {
        for (int i = 0; i < Count; i++) {
            var (out_err, _, ast) = this[i];
            var total_h_err = 0.0f;

            ex_envs.ForEach(env => {
                var target = (Vector3)env.outputs[target_idx];
                var h_err = GradientDescent.Run(env.id, ast, target, generator.program_bank.VarASTs);
                total_h_err += h_err;
            });

            this[i] = (out_err, total_h_err, ast);
        }
    }

    public ResultBuffer GetHypothesizedSorted(Envs ex_envs, int target_idx, Generator generator) {
        if (!hypothesize_computed) {
            ComputeHypothesizedErr(ex_envs, generator, target_idx);
        }
        return (ResultBuffer)(
            this.OrderBy(p => p.h_err * Mathf.Pow(OCCAM_RATIO, p.ast.complexity))
                .ToList()
                .SubArray(0, max_results)
        );
    }

    public ResultBuffer GetOutputSorted() => (
        (ResultBuffer) this.SubArray(0, max_results)
    );
}
