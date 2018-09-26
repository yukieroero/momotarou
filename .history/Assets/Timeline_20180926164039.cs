using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Timeline {

    public class TimelineHelper {
        public static Color ChangeColor (Color current, float r = float.NaN, float g = float.NaN, float b = float.NaN, float a = float.NaN) {
            Color newColor = current;
            if (!float.IsNaN (r)) newColor.r = r;
            if (!float.IsNaN (g)) newColor.g = g;
            if (!float.IsNaN (b)) newColor.b = b;
            if (!float.IsNaN (a)) newColor.a = a;
            return newColor;
        }
        public static void setRectPosition (RectTransform target, int left, int top, int right, int bottom) {
            target.offsetMin = new Vector2 (left, bottom);
            target.offsetMax = new Vector2 (-right, -top);
        }
        public static void setTextStyle (Text target, int fontSize, Color color, Font font) {
            target.fontSize = fontSize;
            target.color = color;
            target.font = font;
        }
    }

    public class Narrator {
        public GameObject GameObject;
        public NarratorContoller contoller;
        private GameObject NarrationText;
        private RectTransform narrationRect;
        public Narrator () {
            GameObject narration = new GameObject ();
            narration.name = "Narration";
            narrationRect = narration.AddComponent<RectTransform> ();
            narration.AddComponent<Canvas>();
            narration.AddComponent<CanvasGroup>().alpha = 0;
            contoller = narration.AddComponent<NarratorContoller> ();

            GameObject background = new GameObject();
            background.name = "background";
            RectTransform backgroundRect = background.AddComponent<RectTransform> ();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.pivot = new Vector2 (0.5f, 0.5f);
            backgroundRect.localScale = Vector3.one;
            TimelineHelper.setRectPosition (backgroundRect, 0, 0, 0, 0);
            background.AddComponent<Image> ().color = new Color (0, 0, 0, 1f);
            background.transform.SetParent(narration.transform);

            GameObject narrationText = new GameObject ();
            narrationText.name = "Text";
            RectTransform narrationTextRect = narrationText.AddComponent<RectTransform> ();
            narrationTextRect.anchorMin = Vector2.zero;
            narrationTextRect.anchorMax = Vector2.one;
            narrationTextRect.pivot = new Vector2 (0.5f, 0.5f);
            TimelineHelper.setRectPosition (narrationTextRect, 0, 0, 0, 0);
            Text text = narrationText.AddComponent<Text> ();
            text.alignment = TextAnchor.MiddleCenter;
            TimelineHelper.setTextStyle (text, 30, Color.white, Resources.Load<Font> ("fonts/hiragino"));
            narrationText.transform.SetParent (narration.transform);
            narration.SetActive (false);
            this.GameObject = narration;
            this.NarrationText = narrationText;
        }
        public void set (string text) {
            this.NarrationText.GetComponent<Text> ().text = text.Replace ("\\r\\n", "\r\n");
            this.GameObject.SetActive (true);
            this.GameObject.GetComponent<Canvas> ().overrideSorting = true;
            narrationRect.anchorMin = Vector2.zero;
            narrationRect.anchorMax = Vector2.one;
            narrationRect.pivot = new Vector2 (0.5f, 0.5f);
            narrationRect.localScale = Vector3.one;
        }
        public void show () {
            this.GameObject.SetActive (true);
            contoller.show();
        }
        public void hide () {
            contoller.hide();
            this.NarrationText.GetComponent<Text> ().text = "";
            this.GameObject.SetActive (false);
        }
    }

    public class HeadHandler {
        private Dictionary<string, Person> persons = new Dictionary<string, Person> ();
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
                if (headerCommandList.Contains (command)) {
                    switch (command) {
                        case "person":
                            string identifier = line[1];
                            GetPersons ().Add (identifier, new Person (line, kamishibai));
                            break;
                        case "title":
                            string value = line[1];
                            kamishibai.setTitle (value);
                            break;
                    }
                }
            }
        }

        public Dictionary<string, Person> GetPersons () {
            return persons;
        }
    }
    public class BodyHandler {
        private List<string> bodyCommandList = new List<string> {
            // シーンの定義
            "scene",
            // 発言
            "say",
            // ナレーション
            "narration",
        };
        private int index = 0;
        private int limit = 0;
        private List<string[]> datas;
        private KamishibaiController kamishibai;

        public int GetLimit () {
            return limit;
        }

        public int GetIndex () {
            return index;
        }

        public BodyHandler (KamishibaiController controller, List<string[]> bodyDatas) {
            kamishibai = controller;
            datas = bodyDatas;
            limit = datas.Count;
        }
        public void play () {
            string[] line = datas[GetIndex ()];
            Debug.LogFormat("command: {0}", line);
            string command = line[0];
            if (bodyCommandList.Contains (command)) {
                switch (command) {
                    case "scene":
                        string bgPath = line[3];
                        // 今表示されているオブジェクトを削除
                        foreach (Person p in kamishibai.GetHeadHandler().GetPersons().Values) {
                            kamishibai.offstage(p);
                        }
                        kamishibai.setBackground(bgPath);
                        this.next();
                        break;
                    case "say":
                        // say,right,oldman,わしがおじいさんじゃ,1
                        string position = line[1];
                        string identifier = line[2];
                        string serif = line[3];
                        Person actor = kamishibai.GetHeadHandler().GetPersons()[identifier];
                        actor.say(serif, position);
                        break;
                    case "narration":
                        // narration,show,昔々あるところにおじいさんとおばあさんが暮らしておりました。
                        if (line[1] == "show") {
                            string narration = line[2];
                            kamishibai.setNarration(narration);
                            kamishibai.showNarration();
                        } else {
                            kamishibai.hideNarration();
                        }
                        break;
                    case "bgm":
                        // bgm, bgm/op, 1000, 1, true
                        string bgmPath = line[1];
                        int fadeInTime = int.Parse(line[2]);
                        bool loop = bool.Parse(line[3]);
                        kamishibai.playAudio(bgmPath, fadeInTime, loop);
                        break;
                }
                this.incrementIndex ();
            } else {
                if (datas.Count > GetIndex ()) {
                    this.next ();
                }
            }

        }
        public void next () {
            this.incrementIndex ();
            this.play ();
        }
        public void incrementIndex () {
            index = index + 1;
        }
    }

    public class AbsCommand {
        public AbsCommand () {

        }
    }

    public class Balloon : AbsCommand {
        private string serif;
        private string position;
        private GameObject gameObject;
        private int balloonPadding = 100;

        private Font font = Resources.Load<Font> ("fonts/hiragino");
        public Balloon (Person actor, string serif, string position) {
            this.serif = serif;
            this.position = position;

            GameObject balloon = new GameObject ();
            balloon.name = "balloon";
            RectTransform balloonTrans = balloon.AddComponent<RectTransform> ();
            VerticalLayoutGroup balloonLayout = balloon.AddComponent<VerticalLayoutGroup> ();
            LayoutElement balloonLayoutElement = balloon.AddComponent<LayoutElement> ();

            // 吹き出しの画像を持つ
            GameObject balloonUI = new GameObject ();
            balloonUI.name = "balloonUI";
            Image balloonImage = balloonUI.AddComponent<Image> ();
            balloonImage.type = Image.Type.Sliced;
            Canvas balloonUICanvas = balloonUI.AddComponent<Canvas> ();
            RectTransform balloonUITrans = balloonUI.GetComponent<RectTransform> ();
            VerticalLayoutGroup balloonUILayout = balloonUI.AddComponent<VerticalLayoutGroup> ();
            balloonUILayout.childAlignment = TextAnchor.MiddleLeft;
            balloonUILayout.spacing = 5;
            // balloonオブジェクトの中に配置
            balloonUI.transform.SetParent (balloon.transform);

            // 誰の発言か
            GameObject nameObj = new GameObject ();
            nameObj.name = "name";
            Text nameText = nameObj.AddComponent<Text> ();
            TimelineHelper.setTextStyle (nameText, 20, Color.black, font);
            nameText.text = actor.GetName ();
            nameObj.transform.SetParent (balloonUI.transform);

            // serifオブジェクト
            GameObject serifObj = new GameObject ();
            serifObj.name = "serif";
            Text serifText = serifObj.AddComponent<Text> ();
            TimelineHelper.setTextStyle (serifText, 25, Color.black, font);
            serifText.text = serif;
            serifObj.transform.SetParent (balloonUI.transform);

            RectTransform serifTrans = serifObj.GetComponent<RectTransform> ();
            serifTrans.pivot = new Vector2 (0.5f, 0.5f);

            // 右と左で位置調整
            balloonUILayout.padding = new RectOffset (30, 30, 40, 90);
            if (position == "right") {
                balloonUITrans.pivot = new Vector2 (1, 0);
                balloonLayout.childAlignment = TextAnchor.LowerRight;
            } else if (position == "left") {
                balloonUITrans.pivot = Vector2.zero;
                balloonLayout.childAlignment = TextAnchor.LowerLeft;
            }
            balloonImage.sprite = actor.getBalloonImg (position);
            balloonUI.SetActive (true);
            serifObj.SetActive (true);
            balloonUILayout.childControlHeight = true;
            balloonUILayout.childControlWidth = true;
            balloonUILayout.childForceExpandHeight = false;
            balloonUILayout.childForceExpandWidth = false;
            balloonLayout.childControlHeight = true;
            balloonLayout.childControlWidth = true;
            balloonLayout.childForceExpandHeight = false;
            balloonLayout.childForceExpandWidth = false;
            this.GameObject = balloon;
        }

        public GameObject GameObject {
            get {
                return gameObject;
            }

            set {
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
        public bool atWing = false;
        private List<Balloon> balloonList = new List<Balloon> ();

        public GameObject Actor {
            get {
                return actor;
            }

            set {
                actor = value;
            }
        }
        public List<Balloon> BalloonList {
            get {
                return balloonList;
            }

            set {
                balloonList = value;
            }
        }
        public Sprite getBalloonImg (string suffix = "") {
            string path = "parts/balloon/" + this.GetIdentifier ().Split ('/') [0];
            if (suffix != "") {
                return Resources.Load<Sprite> (path + "/" + suffix);
            }
            return Resources.Load<Sprite> (path);
        }

        public string GetIdentifier () {
            return identifier;
        }

        public string GetName () {
            return name;
        }

        public void backFromWing (string position) {
            if (position == "right") this.move (x: -200f / 5);
            else if (position == "left") this.move (x: 200f / 5);
            this.atWing = false;
        }
        public void goWing (string position) {
            if (position == "right") this.move (x: 200f / 5);
            else if (position == "left") this.move (x: -200f / 5);
            this.atWing = true;
        }
        public void move (float x = 0, float y = 0, float z = 0, bool animate = true) {
            if (animate) {
                actor.GetComponent<ActorController> ().move (new Vector3 (x, y, z));
            } else {
                Vector3 currentPos = this.Actor.GetComponent<RectTransform> ().position;
                float newX = currentPos.x + x;
                float newY = currentPos.y + y;
                float newZ = currentPos.z + z;
                this.Actor.GetComponent<RectTransform> ().position = new Vector3 (newX, newY, newZ);
            }
        }

        // person,oldwoman,おばあさん,,oldwoman/normal
        public Person (string[] personDatas, KamishibaiController kamishibai) {
            stage = kamishibai;
            identifier = personDatas[1];
            name = personDatas[2];
            icon = personDatas[3];
            image = personDatas[4];
        }
        public void say (string serif, string position) {
            stage.action (this, serif, position);
        }
        public string GetImage () {
            return image;
        }
    }
    public class TimelineReader {
        private TextAsset csvFile;
        private List<string[]> timeLineBody = new List<string[]> ();
        private List<string[]> timeLineHead = new List<string[]> ();
        private int height = 0;
        private Dictionary<string, string> identifier = new Dictionary<string, string> () { { "header", "[head]" }, { "body", "[body]" },
        };
        public TimelineReader (TextAsset csvFile) {
            this.csvFile = csvFile;
            StringReader reader = new StringReader (csvFile.text);
            bool header = false;
            bool body = false;
            while (reader.Peek () > -1) {
                string line = reader.ReadLine ();
                if (line == identifier["header"]) {
                    header = true;
                    body = false;
                } else if (line == identifier["body"]) {
                    header = false;
                    body = true;
                } else if (line.Length > 0) {
                    if (header) timeLineHead.Add (line.Split (',')); // リストに入れる
                    else if (body) timeLineBody.Add (line.Split (',')); // リストに入れる
                }
                height++; // 行数加算
            }
        }

        public List<string[]> GetTimeLineBody () {
            return timeLineBody;
        }

        public List<string[]> GetTimeLineHead () {
            return timeLineHead;
        }
    }
}
