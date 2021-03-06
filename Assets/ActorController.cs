﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour {

    private bool moving = false;
    private Vector3 startPos;
    private Vector3 currentPos;
    private Vector3 destination;
    private Vector2 SPEED;
    private Vector3 movedLength = Vector3.zero; // 移動した距離
    private Vector3 movingLength; //  移動する距離

    // Use this for initialization
    void Start () {
        Debug.Log("start");
        setInitialSpeed();
    }

    void setInitialSpeed() {
        SPEED = new Vector2(0.2f, 0.2f);
    }

    public void move(Vector3 target){
        moving = true;
        currentPos = this.gameObject.GetComponent<RectTransform>().position;
        movingLength = target;
        // 目的座標 = 今いる座標 + 移動距離(x, y)
        destination = currentPos + movingLength;
        // スピードを設定
        SPEED.x *= movingLength.x;
        SPEED.y *= movingLength.y;
    }
    bool moveXEnough() {
        // 左に移動
        if (movingLength.x < 0) return movedLength.x <= movingLength.x;
        // 右に移動
        else if (movingLength.x > 0) return movedLength.x >= movingLength.x;
        // 移動なし
        return true;
    }
    bool moveYEnough() {
        // 下に移動
        if (movingLength.y < 0) return movedLength.y <= movingLength.y;
        // 上に移動
        else if (movingLength.y > 0) return movedLength.y >= movingLength.y;
        // 移動なし
        return true;
    }
    bool moveZEnough() {
        // 奥に移動
        if (movingLength.z < 0) return movedLength.z <= movingLength.z;
        // 手前に移動
        else if (movingLength.z > 0) return movedLength.z >= movingLength.z;
        // 移動なし
        return true;
    }
    void moveHandler () {
        Vector3 newPos = currentPos;
        if (newPos.x != destination.x) newPos.x += SPEED.x;
        if (newPos.y != destination.y) newPos.y += SPEED.y;
        // 位置の更新
        this.gameObject.GetComponent<RectTransform>().position = newPos;
        currentPos = newPos;
        // 移動した距離  今いる座標 - 移動が始まった時の座標
        movedLength = currentPos - (destination - movingLength);
        // 全て十分に移動していれば止める
        if (moveXEnough() && moveYEnough() && moveZEnough()) {
            moving = false;
            // スピードを元に戻す
            setInitialSpeed();
        }
    }
    // Update is called once per frame
    void Update () {
        if (moving == true) {
            moveHandler();
        }
    }
}
