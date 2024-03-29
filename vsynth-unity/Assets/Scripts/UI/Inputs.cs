using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class Inputs {
    public static List<(OVRInput.RawButton key, bool wasDown, Action onUp, Action onDown)> events = new();

    public static void InitKeys(List<(OVRInput.RawButton key, Action onDown, Action onUp)> es) {
        events = es.ConvertAll(e => (e.key, false, e.onDown, e.onUp));
    }

    public static void OnFrame() {
        events.ForEach(e => {
            bool isDown = OVRInput.Get(e.key);
            if (!e.wasDown && isDown && e.onDown != null) e.onDown();
            if (e.wasDown && !isDown && e.onUp != null) e.onUp();
            e.wasDown = isDown;
        });
    }

    public static void InitButtons(List<(string tag, UnityAction onClick)> es) {
        es.ForEach(e => {
            GameObject.FindGameObjectWithTag(e.tag)
                      .GetComponent<Button>().onClick
                      .AddListener(e.onClick);
        });
    }

    public static void InitSliders(List<(string tag, UnityAction<float> onSlide)> es) {
        es.ForEach(e => {
            GameObject.FindGameObjectWithTag(e.tag)
                      .GetComponent<Slider>().onValueChanged
                      .AddListener(e.onSlide);
        });
    }

    public static class Hands {
        public static Vector3 left_tip_pos = Vector3.zero;
        public static Vector3 right_tip_pos = Vector3.zero;

        public static void OnFrame() {
            left_tip_pos = GameObject.FindGameObjectWithTag("left_finger_tip").transform.position;
            right_tip_pos = GameObject.FindGameObjectWithTag("right_finger_tip").transform.position;
        }
    }
}