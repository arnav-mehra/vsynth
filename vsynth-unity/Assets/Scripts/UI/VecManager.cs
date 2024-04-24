using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class GeoObject {
    public const float THICKNESS = 0.005f;

    public GameObject go;

    public Transform t => go.transform;
    
    public Renderer r => go.GetComponent<Renderer>();

    public Color color {
        get => r.material.color;
        set => r.material.color = value;
    }

    public GeoObject(PrimitiveType pt) {
        var example_tag = pt switch {
            PrimitiveType.Sphere => "example-sphere",
            _ => "example-cylinder"
		};
        var example = GameObject.FindGameObjectWithTag(example_tag);
        go = GameObject.Instantiate(example);
        t.localScale = new(THICKNESS, THICKNESS, THICKNESS);
    }
}

public class DrawnVector {
    public GeoObject segment = new(PrimitiveType.Cylinder);
    public List<GeoObject> points = new() { new(PrimitiveType.Sphere), new(PrimitiveType.Sphere) };
    public bool is_input = true;
    public TextMeshPro length_text;

    public bool shown {
        get => segment.r.enabled;
        set {
            points.ForEach(p => p.r.enabled = value);
            segment.r.enabled = value;
        }
    }

    public Color color {
        get => segment.color;
        set {
            segment.color = value;
            points.ForEach(p => p.color = value);
        }
    }

    public Vector3 vector => points[1].t.position - points[0].t.position;

    public DrawnVector() {
        color = Color.blue;
        shown = false;
        length_text = new GameObject().AddComponent<TextMeshPro>();
        length_text.gameObject.transform.localScale = new(0.01f, 0.01f, 0.01f);
        length_text.color = Color.black;
    }

    public void ToggleIsInput() {
        is_input = !is_input;
        color = is_input ? Color.blue : Color.red;
    }

    /*public void Highlight(GameObject go) {
        points.ForEach(pt => pt.Highlight(go));
        segment.Highlight(go);
    }*/

    public void SetPointPos(int i, Vector3 p) {
        points[i].t.position = p;
        FixSegment();
    }

    void FixSegment() {
        var pos = (points[0].t.position + points[1].t.position) / 2.0f;
        var dir = points[1].t.position - points[0].t.position;
        var scale = new Vector3(GeoObject.THICKNESS, dir.magnitude / 2.0f, GeoObject.THICKNESS);
        segment.t.position = pos;
        segment.t.up = dir.normalized;
        segment.t.localScale = scale;

        points[1].t.localScale = 
            new(GeoObject.THICKNESS * 2.0f, GeoObject.THICKNESS * 2.0f, GeoObject.THICKNESS * 2.0f);

        length_text.enabled = true;
        length_text.gameObject.transform.position = points[1].t.position + vector.normalized * 0.05f;
        length_text.text = vector.magnitude.ToString("F2");
    }

    public void Destroy() {
        segment.go.SetActive(false);
        points.ForEach(p => p.go.SetActive(false));
        length_text.gameObject.SetActive(false);
    }
}

public static class DebugText {
    public static void Set(string s) {
        GameObject.FindGameObjectWithTag("debug-text")
                  .GetComponent<TextMeshProUGUI>().text = s;
    }
}

public static class VecManager {
    public static List<List<DrawnVector>> vecs = new() { new() };
    public static DrawnVector preview_vec = new();
    public static void OnFrame()
    {
        if (preview_vec.shown) {
            preview_vec.SetPointPos(1, Inputs.Hands.right_tip_pos);
        }
    }

    public static void StartVector() {
        DebugText.Set("Starting vector");
        preview_vec.SetPointPos(0, Inputs.Hands.right_tip_pos);
        preview_vec.SetPointPos(1, Inputs.Hands.right_tip_pos);
        preview_vec.shown = true;
    }

    public static void EndVector() {
        DebugText.Set("Ending vector");
        vecs[^1].Add(preview_vec);
        preview_vec = new();
    }

    public static void CancelVector() {
        DebugText.Set("Canceled vector");
        preview_vec.shown = false;
    }

    public static void ClearVectors() {
        DebugText.Set("Clearing vectors");
        vecs.ForEach(vs => vs.ForEach(v => v.Destroy()));
        vecs.Clear();
        vecs.Add(new());
    }

    public static void ToggleIsInput() {
        DebugText.Set("Toggled vector io type");
        preview_vec.ToggleIsInput();
    }

    public static void StartExample()
    {
        vecs.Add(new());
    }

    public static List<List<object>> GetVectors(bool is_input) => (
        vecs.Select(vs => vs.Where(v => v.is_input == is_input)
                            .Select(v => (object)v.vector).ToList())
            .ToList()
    );
}