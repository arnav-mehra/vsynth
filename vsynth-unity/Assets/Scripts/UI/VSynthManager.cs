using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VSynthManager {
    public static ProgramGen generator = null;
    public static int max_complexity = 1;
    public static int max_results = 1;

    public static void OnComplexityChange(float c) {
        var v = (int)c;
        max_complexity = v;
        GameObject.FindGameObjectWithTag("complexity-slider-text")
                  .GetComponent<TextMeshProUGUI>().text = v.ToString();
    }

    public static void OnNumResultsChange(float r) {
        var v = (int)r;
        max_results = v;
        GameObject.FindGameObjectWithTag("num-results-slider-text")
                  .GetComponent<TextMeshProUGUI>().text = v.ToString();
    }

    public static void OnGenerate() {
        DebugText.Set("Generating programs");
        return;

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