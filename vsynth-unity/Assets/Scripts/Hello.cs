using UnityEngine;

using static Assets.Scripts.Test.Test;
using Assets.Scripts.Test;

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
        
        var synth = new Synth(
            new() {
                UnityEngine.Random.insideUnitSphere,
                UnityEngine.Random.insideUnitSphere
            },
            null
        );

        TestGen(synth, 7);
    }
}