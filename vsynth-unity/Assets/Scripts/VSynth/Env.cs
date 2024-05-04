using System.Linq;
using System.Collections.Generic;

public class Env {
    public int id;
    public bool is_example;
    public List<object> inputs;
    public List<object> outputs;

    public Env(int _id, bool _is_example, List<object> _inputs, List<object> _outputs) {
        id = _id;
        is_example = _is_example;
        inputs = _inputs;
        outputs = _outputs;
    }

    public Dictionary<object, object> CreateVarMapping(Env e) => (
        inputs.Zip(e.inputs, (a, b) => (a, b))
              .ToDictionary(v => v.a, v => v.b)
    );
}

public class Envs : List<Env> {
    public int InCount => Count == 0 ? 0 : this.First().inputs.Count;
    public int OutCount => Count == 0 ? 0 : this.First().outputs.Count;
    
    public Envs ExampleEnvs => (Envs)FindAll(env => env.is_example);

    public Envs(List<Example> exs) : base() {
        exs.ForEach(ex => AddExample(ex));
    }

    public int Add(bool is_example, List<object> inputs, List<object> outputs) {
        int id = Count;
        Add(new(id, is_example, inputs, outputs));
        return id;
    }

    public Option<int> AddExample(Example ex) => (
        IsValid(ex.inputs, ex.outputs) switch {
            true => Option<int>.Some(Add(true, ex.inputs, ex.outputs)),
            false => Option<int>.None
        }
    );

    public int AddRand() => Add(
        false,
        Utils.Range(1, InCount)
             .Select(_ => (object)UnityEngine.Random.insideUnitSphere)
             .ToList(),
        new()
    );

    public bool IsValid(List<object> inputs, List<object> outputs) => (
        Count == 0 || (inputs.Count == InCount && (outputs.Count == OutCount))
    );
}

public class Example {
    public List<object> inputs;
    public List<object> outputs;

    public Example(List<object> ins, List<object> outs) {
        inputs = ins;
        outputs = outs;
    }
}