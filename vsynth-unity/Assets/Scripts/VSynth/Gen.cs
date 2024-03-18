using System.Linq;
using System.Collections.Generic;

public class ProgramBank : List<List<AST>> {}

public class ProgramGen {
    public ProgramBank prg_bank = new();
    public Env seed; // basis env used for program uniqueness.
    public Dictionary<object, AST> seen = new(new LSC()); // locality-sensitive hashmap used for program uniqueness

    public int GenComplexity { get => prg_bank.Count - 1; }

    public ProgramGen(Env e) {
        seed = e;
        var genASTs = seed.vars.ConvertAll(v => new AST(seed.type, v));
        AddRow(genASTs);
    }

    public void GenRows(int complexity) {
        Utils.ForRange(GenComplexity + 1, complexity)(_ => GenNonBaseRow());
    }

    public void GenBaseRow() {
        var genASTs = seed.vars.ConvertAll(v => new AST(seed.type, v));
        AddRow(genASTs);
    }

    public void GenNonBaseRow() {
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

        var genASTs = genUnaryASTs.Concat(genBinaryASTs).ToList();
        AddRow(genASTs);
    }

    public List<AST> GetAll() => (
        (from row in prg_bank
         from ast in row
         select ast).ToList()
    );

    void AddRow(List<AST> genASTs) {
        var newASTs = genASTs.FindAll(a => seen.TryAdd(a.vals[seed.type], a));
        prg_bank.Add(newASTs);
    }
}