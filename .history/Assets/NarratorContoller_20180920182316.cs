using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class NarratorContoller : MonoBehaviour {

    private bool active;
    private GameObject narrator;
    private GameObject narrationText;
    void Start () {
        active = false;
        narrator = this.gameObject;
        narrationText = narrator.transform.Find("Text").gameObject;
    }

    void show() {
        narrator.SetActive(true);
        narrator.GetComponent<Canvas>().overrideSorting = true;
        TimelineHelper.setRectPosition(this.NarrationText.GetComponent<RectTransform>(), 0, 0, 0, 0);
        TimelineHelper.setRectPosition(this.RectTransform, 0, 0, 0, 0);
        this.RectTransform.localScale = Vector3.one;
    }
    // Update is called once per frame
    void Update () {
        if (active) show();
    }
}
