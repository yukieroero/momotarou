using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class KamishibaiController : MonoBehaviour {
    public Canvas canvas;

    public TextAsset timeline;
    private TimelineReader reader;
    private List<string[]> timelineHead;
    private List<string[]> timelineBody;

    private HeadHandler headHandler;
    private BodyHandler bodyHandler;

    private Image background;
    // 与えられた画像を背景に設置
    public void setBackground (string path) {
        background.sprite = Resources.Load<Sprite>(path);
    }
    public void setTitle (string path) {
        background.sprite = Resources.Load<Sprite>(path);
    }

    // Use this for initialization
    void Start () {
        background = GameObject.Find("Canvas/Background").GetComponent<Image>();
        reader = new TimelineReader(timeline);
        timelineHead = reader.GetTimeLineHead();
        timelineBody = reader.GetTimeLineBody();
        headHandler = new HeadHandler(this, timelineHead);
        bodyHandler = new BodyHandler(this, timelineBody);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Return)) {
            bodyHandler.play();
        }
    }
}

