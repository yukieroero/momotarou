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
    private List<string[]> timelineDatas;

    private int index = 0;

    private

    // 与えられた画像を背景に設置
    void setBackground (string path) {
        background.sprite = Resources.Load<Sprite>(path);
    }

    // Use this for initialization
    void Start () {
        reader = new TimelineReader(timeline);
        timelineDatas = reader.TimeLineDatas;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Return)) {
            foreach (string word in timelineDatas[index]) {
                Debug.Log(word);
            }
            if (timelineDatas[index][0])
            index++;
        }
    }
}
