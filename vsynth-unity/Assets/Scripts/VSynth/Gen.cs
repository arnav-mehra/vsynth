using System.Linq;
using System.Collections.Generic;

public class ProgramBank : List<List<AST>> {
    public int GenComplexity => Count - 1;

    public List<AST> var_asts => this[0];
}

public class ProgramDict : Dictionary<object, AST> {
    public ProgramDict() : base(new LSC()) {}
}

public class ProgramGen {
    public ProgramBank prg_bank = new(); // program table. row idx = complexity.
    public ProgramDict seen = new();     // hashmap for program uniqueness.
    public Env seed = null;              // seed for program key values.

    public int GenComplexity => prg_bank.GenComplexity;

    public ProgramGen(Env e) => seed = e;

    public void GenRow() {
        List<AST> genASTs = GenComplexity switch {
            -1 => GenBaseRow(),
            _  => GenNonBaseRow()
        };
        var newASTs = genASTs.FindAll(
            a => a.IsValid(seed.type)
                 && seen.TryAdd(a.vals[seed.type], a)
        );
        prg_bank.Add(newASTs);
    }

    public void GenRows(int complexity) {
        Utils.Range(GenComplexity + 1, complexity)
             .ForEach(_ => GenRow());
    }

    List<AST> GenBaseRow() => (
        seed.vars.ConvertAll(v => new AST(seed.type, v))
    );

    List<AST> GenNonBaseRow() {
        int targetComplexity = GenComplexity + 1;

        var genUnaryASTs = from ctor in ASTCtors.UNARY_CTORS
                           from ast in prg_bank[targetComplexity - ctor.op.Complexity()]
                               where ast.RetType == ctor.t
                           select ctor.fn(ast);

        var genBinaryASTs = from ctor in ASTCtors.BINARY_CTORS
                            from l_complexity in Utils.Range(0, targetComplexity - ctor.op.Complexity())
                            let r_complexity = targetComplexity - l_complexity - ctor.op.Complexity()
                            from l in prg_bank[l_complexity]
                                where l.RetType == ctor.t1
                            from r in prg_bank[r_complexity]
                                where r.RetType == ctor.t2
                            select ctor.fn(l, r);

        return genUnaryASTs.Concat(genBinaryASTs).ToList();
    }

    public List<AST> GetAll() => (
        (from row in prg_bank
         from ast in row
         select ast).ToList()
    );
}