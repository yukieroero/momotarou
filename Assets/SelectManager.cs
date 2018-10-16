using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Timeline;

public class Option {
    string caption;
    string key;
    string val;
    bool selected;
    Select select; // 親オブジェクト
    public Option(string caption, string expr) {
        this.caption = caption;
        this.key = expr;
        this.val = "";
        if (expr.Contains("=")) {
            string[] keyVal = expr.Split('=');
            this.key = keyVal[0].Trim();
            this.val = keyVal[1].Trim();
        }
        this.selected = false;
    }
    public bool IsLabelJump() {
        return val == "";
    }
    public string Key {
        get {
            return this.key;
        }
    }
    public string Caption {
        get {
            return this.caption;
        }
    }
    public Select Select {
        set {
            this.select = value;
        }
        get {
            return this.select;
        }
    }
    public void Choice() {
        this.selected = true;
    }
    public bool Selected {
        get {
            return this.selected;
        }
    }
}

public class Select {
    string caption;
    string label;
    List<Option> options;
    Option selected;
    public Select(string caption, string label) {
        this.caption = caption;
        this.label = label;
        this.options = new List<Option>();
    }
    public void AddOption(string caption, string expr) {
        Option option = new Option(caption, expr);
        option.Select = this;
        options.Add(option);
    }
    public int Count(bool available=false) {
        // available=trueの場合は未選択のOption数を返す
        int count = options.Count;
        if (available) {
            foreach (Option op in this.options) {
                if (op.Selected) count -= 1;
            }
        }
        return count;
    }
    public string Caption {
        get {
            return this.caption;
        }
    }
    public string Label {
        get {
            return this.label;
        }
    }
    public List<Option> Options {
        get {
            return this.options;
        }
    }
    public Option Selected {
        get {
            return this.selected;
        }
    }
    public void Choice(Option op) {
        op.Choice();
        this.selected = op;
    }
    public void selectNull() {
        this.selected = null;
    }
}

public class SelectManager : SingletonMonoBehaviour<SelectManager> {
    private float fadeOpacity = 0.7f;
    private float fadeDuration = 400f;
    Dictionary<string, Select> selects;
   /// <summary>
    /// select manager
    /// </summary>
    /// <example>
    /// <code>
    /// SelectWrapper
    ///  └ Select
    ///    ├ option
    ///    ├ option
    ///    └ option
    /// </code>
    /// </example>
    new void Awake() {
        // 親クラスのAwakeを実行
        base.Awake();
        gameObject.name = "SelectManager";

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.planeDistance = 0;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = GameObject.Find("/Main Camera").GetComponent<Camera>();
        RectTransform rectTransform = TimelineHelper.addFillRect(gameObject);
        rectTransform.sizeDelta= new Vector2(Screen.width, Screen.height);

        // アタッチしないとonclickが捕捉できない涙
        gameObject.AddComponent<GraphicRaycaster>();
        gameObject.AddComponent<CanvasGroup>();

        gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, fadeOpacity);

        GameObject selectArea = new GameObject("Select");
        selectArea.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        VerticalLayoutGroup vlayout = selectArea.AddComponent<VerticalLayoutGroup>();
        TimelineHelper.AddParam(vlayout, new Hashtable(){
            {"childAlignment", TextAnchor.MiddleCenter},
            {"childControlWidth", true},
            {"childControlHeight", true},
            {"childForceExpandWidth", false},
            {"childForceExpandHeight", false},
            {"padding.top", 30},
            {"padding.bottom", 30},
            {"spacing", 30},
        });
        Text selectCaption = selectArea.AddComponent<Text>();
        TimelineHelper.setTextStyle(selectCaption, 30, Color.white, Resources.Load<Font>("fonts/hiragino"));
        TimelineHelper.AddParam(selectCaption, new Hashtable(){
            {"selectCaption.alignment", TextAnchor.UpperCenter},
        });
        selectArea.transform.SetParent(gameObject.transform);
        // wrapper.transform.SetParent(null);

        TimelineHelper.setRectPosition(TimelineHelper.addFillRect(selectArea), 0, 0, 0, 0);

        selects = new Dictionary<string, Select>();

