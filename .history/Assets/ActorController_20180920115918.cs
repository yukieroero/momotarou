using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour {

    private bool moving = false;
    private GameObject Actor;
    private Vector2 moveDistance

    // Use this for initialization
    void Start () {
        Debug.Log("start");
        this.Actor = this.gameObject;
    }

    void move(Vector2 target){
        moving = true;
        moveDistance = target;
    }
    void moveHandler () {
        Vector2 SPEED = new Vector2(0.05f, 0.05f);
        SPEED.x *= this.moveDistance.x;
        SPEED.y *= this.moveDistance.y;
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
        if (newPos.x == newX && newPos.y != newY) this.moving = false;
    }
    // Update is called once per frame
    void Update () {
        if (moving == true) {
            moveHandler();
        }
    }
}
