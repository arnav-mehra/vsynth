using UnityEngine;

public class UI : MonoBehaviour {
    [SerializeField] private GameObject ResultPrefab;

	private void Start() {
        VSynthManager.result_prefab = ResultPrefab;

	    Inputs.InitKeys(new() {
            (key: OVRInput.RawButton.RIndexTrigger, onDown: VecManager.StartVector, onUp: VecManager.EndVector),
            (key: OVRInput.RawButton.B, onDown: VecManager.CancelVector, onUp: null),
            (key: OVRInput.RawButton.A, onDown: VecManager.ToggleIsInput, onUp: null),
        });
        Inputs.InitButtons(new() {
            (tag: "generate-button", onClick: VSynthManager.OnGenerate),
            (tag: "clear-button", onClick: VecManager.ClearVectors),
            (tag: "new-example-button", onClick: VecManager.StartExample),
        });
        Inputs.InitSliders(new() {
            (tag: "complexity-slider", onSlide: VSynthManager.OnComplexityChange),
            (tag: "num-results-slider", onSlide: VSynthManager.OnNumResultsChange)
        });
	}

	void Update() {
        Inputs.Hands.OnFrame();
        VecManager.OnFrame();
        VSynthManager.OnFrame();
        Inputs.OnFrame();
    }
}