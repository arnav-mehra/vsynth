using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class Inputs {
    public static List<(OVRInput.RawButton key, bool wasDown, Action onDown, Action onUp)> events = new();

    public static void InitKeys(List<(OVRInput.RawButton key, Action onDown, Action onUp)> es) {
        events = es.ConvertAll(e => (e.key, false, e.onDown, e.onUp));
    }

    public static void OnFrame() {
        for (int i = 0; i < events.Count; i++) {
            var (key, wasDown, onDown, onUp) = events[i];
            bool isDown = OVRInput.Get(key);
            if (!wasDown &&  isDown && onDown != null) onDown();
            if ( wasDown && !isDown && onUp   != null) onUp();
            events[i] = (key, isDown, onDown, onUp);
        }
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