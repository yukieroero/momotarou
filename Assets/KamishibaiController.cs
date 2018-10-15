using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;
using Audio;
using UnityEngine.SceneManagement;
public class KamishibaiController : MonoBehaviour {
    private float canvasWidth;
    private float canvasHeight;

    public TextAsset timeline;
    private TimelineReader reader;
    private List<string[]> timelineHead;
    private List<string[]> timelineBody;

    private HeadHandler headHandler;
    private BodyHandler bodyHandler;

    private Image background;
    private Text title;
    private Dictionary<string, Person> previousActor = new Dictionary<string, Person>(){
        {"left", null},
        {"right", null},
    };
    IEnumerator ScrollToBottom(GameObject content) {
        /* contentを最下部までスクロールする */
        // 1フレーム、チャットバルーンの描画を待つ
        yield return new WaitForEndOfFrame();
        float ContentHeight = content.GetComponent<RectTransform>().sizeDelta.y;
        // 0より小さいときは1画面に収まっているのでスクロールなし
        if (ContentHeight > 0) {
            content.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, ContentHeight);
        }
    }
    public TimelineReader TimelineReader {
        get {
            return this.reader;
        }
    }
    public HeadHandler GetHeadHandler()
    {
        return headHandler;
    }
    // bgmを再生
    public void playAudio (string path, int fadeDuration, float volume, bool loop, float loopStart, float loopEnd) {
        AudioController controller = gameObject.GetComponent<AudioController>();
        controller.setLoop(loop, loopStart, loopEnd);
        if (fadeDuration > 0) controller.setFadeIn(fadeDuration, volume);
        controller.play(Resources.Load<AudioClip>(path));
    }
    // bgmをstop
    public void stopAudio (string path, int fadeDuration) {
        AudioController controller = gameObject.GetComponent<AudioController>();
        if (fadeDuration > 0) controller.setFadeOut(fadeDuration);
        controller.stop();
    }

    // 与えられた画像を背景に設置
    public void setBackground (string path) {
        title.text = "";
        background.sprite = Resources.Load<Sprite>(path);
    }
    // タイトルの表示
    public void setTitle (string value) {
        title.text = value;
    }

    public void offstage(Person actor) {
        // 吹き出しの削除
        foreach (Balloon balloon in actor.BalloonList) {
            Destroy(balloon.GameObject);
        }
        actor.BalloonList = new List<Balloon>();
        // 人物を削除
        Destroy(actor.Actor);
        actor.Actor = null;
        actor.atWing = false;
        previousActor["right"] = null;
        previousActor["left"] = null;
    }

    public void action(Person actor, string serif, string position) {
        // positionに対応する値は(left|right)
        GameObject Actor = actor.Actor;
        // Actorが作成されてないなら作成
        if (Actor == null) {
            Actor = new GameObject();
            Sprite actorImage = Resources.Load<Sprite>(actor.GetImage());
            Actor.name = actor.GetIdentifier();
            Image ActorImage = Actor.AddComponent<Image>();
            RectTransform ActorRectTrans = Actor.GetComponent<RectTransform>();
            ActorRectTrans.anchorMin = new Vector2(0.5f, 0f);
            ActorRectTrans.anchorMax = new Vector2(0.5f, 0f);

            Vector2 anchoredPosition = new Vector2(0, (float)ActorImage.rectTransform.rect.height / 2 * 5);
            if (position == "left") {
                ActorImage.transform.Rotate(new Vector3(0f, 180f, 0f));
                // anchoredPosition.x = -(canvasWidth - ActorImage.rectTransform.rect.width) / 2;
                anchoredPosition.x = -ActorImage.rectTransform.rect.width;
            } else if (position == "right") {
                // anchoredPosition.x = (canvasWidth - ActorImage.rectTransform.rect.width) / 2;
                anchoredPosition.x = ActorImage.rectTransform.rect.width;
            }
            ActorImage.sprite = actorImage;
            ActorImage.preserveAspect = true;

            Actor.GetComponent<RectTransform>().SetParent(this.gameObject.transform);
            Actor.SetActive(true);

            ActorRectTrans.anchoredPosition = anchoredPosition;
            ActorRectTrans.localScale = new Vector3(3f, 3f, 1f);
            ActorController actorController = Actor.AddComponent<ActorController>();
        }
        // 今回表示するpositionに別にactorが立っている場合は削除
        if ((previousActor[position] != null) && previousActor[position] != actor) {
            Destroy(previousActor[position].Actor);
            previousActor[position].Actor = null;
            previousActor[position].atWing = false;
        }
        // セリフの表示
        Balloon balloon = new Balloon(actor, serif, position);

        // サイズ調整
        balloon.GameObject.GetComponent<LayoutElement>().preferredWidth = canvasWidth * 4 / 5;
        // chatviewにセリフを表示
        GameObject chatview = GameObject.Find("/Canvas/ChatView/Viewport/Content");
        balloon.GameObject.transform.SetParent(chatview.transform);
        balloon.GameObject.SetActive(true);
        balloon.GameObject.transform.localScale = new Vector3(1f, 1f, 1f);

        // 昔のバルーンから三角を削除
        foreach (Person a in previousActor.Values) {
            if (a != null && a.BalloonList.Count > 0) {
                GameObject latestBalloonUIObj = a.BalloonList[a.BalloonList.Count - 1].GameObject.transform.Find("balloonUI").gameObject;
                latestBalloonUIObj.GetComponent<Image>().sprite = a.getBalloonImg();
                latestBalloonUIObj.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(30, 30, 30, 30);
            }
        }

        actor.BalloonList.Add(balloon);
        actor.Actor = Actor;
        previousActor[position] = actor;

        // 立ち位置調整
        if (actor.atWing == true) actor.backFromWing(position);
        if (position == "left") {
            // 左で人が喋り出すときは右の人は少しずれる
            if (previousActor["right"] != null && previousActor["right"].Actor != null) {
                if (previousActor["right"].atWing == false) previousActor["right"].goWing("right");
            }
        } else if (position == "right") {
            // 右で人が喋り出すときは左の人は少しずれる
            if (previousActor["left"] != null && previousActor["left"].Actor != null) {
                if (previousActor["left"].atWing == false) previousActor["left"].goWing("left");
            }
        }
        // balloonが描画された後でないとスクロール位置がずれるので、
        // coroutineを利用して画面のスクロール
        // 新しいメッセージは毎回一番下に表示されるので、必要なくなりました
        // StartCoroutine(ScrollToBottom(chatview));
    }
    /// <summary>
    /// 渡された処理を指定時間後に実行する
    /// </summary>
    /// <param name="event">実行したい処理</param>
    /// <param name="waitTime">遅延時間[ミリ秒]</param>
    /// <param name="descrimination">判定用の関数</param>
    /// <returns></returns>
    public void sleep(System.Action action, float waitTime=0, System.Func<bool> descrimination=null) {
        StartCoroutine(_sleep(action, waitTime, descrimination));
    }
    private IEnumerator _sleep(System.Action action, float waitTime, System.Func<bool> descrimination)
    {
        while (true) {
            if (waitTime > 0) {
                yield return new WaitForSeconds(waitTime / 1000f);
                action();
                break;
            } else if (descrimination != null) {
                yield return new WaitForFixedUpdate();
                if (descrimination() == true) {
                    action();
                    break;
                }
            }
        }
    }

    // Use this for initialization
    void Awake () {
        Debug.Log("end of kamishibiai awake");
    }
    void Start () {
        Debug.Log("start of kamishibiai start");
        canvasWidth = this.gameObject.GetComponent<RectTransform>().rect.width;
        canvasHeight = this.gameObject.GetComponent<RectTransform>().rect.height;

        background = GameObject.Find("/Canvas/Background").GetComponent<Image>();
        title = GameObject.Find("/Canvas/Title").GetComponent<Text>();

        // narration = new Narrator();
        // narration.GameObject.transform.SetParent(this.gameObject.transform);
        // narration managerをセット
        new GameObject().AddComponent<NarratorManager>();

        reader = new TimelineReader(timeline);
        timelineHead = reader.GetTimeLineHead();
        timelineBody = reader.GetTimeLineBody();
        headHandler = new HeadHandler(this, timelineHead);
        bodyHandler = new BodyHandler(this, timelineBody);

        // 選択肢を管理する
        new GameObject().AddComponent<SelectManager>();
        SelectManager.Instance.Load(timelineBody);

        Debug.Log("end of kamishibiai start");
    }

    // Update is called once per frame
    void Update () {
        if (bodyHandler.GetIndex() == 0) bodyHandler.play();
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            if (bodyHandler.GetIndex() < bodyHandler.GetLimit()) {
                bodyHandler.play();
            } else {
                setTitle("おしまい");
            }
        }
        if (Input.GetKeyUp(KeyCode.Return)) {
            if (bodyHandler.GetIndex() < bodyHandler.GetLimit()) {
                bodyHandler.play();
            } else {
                setTitle("おしまい");
            }
        }
    }
}

