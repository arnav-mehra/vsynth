using System;
using System.Linq;
using System.Collections.Generic;

public class ProgramBank : List<List<AST>> {
    public int GenComplexity => Count - 1;

    public List<AST> var_asts => this[0];
}

public class ProgramDict : Dictionary<object, AST> {
    public ProgramDict() : base(new LSC()) {}
}

public class Generator {
    public Env seed;                     // seed for program key values.
    public ProgramBank prg_bank = new(); // program table. row idx = complexity.
    public ProgramDict seen = new();     // hashmap for program uniqueness.
    public List<(Op op, Type t, Func<AST, AST> fn)> unary_ctors;
    public List<(Op op, Type t1, Type t2, Func<AST, AST, AST> fn)> binary_ctors;

    public int GenComplexity => prg_bank.GenComplexity;

    public Generator(Envs envs) {
        seed = envs[envs.AddRand()];
        unary_ctors = Ctors.MakeUnaryCtors(envs.Count);
        binary_ctors = Ctors.MakeBinaryCtors(envs.Count);
    }

    public void GenRow(Envs envs) {
        var genASTs = GenComplexity switch {
            -1 => GenBaseRow(envs),
            _  => GenNonBaseRow()
        };
        var newASTs = genASTs.Where(
            a => a.IsValid(seed.id)
                 && seen.TryAdd(a.vals[seed.id], a)
        );
        prg_bank.Add(newASTs.ToList());
    }

    public void GenRows(Envs envs, int complexity) {
        Utils.Range(GenComplexity + 1, complexity)
             .ForEach(_ => GenRow(envs));
    }

    IEnumerable<AST> GenBaseRow(Envs envs) => (
        seed.inputs.ConvertAll(val => new AST(val, seed.id, envs.Count))
    );

    IEnumerable<AST> GenNonBaseRow() {
        int targetComplexity = GenComplexity + 1;

        var genUnaryASTs = from ctor in unary_ctors
                           from complexity in Utils.Range(0, targetComplexity - ctor.op.Complexity())
                           from ast in prg_bank[complexity]
                               where ast.op.RetType() == ctor.t
                           select ctor.fn(ast);

        var genBinaryASTs = from ctor in binary_ctors
                            from l_complexity in Utils.Range(0, targetComplexity - ctor.op.Complexity())
                            let r_complexity = targetComplexity - l_complexity - ctor.op.Complexity()
                            from l in prg_bank[l_complexity]
                                where l.op.RetType() == ctor.t1
                            from r in prg_bank[r_complexity]
                                where r.op.RetType() == ctor.t2
                            select ctor.fn(l, r);

        return genUnaryASTs.Concat(genBinaryASTs);
    }

    public List<AST> GetAll() => (
        (from row in prg_bank
         from ast in row
         select ast).ToList()
    );
}