using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class KamishibaiController : MonoBehaviour {

    public Image background;
    public TextAsset timeline;
    string Inaka = "background/inaka";
    private TimelineReader reader;

    // 与えられた画像を背景に設置
    void setBackground (string path) {
        background.sprite = Resources.Load<Sprite>(path);
    }

    // Use this for initialization
    void Start () {
        reader = new TimelineReader(timeline);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Return)) {
            Debug.Log(reader.next()[0][0]);
        }
    }
}
