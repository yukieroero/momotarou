using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour {

    private bool mooving = false;
    private GameObject Actor;

    // Use this for initialization
    void Start () {
        Debug.Log("start");
        this.Actor = this.gameObject;
    }

    void move(Vector2 target){
        Vector2 SPEED = new Vector2(0.05f, 0.05f);
        SPEED.x *= target.x;
        SPEED.y *= target.y;
        Vector3 currentPos = this.Actor.GetComponent<RectTransform>().position;
        float newX = currentPos.x + SPEED.x;
        float newY = currentPos.y + SPEED.y;
        Vector3 newPos = currentPos;
        while (newPos.x != newX || newPos.y != newY) {
            Debug.Log(newPos.x);
            if (newPos.x != newX) newPos.x += SPEED.x;
            if (newPos.y != newY) newPos.y += SPEED.y;
            this.Actor.GetComponent<RectTransform>().position = newPos;
        }
    }
    // Update is called once per frame
    void Update () {
        if (mooving == false) {
            mooving = true;
        }
    }
}
