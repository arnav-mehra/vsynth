using System.Collections.Generic;

public class VSynthManager {
    public static ProgramGen generator = null;
    public static int max_complexity = 1;
    public static int max_results = 1;

    public static void OnComplexityChange(float c) {
        max_complexity = (int)c;
    }

    public static void OnNumResultsChange(float r) {
        max_results = (int)r;
    }

    public static void OnGenerate() {
        List<object> inputs = VecManager.GetVectors(true);
        List<object> outputs = VecManager.GetVectors(false);

        if (generator == null || generator.seed.vars.Count != inputs.Count) {
            Envs.InitRand(inputs.Count);
            generator = new(Envs.Rand);
        }
        generator.GenRows(max_complexity);
        
        Envs.InitUser(inputs);
        Search search = new(Envs.User, outputs, max_results, max_complexity);
        search.FindAllASTs(generator);
    }
}