using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Threading;
using System;
using System.Text;
using UnityEngine.Windows;

public class VSynthManager {
    public static GameObject result_prefab = null;

    public static ProgramGen generator = null;
    public static int max_complexity = 5;
    public static int max_results = 10;
    
    public static List<(GameObject toggler, DrawnVector vec)> result_objects = new();
    public static Search search = null;
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
        List<object> inputs = new()
        {
            new Vector3(1, 0, 3),
            new Vector3(-1, 4, 2)
        };
        List<object> outputs = new()
        {
            new Vector3(0.5f, 0, 1.5f)
        };
        //List<object> inputs = VecManager.GetVectors(true);
        //List<object> outputs = VecManager.GetVectors(false);
        /*Debug.Log("Generating programs " + inputs.Aggregate((acc, i) => acc + i.ToString())  + " " + outputs.Aggregate((acc, i) => acc + i.ToString()));
        */

        if (generator == null || generator.seed.vars.Count != inputs.Count) {
            Envs.InitRand(inputs.Count);    
            generator = new(Envs.Rand);
        }
        
        new Thread(() => {
            Thread.CurrentThread.IsBackground = true;
            Generate(inputs, outputs);
        }).Start();
        
        //Generate(inputs, outputs);
    }

    public static void Generate(List<object> user_env, List<object> targets, List<object> inputs, List<object> outputs)
    {
        Envs.InitRand(user_env.Count);
        Envs.InitUser(user_env);

    }

    public static void Generate(List<object> inputs, List<object> outputs) {
        /*
        List<object> user_env = new() {
            UnityEngine.Random.insideUnitSphere,
            UnityEngine.Random.insideUnitSphere
        };
        List<object> targets = new() {
            UnityEngine.Random.insideUnitSphere
		};

        Envs.InitRand(user_env.Count);
        Envs.InitUser(user_env);

        generator = new(Envs.Rand);
        search = new(Envs.User, targets, 8, 3);
        search.FindAllASTs(generator); // crashes
        */

        /*Debug.Log(
            targets.Zip(search.results, (target, results) => (target, results)).ToList()
                   .Aggregate("", (acc, p) =>
                acc + "\nTarget: " + p.target
                    + "\nASTs Found:" + p.results.ToString(search)
                    + "\nErr Inversions: " + p.results.CountInversions()
            )
            + "\nPerformance: " + generator.seen.Count + " ASTs/s"
            + "\n"
        );*/
        // sc generator.GenRows(max_complexity);
        /*Debug.Log("Programs generated: " + generator.seen.Count);*/

        /*List<object> user_env = new() {
            new Vector3(1, 0, 3),
            new Vector3(-1, 4, 2)
        };
        List<object> targets = new() {
            new Vector3(0.5f, 0, 1.5f)
        };

        Test.Find(targets, user_env, 8);*/


        /*Envs.InitRand(inputs.Count);
        Envs.InitUser(inputs);
        search = new(Envs.User1, outputs, max_results, max_complexity);
        Debug.Log("init search");
        search.FindAllASTs(generator);
        Debug.Log("search: " + search.ToString() + " " + generator.prg_bank.ToString());
        Debug.Log("Found ASTS: " + search.results.Count);
        for (int i = 0; i < search.results.Count; i++)
        {
            Debug.Log("Results: " + search.results[i]);
        }
        results_changed = true;*/
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
            Debug.Log("results: " + results.Count);
            results.ForEach(r => {
                var (out_err, h_err, ast) = r;

                outputStr.AppendLine("Output Error: " + out_err + " H Error: " + h_err);
                outputStr.Append(ast.ToCode() + "\n\n");

                var origin = Vector3.up * 0.5f;
                /*var vec = new DrawnVector();
                vec.SetPointPos(0, origin);
                vec.SetPointPos(1, origin + (Vector3) ast.vals[EnvType.User1]);
                vec.color = Color.green;*/

                var go = GameObject.Instantiate(result_prefab);
                go.GetComponentInChildren<Text>().text = Utils.StringifyAST(search, ast);
                /*go.GetComponentInChildren<Toggle>().onValueChanged
                  .AddListener(is_shown => vec.shown = is_shown);*/
                go.transform.SetParent(parent.transform, false);

                //DebugText.Set("Adding AST: " + r.ast.ToString());
                /*result_objects.Add((go, vec));*/
            });
        });

        File.WriteAllBytes("output.txt", Encoding.ASCII.GetBytes(outputStr.ToString()));
    }

    static void ClearResults() {
        result_objects.ForEach(o => {
            o.toggler.SetActive(false);
            o.vec.Destroy();
        });
        result_objects.Clear();
	}
}