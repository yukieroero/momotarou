using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Timeline {
    public class HeadHandler {
        private Dictionary <string, Person> persons = new Dictionary<string, Person>();
        private List<string> headerCommandList = new List<string> {
            // 登場人物の定義
            "person",
            "title",
        };

        private KamishibaiController kamishibai;
        public HeadHandler (KamishibaiController controller, List<string[]> headDatas) {
            kamishibai = controller;
            foreach (string[] line in headDatas) {
                string command = line[0];
                if (headerCommandList.Contains(command)) {
                    switch (command) {
                        case "person":
                            string identifier = line[1];
                            GetPersons().Add(identifier, new Person(line, kamishibai));
                            break;
                        case "title":
                            string value = line[1];
                            kamishibai.setTitle(value);
                            break;
                    }
                }
            }
        }

        public Dictionary<string, Person> GetPersons()
        {
            return persons;
        }
    }
    public class BodyHandler {
        private List<string> bodyCommandList = new List<string> {
            // シーンの定義
            "scene",
            "say",
        };
        private int index = 0;
        private int limit = 0;
        private List<string[]> datas;
        private KamishibaiController kamishibai;

        public int GetLimit()
        {
            return limit;
        }

        public int GetIndex()
        {
            return index;
        }

        public BodyHandler (KamishibaiController controller, List<string[]> bodyDatas) {
            kamishibai = controller;
            datas = bodyDatas;
            limit = datas.Count;
        }
        public void play() {
            string[] line = datas[GetIndex()];
            string command = line[0];
            if (bodyCommandList.Contains(command)) {
                switch (command) {
                    case "scene":
                        string path = line[3];
                        kamishibai.setBackground(path);
                        break;
                    case "say":
                        // say,right,oldman,わしがおじいさんじゃ,1
                        string position = line[1];
                        string identifier = line[2];
                        string serif = line[3];
                        Person chara = kamishibai.GetHeadHandler().GetPersons()[identifier];
                        chara.say(serif, position);
                        break;
                }
                this.incrementIndex();
            } else {
                if (datas.Count > GetIndex()) {
                    this.next();
                }
            }

        }
        public void next() {
            this.incrementIndex();
            this.play();
        }
        public void incrementIndex() {
            index = index + 1;
        }
    }

    public class AbsCommand {
        public AbsCommand() {

        }
    }

    public class Balloon : AbsCommand {
        private string serif;
        private string position;
        private GameObject gameObject;
        private int balloonPadding = 100;

        private string jibunBalloon = "parts/jibun";
        private string aiteBalloon = "parts/aite";
        public Balloon (string serif, string position) {
            this.serif = serif;
            this.position = position;
            GameObject balloon = new GameObject();
            balloon.name = "balloon";
            RectTransform balloonTrans = balloon.AddComponent<RectTransform>();
            // VerticalLayoutGroup balloonLayout = balloon.AddComponent<VerticalLayoutGroup>();
            ContentSizeFitter balloonFitter = balloon.AddComponent<ContentSizeFitter>();
            balloonFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            balloonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject serifObj = new GameObject();
            serifObj.name = "serif";
            Text serifText = serifObj.AddComponent<Text>();
            serifText.font = Resources.Load<Font>("fonts/hiragino");
            serifText.color = Color.black;
            serifText.text = serif;
            serifText.fontSize = 25;
            ContentSizeFitter serifFitter = serifObj.AddComponent<ContentSizeFitter>();
            serifFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            serifFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            serifObj.transform.SetParent(balloon.transform);

            GameObject balloonUI = new GameObject();
            balloonUI.name = "balloonUI";
            Image balloonImage = balloonUI.AddComponent<Image>();
            balloonImage.raycastTarget = true;
            balloonImage.fillCenter = true;
            Canvas balloonUICanvas = balloonUI.AddComponent<Canvas>();
            RectTransform balloonUITrans = balloonUI.GetComponent<RectTransform>();
            balloonUITrans.anchorMin = Vector2.zero;
            balloonUITrans.anchorMax = Vector2.one;
            balloonUITrans.pivot = new Vector2(0.5f, 0.5f);
            balloonUI.transform.SetParent(serifObj.transform);

            RectTransform serifTrans = serifObj.GetComponent<RectTransform>();
            serifTrans.anchorMin = Vector2.zero;
            serifTrans.anchorMax = Vector2.one;
            serifTrans.pivot = new Vector2(0.5f, 0.5f);
            // serifに謎の-50が入るので修正
            serifTrans.offsetMax = Vector2.zero;
            serifTrans.offsetMin = Vector2.zero;

            // 位置調整 right => pos x = -150 left => pos x = 150;
            if (position == "right") {
                serifTrans.offsetMax = new Vector2(-300f, 0f);
                balloonUITrans.offsetMin = new Vector2(-30f, -25f);
                balloonUITrans.offsetMax = new Vector2(80f, 20f);
                // balloonUILayout.padding = new RectOffset(25, 120, 10, 0);
                // balloon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150f, 0f);
                // balloonLayout.childAlignment = TextAnchor.LowerRight;
                // balloonLayout.padding.right = 200;
                balloonImage.sprite = Resources.Load<Sprite>(jibunBalloon);
            } else if (position == "left") {
                // balloonUILayout.padding = new RectOffset(120, 25, 10, 0);
                // balloon.GetComponent<RectTransform>().anchoredPosition = new Vector2(150f, 0f);
                // balloonLayout.childAlignment = TextAnchor.LowerLeft;
                // balloonLayout.padding.left = 200;
                balloonImage.sprite = Resources.Load<Sprite>(aiteBalloon);
            }
            balloonUICanvas.overrideSorting = true;
            balloonUICanvas.sortingOrder = -1;
            balloonUI.SetActive(true);
            serifObj.SetActive(true);
            // balloonUILayout.childControlHeight = false;
            // balloonUILayout.childControlWidth = false;
            // balloonUILayout.childForceExpandHeight = false;
            // balloonUILayout.childForceExpandWidth = true;
            // balloonLayout.childControlHeight = false;
            // balloonLayout.childControlWidth = false;
            // balloonLayout.childForceExpandHeight = false;
            // balloonLayout.childForceExpandWidth = false;
            this.GameObject = balloon;
        }

        public GameObject GameObject
        {
            get
            {
                return gameObject;
            }

            set
            {
                gameObject = value;
            }
        }
    }
    public class Person : AbsCommand {
        private string identifier;
        private string name;
        private string icon;
        private string image;
        private KamishibaiController stage;
        private string serif;
        private GameObject actor;
        private List<Balloon> balloonList = new List<Balloon>();

        public GameObject Actor
        {
            get
            {
                return actor;
            }

            set
            {
                actor = value;
            }
        }
        public List<Balloon> BalloonList
        {
            get
            {
                return balloonList;
            }

            set
            {
                balloonList = value;
            }
        }

        public string GetIdentifier()
        {
            return identifier;
        }

        // person,oldwoman,おばあさん,,oldwoman/normal
        public Person(string[] personDatas, KamishibaiController kamishibai) {
            stage = kamishibai;
            identifier = personDatas[1];
            name = personDatas[2];
            icon = personDatas[3];
            image = personDatas[4];
        }
        public void say (string serif, string position) {
            stage.action(this, serif, position);
        }
        public string GetImage(){
            return image;
        }
    }
    public class TimelineReader
    {
        private TextAsset csvFile;
        private List <string[]> timeLineBody = new List<string[]>();
        private List <string[]> timeLineHead = new List<string[]>();
        private int height = 0;
        private Dictionary<string, string> identifier = new Dictionary<string, string>() {
            {"header", "[head]"},
            {"body", "[body]"},
        };
        public TimelineReader(TextAsset csvFile)
        {
            this.csvFile = csvFile;
            StringReader reader = new StringReader(csvFile.text);
            bool header = false;
            bool body = false;
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                if (line == identifier["header"]) {
                    header = true;
                    body=false;
                } else if (line == identifier["body"]) {
                    header = false;
                    body = true;
                } else if (line.Length > 0) {
                    if (header) timeLineHead.Add(line.Split(',')); // リストに入れる
                    else if (body) timeLineBody.Add(line.Split(',')); // リストに入れる
                }
                height++; // 行数加算
            }
        }

        public List<string[]> GetTimeLineBody()
        {
            return timeLineBody;
        }

        public List<string[]> GetTimeLineHead()
        {
            return timeLineHead;
        }
    }
}
