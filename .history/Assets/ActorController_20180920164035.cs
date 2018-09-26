using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour {

    private bool moving = false;
    private Vector3 startPos;
    private Vector3 currentPos;
    private Vector3 destination;
    private Vector2 SPEED = new Vector2(0.05f, 0.05f);
    private Vector3 movedLength = Vector3.zero; // 移動した距離
    private Vector3 movingLength; //  移動する距離

    // Use this for initialization
    void Start () {
        Debug.Log("start");
    }

    public void move(Vector2 target){
        moving = true;
        currentPos = this.gameObject.GetComponent<RectTransform>().position;
        movingLength = target;
        // 目的座標 = 今いる座標 + 移動距離(x, y)
        destination = currentPos + movingLength;
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
        // 移動した距離  今いる座標 - 移動が始まった時の座標
        movingLength = currentPos - (destination - movedLength);
    }
    // Update is called once per frame
    void Update () {
        if (moving == true) {
            moveHandler();
        }
    }
}
