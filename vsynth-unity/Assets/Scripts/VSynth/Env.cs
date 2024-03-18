using System.Linq;
using System.Collections.Generic;

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
        Rand.vars = (
            from _ in Utils.Range(1, var_cnt)
            select (object)UnityEngine.Random.insideUnitSphere
        ).ToList();
    }

    public static void InitUser(List<object> u) => User.vars = u;
}