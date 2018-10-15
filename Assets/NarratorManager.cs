using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class NarratorManager : SingletonMonoBehaviour<NarratorManager> {

    bool isFading;
    Text narrationText; // このtextプロパティを書き換えると反映される
    float duration; // 何msでふぇーどさせるか
    GameObject background;

    /// <summary>
    /// GameObjectを作成する
    /// </summary>
    /// <example>
    /// <code>
    /// NarrationManger
    /// ├ Background
    /// └ Text
    /// </code>
    /// </example>
    new void Awake () {
        // 親クラスのAwakeを実行
        base.Awake();

        // NarrationManagerの初期化
        gameObject.name = "NarratorManager";
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.planeDistance = 10;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = GameObject.Find("/Main Camera").GetComponent<Camera>();
        RectTransform rectTransform = TimelineHelper.addFillRect(gameObject);
        rectTransform.sizeDelta= new Vector2(Screen.width, Screen.height);
        gameObject.AddComponent<CanvasGroup>().alpha = 0;

        // 背景の初期化
        background = new GameObject();
        background.name = "background";
        RectTransform backgroundRect = TimelineHelper.addFillRect(background);
        background.AddComponent<Image>().color = new Color(0, 0, 0, 1f);
        background.transform.SetParent(gameObject.transform);
        Timeline.TimelineHelper.setRectPosition (backgroundRect, 0, 0, 0, 0);

        // narrationTextの初期化
        GameObject narrationText = new GameObject();
        narrationText.name = "Text";
        RectTransform narrationTextRect = TimelineHelper.addFillRect(narrationText);
        Text text = narrationText.AddComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        Timeline.TimelineHelper.setTextStyle (text, 30, Color.white, Resources.Load<Font> ("fonts/hiragino"));
        narrationText.transform.SetParent(gameObject.transform);
        Timeline.TimelineHelper.setRectPosition (narrationTextRect, 0, 0, 0, 0);

        this.narrationText = text;

        // シーンをまたいでも削除されないようにする
        DontDestroyOnLoad (gameObject);
    }
    // Use this for initialization
    void Start () {
    }
    void fadeHandler(float alpha) {
        gameObject.GetComponent<CanvasGroup>().alpha = alpha;
    }

    /// <summary>
    /// narrationを表示する準備
    /// 実際の処理はfade
    /// </summary>
    /// <param name="duration">0 > であればフェードイン</param>
    public void show(string text, float duration) {
        this.duration = duration;
        narrationText.text = text.Replace("\\r\\n", "\r\n");
        Hashtable hash = new Hashtable(){
            {"name", "NarrationFader"},
            {"from", 0f},
            {"to", 0.7f},
            {"time", duration / 1000f},
            {"onupdatetarget", gameObject},
            {"onupdate", "fadeHandler"},
        };
        iTween.ValueTo(background, hash);
    }
    /// <summary>
    /// narrationを非表示にする準備
    /// 実際の処理はfade
    /// </summary>
    /// <param name="duration">0 > であればフェードアウト</param>
    public void hide(float duration) {
        this.duration = duration;
        Hashtable hash = new Hashtable(){
            {"name", "NarrationFader"},
            {"from", gameObject.GetComponent<CanvasGroup>().alpha},
            {"to", 0f},
            {"time", duration / 1000f},
            {"onupdatetarget", gameObject},
            {"onupdate", "fadeHandler"},
        };
        iTween.ValueTo(gameObject, hash);
    }

    // Update is called once per frame
    void Update () {
    }
}
