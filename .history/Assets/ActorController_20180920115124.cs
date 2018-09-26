using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour {

    private bool mooving = false;

    // Use this for initialization
    void Start () {
        Debug.Log("start");
    }

    // Update is called once per frame
    void Update () {
        if (mooving == false) {
            mooving = true;
        }
    }
}
