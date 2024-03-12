using UnityEngine;
using System.Collections.Generic;

using static Assets.Scripts.Test.Test;
using Assets.Scripts.Test;
using Assets.Scripts.LSC;

public class Hello : MonoBehaviour {
    void Start() {
        /*
        var synth = new Synth(
            new() {
                new Vector3(1, 0, 3),
                new Vector3(-1, 4, 2)
            },
            new Vector3(0.5f, 0, 1.5f)
        );
        */
        
        var synth = new Synth(2);

        List<object> env = new() {
            //new Vector3(-0.1003472f, -0.7958283f, -0.5304268f),
            //new Vector3(0.520802f, 0.3288752f, -0.006259157f)
            UnityEngine.Random.insideUnitSphere,
            UnityEngine.Random.insideUnitSphere
        };

        /*List<(int, int)> res = new();
        for (int i = 1; i <= 15; i++) {
            RoundExt.SIG_DIGITS = i;
            int y = TestGen(synth, 6);
            res.Add((i, y));
        }
        string s = "";
        res.ForEach(p => s += "(" + p.Item1 + ", " + p.Item2 + "), ");
        Debug.Log(s);*/
        TestGen(synth, 7);
    }
}