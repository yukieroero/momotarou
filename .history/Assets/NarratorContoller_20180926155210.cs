using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class NarratorContoller : MonoBehaviour {

    private bool active;
    private GameObject narrator;
    private GameObject narrationText;
    private Image narrationMask;
    float fadeInSpeed = 0.01f;
    void Start () {
        active = false;
        narrator = this.gameObject;
        narrationMask = narrator.GetComponent<Image>();
        narrationText = narrator.transform.Find("Text").gameObject;
    }

    Color fadeIn(Color current) {
        // 背景色の透明度を1から0にしてフェードインを表現する
        float currentOpacity = current.a;
        float newOpacity = currentOpacity - fadeInSpeed;
        // 0になったら更新を止める
        if (newOpacity <= 0) {
            active = true;
        }
        return Timeline.TimelineHelper.ChangeColor(current, a:newOpacity);
    }
    // Update is called once per frame
    void Update () {
        if (!active) {
            narrationMask.color = fadeIn(narrationMask.color);
        }
    }
}
