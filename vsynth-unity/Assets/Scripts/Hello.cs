using System.Collections.Generic;
using UnityEngine;

public class Hello : MonoBehaviour {
    void Start() {
        var ex1 = new Example(
            new() {
                new Vector3(1, 0, 3),
                new Vector3(-1, 4, 2)
            },
            new() {
                new Vector3(0.5f, 0, 1.5f)
            }
        );

        var ex2 = new Example(
            new() {
                new Vector3(1, 0, 3),
                new Vector3(1, -4, -2)
            },
            new() {
                new Vector3(-0.5f, 0, -1.5f)
            }
        );

        List<Example> exs = new() { ex1, ex2 };
        Test.Find(exs, 8);

        //Test.GD(user_env, targets);

        /*List<(int, int)> res = new();
        for (int i = 1; i <= 15; i++) {
            RoundExt.SIG_DIGITS = i;
            int y = TestGen(synth, 6);
            res.Add((i, y));
        }
        string s = "";
        res.ForEach(p => s += "(" + p.Item1 + ", " + p.Item2 + "), ");
        Debug.Log(s);*/
    }
}