using System.Collections.Generic;

public class Search {
    public Env env;
    public Dictionary<object, List<AST>> seen = new(new LSC());

    public Search(Env e) => env = e;

    // find target asts using what has been seen so far.
    public (bool all_found, List<List<AST>> asts) FindASTs(List<object> targets) {
        List<List<AST>> asts = targets.ConvertAll(seen.TryGet);
        bool all_found = asts.TrueForAll(a => a != null);
        return (all_found, asts);
    }

    // find all target asts generating up to max complexity.
    public (bool all_found, List<List<AST>> asts) FindAllASTs(ProgramGen generator, List<object> targets, int maxComplexity) {
        Transpose(generator);
        while (generator.GenComplexity < maxComplexity) {
            generator.GenNonBaseRow();
            TransposeRow(generator, generator.GenComplexity);
        }
        return FindASTs(targets);
    }

    // find target asts generating until maxComplexity is reached.
    public (bool all_found, List<List<AST>> asts) FindASTs(ProgramGen generator, List<object> targets, int maxComplexity) {
        Transpose(generator);

        while (generator.GenComplexity < maxComplexity) {
            // check for solution
            var res = FindASTs(targets);
            if (res.all_found) return res;
            // grow + transpose
            generator.GenNonBaseRow();
            TransposeRow(generator, generator.GenComplexity);
        }

        return FindASTs(targets);
    }

    // fill in env search using generator program bank.
    void Transpose(ProgramGen generator) {
        var env_map = generator.seed.CreateMapping(env);

        generator.prg_bank[0].ForEach(a => {
            a.vals[env.type] = env_map[a.vals[generator.seed.type]];
            AddAST(a);
        });

        Utils.ForRange(1, generator.GenComplexity)(c => TransposeRow(generator, c));
    }

    void TransposeRow(ProgramGen generator, int complexity) {
        generator.prg_bank[complexity].ForEach(a => {
            a.Eval(env.type);
            AddAST(a);
        });
    }

    void AddAST(AST a) {
        var key = a.vals[env.type];
        if (seen.ContainsKey(key)) seen[key].Add(a);
        else seen.Add(key, new() { a });
    }
}