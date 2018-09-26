using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour {

    private bool moving = false;
    private GameObject Actor;
    private Vector3 currentPos;
    private Vector3 destination;
    private Vector2 SPEED = new Vector2(0.05f, 0.05f);

    // Use this for initialization
    void Start () {
        Debug.Log("start");
        this.Actor = this.gameObject;
    }

    void move(Vector2 target){
        moving = true;
        currentPos = this.Actor.GetComponent<RectTransform>().position;
        destination = new Vector3(currentPos.x + target.x, currentPos.y + target.y, currentPos.z);
        SPEED.x *= target.x;
        SPEED.y *= target.y;
    }
    void moveHandler () {
        float newX = currentPos.x + SPEED.x;
        float newY = currentPos.y + SPEED.y;
        Vector3 newPos = currentPos;
        Debug.Log(currentPos.x);
        while (newPos.x != newX || newPos.y != newY) {
            Debug.Log(newPos.x);
            if (newPos.x != newX) newPos.x += SPEED.x;
            if (newPos.y != newY) newPos.y += SPEED.y;
            this.Actor.GetComponent<RectTransform>().position = newPos;
        }
        this.moving = false;
    }
    // Update is called once per frame
    void Update () {
        if (moving == true) {
            moveHandler();
        }
    }
}
