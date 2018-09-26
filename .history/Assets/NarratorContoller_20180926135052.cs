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
    }
    // Update is called once per frame
    void Update () {
        if (!active) show();
    }
}
