using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour {

    private bool moving = false;
    private Vector3 currentPos;
    private Vector3 destination;
    private Vector2 SPEED = new Vector2(0.1f, 0.1f);

    // Use this for initialization
    void Start () {
        Debug.Log("start");
    }

    public void move(Vector2 target){
        moving = true;
        currentPos = this.gameObject.GetComponent<RectTransform>().position;
        Vector3 targetV3 = target;
        destination = targetV3 + currentPos;
        SPEED.x *= target.x;
        SPEED.y *= target.y;
    }
    void moveHandler () {
        Vector3 newPos = currentPos;
        if (newPos.x != destination.x) newPos.x += SPEED.x;
        if (newPos.y != destination.y) newPos.y += SPEED.y;
        // 位置の更新
        this.gameObject.GetComponent<RectTransform>().position = newPos;
        currentPos = newPos;
        // 残りの移動距離
        float residue = (destination - currentPos).magnitude;
        if (residue >= 0)
            if (currentPos.x <= destination.x) this.moving = false;
    }
    // Update is called once per frame
    void Update () {
        if (moving == true) {
            moveHandler();
        }
    }
}
