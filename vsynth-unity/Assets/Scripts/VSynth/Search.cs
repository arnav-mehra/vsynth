using System.Collections.Generic;
using UnityEngine;

public class Searcher {
    public List<ResultBuffer> results; // size = number of targets.

    public Searcher(Envs envs, int max_r = 10) {
        results = Utils.Range(1, envs.OutCount)
                       .ConvertAll(_ => new ResultBuffer(max_r));
    }

    // find all target asts generating up to max complexity.
    public List<ResultBuffer> FindAll(Envs envs, Generator generator) {
        Transpose(envs.ExampleEnvs, generator);
        return results;
    }

    // fill in env search using generator program bank.
    void Transpose(Envs ex_envs, Generator generator) {
        ex_envs.ExampleEnvs.ForEach(env => {
            var env_map = generator.seed.CreateVarMapping(env);

            generator.program_bank.VarASTs.ForEach(a => {
                a.vals[env.id] = env_map[a.vals[generator.seed.id]];
                AddAST(ex_envs, a);
            });
        });

        Utils.Range(1, generator.GenComplexity)
             .ForEach(c => TransposeRow(ex_envs, generator, c));
    }

    void TransposeRow(Envs ex_envs, Generator generator, int complexity) {
        generator.program_bank[complexity].ForEach(a => {
            ex_envs.ForEach(env => a.Eval(env.id));
            AddAST(ex_envs, a);
        });
    }

    void AddAST(Envs ex_envs, AST a) {
        for (int i = 0; i < results.Count; i++) {
            var buff = results[i];
            var total_error = 0.0f;

            ex_envs.ForEach(env => {
                var target = env.outputs[i];
                var key = a.vals[env.id];

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
}