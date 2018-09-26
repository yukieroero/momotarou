using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class NarratorContoller : MonoBehaviour {

    private bool active;
    void Start () {
        active = false;
    }

    void show() {

    }
    // Update is called once per frame
    void Update () {
        if (active) show();
    }
}
