using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class NarratorContoller : MonoBehaviour {

    private bool fadingIn;
    private bool fadingOut;
    private GameObject narrator;
    private GameObject narrationText;
    private CanvasGroup narrationMask;
    float fadeInSpeed = 0.01f;
    float targetOpacity = 0.7f;
    void Start () {
        fadingIn = false;
        fadingOut = false;
        narrator = this.gameObject;
        narrationMask = narrator.GetComponent<CanvasGroup>();
        narrationText = narrator.transform.Find("Text").gameObject;
    }

    void fadeIn() {
        float currentOpacity = narrationMask.alpha;
        float newOpacity = currentOpacity + fadeInSpeed;
        // targetOpacityになったら更新を止める
        if (newOpacity >= targetOpacity) {
            fadingIn = false;
            fadingOut = false;
        }
        narrationMask.alpha = newOpacity;
    }
    void fadeOut() {
        float currentOpacity = narrationMask.alpha;
        float newOpacity = currentOpacity - fadeInSpeed;
        // 0になったら更新を止める
        if (newOpacity <= 0f) {
            fadingIn = false;
            fadingOut = false;
        }
        narrationMask.alpha = newOpacity;
    }
    // Update is called once per frame
    void Update () {
        if (fadingIn == true) fadeIn();
        if (fadingOut == true) fadeOut();
    }
}
