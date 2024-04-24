using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum EnvType {
    Rand = 0,
    User1 = 1,
    User2 = 2,
    User3 = 3,
    User4 = 4,
    User5 = 5,
    User6 = 6
}

public class Env {
    public EnvType type;
    public List<object> vars = new();

    public Env(EnvType t) => type = t;
    public Env(EnvType t, List<object> vs) {
        type = t;
        vars = vs;
    }

    public Dictionary<object, object> CreateMapping(Env e) => (
        vars.Zip(e.vars, (a, b) => (a, b))
            .ToDictionary(v => v.a, v => v.b)
    );
}

public static class Envs {
    public static List<Env> envs = new() {
        new(EnvType.Rand)
    };

    public static Env Rand => envs[(int)EnvType.Rand];

    public static void InitUserSets(List<List<object>> input_sets) {
        envs.AddRange(
            Utils.Range(0, input_sets.Count - 1)
                 .Select(i => new Env((EnvType)(i + 1), input_sets[i]))
        );
    }

    public static void InitUser(List<object> input_set) {
        envs.Add(new Env((EnvType)envs.Count, input_set));
    }

    public static void InitRand(int var_cnt) {
        Rand.vars = (
            from _ in Utils.Range(1, var_cnt)
            select (object)Random.insideUnitSphere
        ).ToList();
    }
}