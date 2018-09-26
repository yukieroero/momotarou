using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class NarratorContoller : MonoBehaviour {

    private bool active;
    private GameObject narrator;
    private GameObject narrationText;
    private CanvasGroup narrationMask;
    float fadeInSpeed = 0.01f;
    float targetOpacity = 0.7f;
    void Start () {
        active = false;
        narrator = this.gameObject;
        narrationMask = narrator.GetComponent<CanvasGroup>();
        narrationText = narrator.transform.Find("Text").gameObject;
    }

    void fadeIn() {
        float currentOpacity = narrationMask.alpha;
        float newOpacity = currentOpacity - fadeInSpeed;
        // targetOpacityになったら更新を止める
        if (newOpacity <= targetOpacity) {
            active = true;
        }
        narrationMask.alpha = newOpacity;
    }
    // Update is called once per frame
    void Update () {
        if (!active) {
            fadeIn();
        }
    }
}
