using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using TMPro;

public class VSynthManager {
    public static GameObject result_prefab = null;

    public static Synthesizer synthesizer = null;
    public static int max_complexity = 5;
    public static int max_results = 10;
    public static bool use_output_error = false;
    
    public static List<(GameObject toggler, List<DrawnVector> vecs)> result_objects = new();
    public static Searcher search = null;
    public static bool results_changed = false;

    public static void OnComplexityChange(float c) {
        var v = (int)c;
        max_complexity = v;
        var go = GameObject.FindGameObjectWithTag("complexity-slider-text");
        go.GetComponent<TextMeshProUGUI>().text = v.ToString();
    }

    public static void OnNumResultsChange(float r) {
        var v = (int)r;
        max_results = v;
        var go = GameObject.FindGameObjectWithTag("num-results-slider-text");
        go.GetComponent<TextMeshProUGUI>().text = v.ToString();
    }

    public static void OnGenerate() {
        List<List<object>> inputs = VecManager.GetVectors(true);
        List<List<object>> outputs = VecManager.GetVectors(false);

        List<Example> exs = inputs.Zip(outputs, (ins, outs) => (ins, outs))
                                  .Select(p => new Example(p.ins, p.outs))
                                  .ToList();
        synthesizer = new(exs, max_results, max_complexity);

        new Thread(() => {
            Thread.CurrentThread.IsBackground = true;
            Generate();
        }).Start();
    }

    public static void Generate() {
        synthesizer.Run();
        search = synthesizer.searcher;
        results_changed = true;
    }

    public static void OnFrame() {
        if (results_changed) {
            results_changed = false;
            ViewResults();
        }
    }

    static void ViewResults() {
        ClearResults();

        var parent = GameObject.FindGameObjectWithTag("result-box");

        StringBuilder outputStr = new();

        search.results.ForEach(results => {
            results.ForEach(r => {
                var (out_err, h_err, ast) = r;

                outputStr.AppendLine("Output Error: " + out_err + " Drawing Error: " + h_err);
                outputStr.Append(Utils.CodifyAST(synthesizer.generator, ast) + "\n\n");

                var vecs = synthesizer.envs.ExampleEnvs.Select(env => {
                    var vec = new DrawnVector();
                    var origin = VecManager.origins[env.id];
                    vec.SetPointPos(0, origin);
                    vec.SetPointPos(1, origin + (Vector3) ast.vals[env.id]);
                    vec.color = Color.green;
                    return vec;
                }).ToList();

                var go = GameObject.Instantiate(result_prefab);
                go.GetComponentInChildren<Text>().text = Utils.StringifyAST(synthesizer.generator, ast);
                go.GetComponentInChildren<Toggle>().onValueChanged
                  .AddListener(is_shown => vecs.ForEach(vec => vec.shown = is_shown));
                go.transform.SetParent(parent.transform, false);
                result_objects.Add((go, vecs));
            });
        });

        File.WriteAllBytes("output.txt", Encoding.ASCII.GetBytes(outputStr.ToString()));
    }

    public static void ClearResults() {
        result_objects.ForEach(o => {
            o.toggler.SetActive(false);
            o.vecs.ForEach(vec => vec.Destroy());
        });
        result_objects.Clear();
	}
}