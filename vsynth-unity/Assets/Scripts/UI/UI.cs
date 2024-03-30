using UnityEngine;

public class UI : MonoBehaviour {
	private void Start() {
	    Inputs.InitKeys(new() {
            (key: OVRInput.RawButton.RIndexTrigger, onDown: VecManager.StartVector, onUp: VecManager.EndVector),
            (key: OVRInput.RawButton.B, onDown: VecManager.CancelVector, onUp: null),
            (key: OVRInput.RawButton.A, onDown: VecManager.ToggleIsInput, onUp: null),
        });
        Inputs.InitButtons(new() {
            (tag: "generate-button", onClick: VSynthManager.OnGenerate),
            (tag: "clear-button", onClick: VecManager.ClearVectors)
        });
        Inputs.InitSliders(new() {
            (tag: "complexity-slider", onSlide: VSynthManager.OnComplexityChange),
            (tag: "num-results-slider", onSlide: VSynthManager.OnNumResultsChange)
        });
	}

	void Update() {
        Inputs.Hands.OnFrame();
        VecManager.OnFrame();
        Inputs.OnFrame();
    }
}