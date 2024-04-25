using UnityEngine;
using UnityEngine.UI;

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

        Toggle output_error_checkbox = GameObject.FindGameObjectWithTag("use-output-error-checkbox").GetComponentInChildren<Toggle>();
        output_error_checkbox.onValueChanged.AddListener((bool v) => VSynthManager.use_output_error = v);
	}

	void Update() {
        Inputs.Hands.OnFrame();
        VecManager.OnFrame();
        VSynthManager.OnFrame();
        Inputs.OnFrame();
    }
}