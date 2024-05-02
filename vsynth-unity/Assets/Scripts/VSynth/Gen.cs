using System;
using System.Linq;
using System.Collections.Generic;

public class ProgramBank : List<List<AST>> {
    public int GenComplexity => Count - 1;

    public List<AST> VarASTs => this[0];
}

public class ProgramDict : Dictionary<object, AST> {
    public ProgramDict() : base(new LSC()) {}
}

public class Generator {
    public Env seed;                     // seed for program key values.
    public ProgramBank program_bank = new(); // program table. row idx = complexity.
    public ProgramDict seen = new();     // hashmap for program uniqueness.
    public List<(UnOp op, Func<AST, AST> fn)> unary_ctors;
    public List<(BinOp op, Func<AST, AST, AST> fn)> binary_ctors;

    public int GenComplexity => program_bank.GenComplexity;

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
        program_bank.Add(newASTs.ToList());
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

        var gen_unary_asts = from ctor in unary_ctors
                             from complexity in Utils.Range(0, targetComplexity - ctor.op.Complexity)
                             from ast in program_bank[complexity]
                                 where ast.op.RetType == ctor.op.InputType
                             select ctor.fn(ast);

        var gen_binary_asts = from ctor in binary_ctors
                              from l_complexity in Utils.Range(0, targetComplexity - ctor.op.Complexity)
                              let r_complexity = targetComplexity - l_complexity - ctor.op.Complexity
                              from l in program_bank[l_complexity]
                                  where l.op.RetType == ctor.op.InputTypes._1
                              from r in program_bank[r_complexity]
                                  where r.op.RetType == ctor.op.InputTypes._2
                              select ctor.fn(l, r);

        return gen_unary_asts.Concat(gen_binary_asts);
    }

    public List<AST> GetAll() => (
        (from row in program_bank
         from ast in row
         select ast).ToList()
    );
}