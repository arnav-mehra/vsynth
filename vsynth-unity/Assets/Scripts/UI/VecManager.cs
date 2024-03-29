using System.Collections.Generic;
using UnityEngine;

public class GeoObject {
    public GameObject go;

    public Transform t => go.transform;
    
    public Renderer r => go.GetComponent<Renderer>();

    public Color color {
        get => r.material.color;
        set => r.material.color = value;
    }

    public GeoObject(PrimitiveType pt) {
        go = GameObject.CreatePrimitive(pt);
        r.material = new(Shader.Find("Unlit/Texture"));
    }
}

public class DrawnVector {
    public GeoObject segment = new(PrimitiveType.Cylinder);
    public List<GeoObject> points = new() { new(PrimitiveType.Sphere), new(PrimitiveType.Sphere) };
    public bool is_input = true;

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
        var pos = (points[0].t.position + points[1].t.position) / 2.0f;
        var rot = Quaternion.FromToRotation(points[0].t.position, points[1].t.position);
        var height = Vector3.Distance(points[0].t.position, points[1].t.position);
        var scale = new Vector3(0.05f, 0.05f, height);
        segment.t.SetPositionAndRotation(pos, rot);
        segment.t.localScale = scale;
    }

    public void Destroy() {
        segment.go.SetActive(false);
        points.ForEach(p => p.go.SetActive(false));
    }
}

public static class VecManager {
    public static List<DrawnVector> vecs = new();
    public static DrawnVector preview_vec = new();

    public static void OnFrame() {
        if (preview_vec.shown) {
            preview_vec.SetPointPos(1, Inputs.Hands.right_tip_pos);
        }
    }

    public static void StartVector() {
        preview_vec.SetPointPos(0, Inputs.Hands.right_tip_pos);
        preview_vec.SetPointPos(1, Inputs.Hands.right_tip_pos);
        preview_vec.shown = true;
    }

    public static void EndVector() {
        vecs.Add(preview_vec);
        preview_vec = new();
    }

    public static void CancelVector() {
        preview_vec.shown = false;
    }

    public static void ClearVectors() {
        vecs.ForEach(v => v.Destroy());
        vecs.Clear();
    }

    public static void ToggleIsInput() {
        preview_vec.ToggleIsInput();
    }

    public static List<object> GetVectors(bool is_input) => (
        vecs.FindAll(v => v.is_input == is_input)
            .ConvertAll(v => (object)v.vector)
    );
}