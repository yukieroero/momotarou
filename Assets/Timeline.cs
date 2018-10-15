using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Timeline {

    /// <summary>
    /// 対象GameObjectを範囲いっぱいに広げる
    /// </summary>
    /// <param name="target">対象GameObject</param>
    /// <returns>対象GameObjectにアタッチしたRectTransform</returns>
    public static class TimelineHelper {
        public static void AddParam(object target, Hashtable parameters) {
            object original = target;
            foreach(string key in parameters.Keys) {
                target = original;
                PropertyInfo pi = null;
                // .でネストの深いところを変更する場合の対応
                string[] nest = key.Split('.');
                for (int i=0; i<nest.Length; i++) {
                    // propertyを変更してくれるPropertyInfo
                    pi = target.GetType().GetProperty(nest[i]);
                    // 最後のループではtargetの更新はなし
                    if (nest.Length > 1 && pi != null && i < nest.Length - 1){
                        target = pi.GetValue(target, null);
                        continue;
                    }
                }
                if (pi != null) {
                    System.Type expect = pi.PropertyType;
                    System.Type actually = parameters[key].GetType();
                    if (expect == actually) {
                        pi.SetValue(target, parameters[key], null);
                    } else Debug.LogErrorFormat("Type error of {0}: Expected {1} but {2}", key, expect, actually);
                } else Debug.LogErrorFormat("No such property: {0}", key);
            }
        }
        public static RectTransform addFillRect(GameObject target) {
            RectTransform fillRect = target.GetComponent<RectTransform>();
            if (fillRect == null) {
                fillRect = target.AddComponent<RectTransform>();
            }
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2 (0.5f, 0.5f);
            fillRect.localScale = Vector3.one;
            return fillRect;
        }
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


    public class HeadHandler {
        private Dictionary<string, Person> persons = new Dictionary<string, Person>();
        private Dictionary<string, AdvancedAudioClip> bgms = new Dictionary<string, AdvancedAudioClip>();
        private List<string> headerCommandList = new List<string> {
            // 登場人物の定義
            "person",
            "title",
            // BGMの定義
            "bgm",
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
                            GetPersons().Add (identifier, new Person (line, kamishibai));
                            break;
                        case "title":
                            string value = line[1];
                            kamishibai.setTitle(value);
                            break;
                        case "bgm":
                            // bgm, source/id, loop_start(ms), loop_end(ms)
                            string bgmId = line[1];
                            float loopStart = float.Parse(line.Length > 2 ? line[2] : "0");
                            float loopEnd = float.Parse(line.Length > 3 ? line[3] : "0");
                            AdvancedAudioClip clip = AudioManager.Instance.getClip(bgmId);
                            clip.SetLoop(loopStart, loopEnd);
                            GetBGMs().Add(bgmId, clip);
                            break;
                    }
                }
            }
        }

        public Dictionary<string, Person> GetPersons () {
            return persons;
        }
        public Dictionary<string, AdvancedAudioClip> GetBGMs () {
            return bgms;
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
            // bgm
            "bgm",
            // se
            "se",
            // 選択ダイアログ
            "select",
            // ラベル
            "label",
            // goto
            "goto",
            // 何もしないで先おくり
            "nothing",
        };
        private int index = 0;
        private int limit = 0;
        private List<string[]> datas;
        private KamishibaiController kamishibai;
        private bool isNothing;

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
            isNothing = false;
        }
        public void play (string[] line=null) {
            if (isNothing) return;
            // 引数lineを与えるとそれを実行できる
            if (line == null) line = datas[GetIndex ()];
            string command = line[0].Split('@')[0];
            Debug.LogFormat("command: {0}", command);
            if (bodyCommandList.Contains(command)) {
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
                        this.incrementIndex ();
                        break;
                    case "narration":
                        // narration,show,昔々あるところにおじいさんとおばあさんが暮らしておりました。
                        if (line[1] == "show") {
                            isNothing = true;
                            string narration = line[2];
                            NarratorManager.Instance.show(narration, 200f);
                            kamishibai.sleep(() => {
                                isNothing = false;
                                this.incrementIndex();
                                Debug.Log("nothing end");
                            }, waitTime: 200f);
                        } else {
                            isNothing = true;
                            NarratorManager.Instance.hide(200f);
                            kamishibai.sleep(() => {
                                isNothing = false;
                                this.next();
                                Debug.Log("nothing end");
                            }, 200f);
                        }
                        break;
                    case "bgm":
                        // bgm, bgm/op, 1000, 1, true
                        string bgmId = line[1];
                        int fadeDuration = int.Parse(line[2]);
                        float bgmVolume = float.Parse(line[3]);
                        bool loop = line.Length > 4 ? bool.Parse(line[4]) : false;
                        AudioManager.Instance.fadeBGM(bgmId, fadeDuration, bgmVolume, loop);
                        this.next();
                        break;
                    case "se":
                        // se, SE/se_bat_hit,0.5
                        string seId = line[1];
                        float seVolume = line.Length > 2 ? float.Parse(line[2]) : 1f;
                        AudioManager.Instance.playSE(seId, seVolume);
                        this.next();
                        break;
                    case "select":
                        // select,このあとどうする？,label_1_end
                        Select select = SelectManager.Instance.GetSelect(line[2]);
                        // 未選択の個数
                        if (select.Count(available:true) > 0) {
                            SelectManager.Instance.Show(select);
                            isNothing = true;
                            kamishibai.sleep(()=>{
                                // index値を更新
                                index = kamishibai.TimelineReader.GetLabelIndex(select.Selected.Key);
                                isNothing = false;
                                Debug.Log("nothing end");
                                this.play();
                            }, descrimination:() => {
                                return select.Selected != null;
                            });
                        } else {
                            this.play(line: new string[] {
                                "goto",
                                string.Format("{0}", line[2]),
                            });
                        }
                        break;
                    case "label":
                        this.next();
                        break;
                    case "goto":
                        // goto,label_name
                        string goTo = line[1];
                        int gotoIndex = kamishibai.TimelineReader.GetLabelIndex(goTo);
                        // 0だと失敗なので無視
                        if (gotoIndex > 0) this.index = gotoIndex;
                        this.play();
                        break;
                    case "nothing":
                        // nothing@4000
                        Debug.Log("nothing start");
                        float waitTime = float.Parse(line[0].Split('@')[1]);
                        isNothing = true;
                        kamishibai.sleep(() => {
                            isNothing = false;
                            this.next();
                            Debug.Log("nothing end");
                        }, waitTime:waitTime);
                        break;
                    default:
                        break;
                }
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
        public string[] GetNextLine () {
            string[] nextLine = datas[GetIndex() + 1];
            return nextLine;
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
        private Dictionary<string, int> labelMap = new Dictionary<string, int>();
        private int height = 0;
        private Dictionary<string, string> identifier = new Dictionary<string, string> () {
            { "header", "[head]" },
            { "body", "[body]" },
            { "label", "label" },
        };
        public TimelineReader (TextAsset csvFile) {
            this.csvFile = csvFile;
            StringReader reader = new StringReader (csvFile.text);
            bool header = false;
            bool body = false;
            int bodyIndex = 0;
            while (reader.Peek () > -1) {
                string line = reader.ReadLine ();
                if (line == identifier["header"]) {
                    header = true;
                    body = false;
                } else if (line == identifier["body"]) {
                    header = false;
                    body = true;
                // 空白は無視する
                } else if (line.Length > 0) {
                    string[] splitted = line.Split(',');
                    if (header) timeLineHead.Add(splitted); // リストに入れる
                    else if (body) timeLineBody.Add (splitted); // リストに入れる
                    // labelMapに登録する
                    // label,label_name
                    if (splitted[0] == identifier["label"]) labelMap[splitted[1]] = bodyIndex;
                    if (body) bodyIndex += 1;
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
        public int GetLabelIndex(string label) {
            if (labelMap.ContainsKey(label)) return labelMap[label];
            return 0;
        }
    }
}
