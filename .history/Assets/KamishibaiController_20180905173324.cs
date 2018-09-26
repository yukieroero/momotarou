using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class KamishibaiController : MonoBehaviour {

    public Image background;
    public TextAsset timeline;
    private TimelineReader reader;
    private List<string[]> timelineHead;
    private List<string[]> timelineBody;

    private int index = 0;

    private bool head = false;
    private bool body = false;

    private List<string> headerCommandList = new List<string> {
        // 登場人物の定義
        "person",
    };

    private List<string> bodyCommandList = new List<string> {
        // 登場人物の定義
        "scene",
    };

    private Dictionary <string, Person> persons = new Dictionary<string, Person>();
    // 与えられた画像を背景に設置
    void setBackground (string path) {
        background.sprite = Resources.Load<Sprite>(path);
    }
    void headHandler (List<string[]> headDatas) {
        foreach (string[] line in headDatas) {
            string command = line[0];
            if (headerCommandList.Contains(command)) {
                switch (command) {
                    case "person":
                        string identifier = line[1];
                        persons.Add(identifier, new Person(line));
                        break;
                }
            }
        }
    }

    void bodyHandler (string[] line) {
        string command = line[0];
        if (bodyCommandList.Contains(command)) {
            switch (command) {
                case "scene":
                    setBackground(line[3]);
                    break;
            }
        } else {
            index++;
            bodyHandler(timelineBody[index]);
        }
    }

    // Use this for initialization
    void Start () {
        reader = new TimelineReader(timeline);
        timelineHead = reader.TimeLineHead;
        timelineBody = reader.TimeLineBody;
        headHandler(timelineHead);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Return)) {
            bodyHandler(timelineBody[index]);
            index++;
        }
    }
}
