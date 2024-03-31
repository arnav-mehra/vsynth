using System.Collections.Generic;
using UnityEngine;

public class ResultBuffer : List<(float err, AST ast)> {
    public int size;

    public ResultBuffer(int s) : base() { size = s; }

    public void Add(float err, AST ast) {
        Add((err, ast));

        for (int i = size - 1; i >= 1; i--) {
            if (this[i].err >= this[i - 1].err) break;

            var temp = this[i];
            this[i] = this[i - 1];
            this[i - 1] = temp;
        }

        if (Count > size) RemoveAt(Count - 1);
    }

    const float LEARNING_RATE = 0.01f;
    const int MAX_ITERS = 50;

    public void DiffSort(Vector3 target, EnvType et, ProgramBank pb) {
        ForEach(p => {
            var errs = pb.var_asts.ConvertAll(_ => Vector3.zero);
            var vals = pb.var_asts.ConvertAll(a => (Vector3)a.vals[et]);

            Utils.Range(0, vals.Count - 1).ForEach(i => {
                var w = vals[i];
                var err_w = errs[i];
                var w_ast = pb.var_asts[i];

                // C = err_out . err_out + sum(err_vi . err_vi)
                //   = |out - f(v1 + err_v1, v2 + err_v2, ...)|^2 + sum_i(|err_vi|^2)
                // C(err_wx) = |out - f(wx + err_wx)|^2 + err_wx^2 + sum_rem
                // C(err_wy) = |out - f(wy + err_wy)|^2 + err_wy^2 + sum_rem
                // C(err_wz) = |out - f(wz + err_wz)|^2 + err_wz^2 + sum_rem

                // dC/derr_wx = (|out - f(wx + err_wx)|^2)' + (err_wx^2)'
                //            = 2 (out - f(wx + err_wx))' . (out - f(wx + err_wx)) + 2 err_wx
                //            = 2 (-f'(wx + err_wx)) . (out - f(wx + err_wx)) + 2 err_wx
                //            = 2 (err_wx - f'(wx + err_wx) . (out - f(wx + err_wx)))
                // dC/derr_wy = 2 (err_wy - f'(wy + err_wy) . (out - f(wy + err_wy)))
                // dC/derr_wz = 2 (err_wz - f'(wz + err_wz) . (out - f(wz + err_wz)))

                for (int iter = 1; iter <= MAX_ITERS; iter++) {
                    var f = (Vector3) p.ast.vals[et];
                    var fp_x = ((Derivative.FV) p.ast.Diff(EnvType.User, w_ast, 0)).v;
                    var fp_y = ((Derivative.FV) p.ast.Diff(EnvType.User, w_ast, 1)).v;
                    var fp_z = ((Derivative.FV) p.ast.Diff(EnvType.User, w_ast, 2)).v;
                    var delta = target - f;

                    var dC_derr_w = new Vector3(
                        2.0f * (err_w.x - Vector3.Dot(fp_x, delta)),
                        2.0f * (err_w.y - Vector3.Dot(fp_y, delta)),
                        2.0f * (err_w.z - Vector3.Dot(fp_z, delta))
                    );

                    // gradient descent
                    err_w -= LEARNING_RATE * dC_derr_w;

                    // update env and reeval ast
                    w_ast.vals[et] = w + err_w;
                    p.ast.ReEval(et);
                }
            });
        });
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