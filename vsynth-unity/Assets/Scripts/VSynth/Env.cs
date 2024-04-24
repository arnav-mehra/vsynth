using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum EnvType {
    Rand = 0,
    User = 1
}

public class Env {
    public EnvType type;
    public List<object> vars = new();

    public Env(EnvType t) => type = t;

    public Dictionary<object, object> CreateMapping(Env e) => (
        vars.Zip(e.vars, (a, b) => (a, b))
            .ToDictionary(v => v.a, v => v.b)
    );
}

public static class Envs {
    public static Env[] envs = {
        new(EnvType.Rand),
        new(EnvType.User)
    };

    public static Env Rand => envs[(int)EnvType.Rand];
    public static Env User => envs[(int)EnvType.User];

    public static void InitRand(int var_cnt) {
        /*
        Rand.vars = (
            from _ in Utils.Range(1, var_cnt)
            select (object)UnityEngine.Random.insideUnitSphere
        ).ToList();
    */
        Rand.vars = new List<object> {
            new Vector3(1.12587f, 2.6542f, 3.12354f),
            new Vector3(-1.541231f, 1.879123f, -1.12354f),
            new Vector3(2.12354f, 1.12354f, 1.12354f),
        };
        Rand.vars = Rand.vars.Take(var_cnt).ToList();
    }

    public static void InitUser(List<object> u) => User.vars = u;
}