        // シーンをまたいでも削除されないようにする
        DontDestroyOnLoad (this.gameObject);
    }

    /// <summary>
    /// フラグ管理dictを作成
    /// </summary>
    /// <param name="lines"></param>
    public void Load(List<string[]> lines) {
        for(int i=0;i<lines.Count;i++) {
            string[] line = lines[i];
            if (line[0] == "select") {
                // select,このあとどうする？,label_1_end
                Select select = new Select(line[1], line[2]);
                // 次の行
                i += 1;
                line = lines[i];
                while (line[0] == "select_option") {
                    // select_option,caption,expr,type
                    select.AddOption(line[1], line[2]);
                    i += 1;
                    line = lines[i];
                }
                // 次の行をすっとばさないようにdecrementする
                i -= 1;
                selects[select.Label] = select;
            }
        }
    }
    public bool IsSelected(string label) {
        foreach(Select select in selects.Values) {
            foreach(Option option in select.Options) {
                if (option.Key == label) return option.Selected;
            }
        }
        return false;
    }
    public Select GetSelect(string label) {
        return this.selects[label];
    }

    public void Show (Select select) {
        // 繰り返し同じ選択肢を表示する場合、Selectedをnullにしておかないと無限ループしてしまう
        if (select.Selected != null) select.selectNull();
        Transform selectArea = this.transform.Find("Select");
        selectArea.GetComponent<Text>().text = select.Caption;
        foreach(Option op in select.Options){
            GameObject option = this.createOptionObject(op);
            option.transform.SetParent(selectArea);
            option.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        }
        // 選択肢は通常のCanvasとは別にその上に表示されるが、なぜか一番上の選択肢が選択できなくなるので
        // 選択肢を表示する際に、一旦タッチ判定をなくす
        GameObject.Find("/Canvas").GetComponent<GraphicRaycaster>().enabled = false;
        // 表示
        Hashtable hash = new Hashtable(){
            {"name", "SelectFader"},
            {"from", 0f},
            {"to", fadeOpacity},
            {"time", fadeDuration / 1000f},
            {"onupdatetarget", gameObject},
            {"onupdate", "fadeHandler"},
        };
        iTween.ValueTo(gameObject, hash);
    }
    private void fadeHandler(float value) {
        gameObject.GetComponent<CanvasGroup>().alpha = value;
    }
    private void actionCallback(System.Action callback) {
        callback();
    }

    public class WaitForSelect : CustomYieldInstruction {
        Select select;
        public WaitForSelect(Select select) {
            this.select = select;
        }
        public override bool keepWaiting {
            get {
                return this.select.Selected == null;
            }
        }
    }
    public IEnumerator WaitForChoice(Select select, System.Action callback=null) {
        yield return new WaitForSelect(select);
        this.Hide();
        // fadeDuration分待つ
        yield return new WaitForSeconds(fadeDuration / 1000f);
        if (callback != null) callback();
    }
    public void Hide () {
        // 非表示
        Hashtable hash = new Hashtable(){
            {"name", "SelectFader"},
            {"from", gameObject.GetComponent<CanvasGroup>().alpha},
            {"to", 0f},
            {"time", fadeDuration / 1000f},
            {"onupdatetarget", gameObject},
            {"onupdate", "fadeHandler"},
            {"oncompletetarget", gameObject},
            {"oncomplete", "actionCallback"},
        };
        // 透明になって見えなくなったら表示されていたOptionを削除する
        System.Action callback = () => {
            Transform selectArea = this.transform.Find("Select");
            Transform[] children = selectArea.GetComponentsInChildren<Transform>();
            foreach(Transform child in children) {
                if (child != selectArea) {
                    Destroy(child.gameObject);
                }
            }
            GameObject.Find("/Canvas").GetComponent<GraphicRaycaster>().enabled = true;
        };
        hash.Add("oncompleteparams", callback);
        iTween.ValueTo(gameObject, hash);
    }

    GameObject createOptionObject(Option op) {
        GameObject option = new GameObject("Option");
        // これをアタッチしないとonclickが発火しない涙
        option.AddComponent<CanvasRenderer>();

        Button optionButton = option.AddComponent<Button>();
        Color buttonTextColor = Color.black;
        // 未選択の場合のみ選択アクションを作成
        if (op.Selected == false) {
            optionButton.onClick.AddListener(() => {onClickOptionHandler(op); });
            buttonTextColor = Color.red;
        }

        // buttonのテキスト部分
        GameObject buttonText = new GameObject("text");
        Text textObj = buttonText.AddComponent<Text>();
        textObj.text = op.Caption;
        TimelineHelper.setTextStyle(textObj, 16, buttonTextColor, Resources.Load<Font>("fonts/hiragino"));
        textObj.transform.SetParent(option.transform);


        VerticalLayoutGroup vlayout = option.AddComponent<VerticalLayoutGroup>();
        TimelineHelper.AddParam(vlayout, new Hashtable(){
            {"padding.top", 30},
            {"padding.bottom", 30},
            {"childAlignment", TextAnchor.MiddleCenter},
            {"childControlWidth", true},
            {"childControlHeight", true},
            {"childForceExpandWidth", false},
            {"childForceExpandHeight", false},
        });
        return option;
    }
    void onClickOptionHandler (Option op) {
        // TODO: selectedに値を詰める
        op.Select.Choice(op);
        Debug.LogFormat("Selected: {0}", op.Caption);
    }

    void Start () {
    }

    void OnDestroy() {
        Debug.Log("きえます、さよなら");
    }
    // Update is called once per frame
    void Update () {
    }
}
