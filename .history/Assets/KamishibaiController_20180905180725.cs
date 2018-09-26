﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

namespace Kamishibai {
    public class KamishibaiController : MonoBehaviour {

        public Image background;
        public TextAsset timeline;
        private TimelineReader reader;
        private List<string[]> timelineHead;
        private List<string[]> timelineBody;

        private HeadHandler headHandler;
        private BodyHandler bodyHandler;
        // 与えられた画像を背景に設置
        void setBackground (string path) {
            background.sprite = Resources.Load<Sprite>(path);
        }

        // Use this for initialization
        void Start () {
            reader = new TimelineReader(timeline);
            timelineHead = reader.GetTimeLineHead();
            timelineBody = reader.GetTimeLineBody();
            headHandler = new HeadHandler(timelineHead);
            bodyHandler = new BodyHandler(timelineBody);
        }

        // Update is called once per frame
        void Update () {
            if (Input.GetKeyUp(KeyCode.Return)) {
                bodyHandler.play();
            }
        }
    }
}
