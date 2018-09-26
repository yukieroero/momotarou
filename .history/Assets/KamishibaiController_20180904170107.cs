using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class KamishibaiController : MonoBehaviour {

    public Image background;
    string Inaka = "background/inaka";
    // 与えられた画像を背景に設置
    void setBackground (string path) {
        background.sprite = Resources.Load<Sprite>(path);
    }

    // Use this for initialization
    void Start () {
        TimelineReader reader = new Reader();
    }

    // Update is called once per frame
    void Update () {

    }
}
