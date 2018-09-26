using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;
using UnityEngine.SceneManagement;
public class KamishibaiController : MonoBehaviour {
    public Canvas canvas;
    public GameObject jibunBalloon;
    public GameObject aiteBalloon;
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
    private Narrator narration;
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
    public HeadHandler GetHeadHandler()
    {
        return headHandler;
    }

    // 与えられた画像を背景に設置
    public void setBackground (string path) {
        title.text = "";
        background.sprite = Resources.Load<Sprite>(path);
    }
    // narrationを表示
    public void setNarration (string text) {
        narration.set(text);
    }
    public void hideNarration () {
        narration.hide();
    }
    // タイトルの表示
    public void setTitle (string value) {
        title.text = value;
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

            Actor.GetComponent<RectTransform>().SetParent(canvas.transform);
            Actor.SetActive(true);

            ActorRectTrans.anchoredPosition = anchoredPosition;
            ActorRectTrans.localScale = new Vector3(5f, 5f, 1f);
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

    // Use this for initialization
    void Start () {
        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        background = GameObject.Find("/Canvas/Background").GetComponent<Image>();
        title = GameObject.Find("/Canvas/Title").GetComponent<Text>();

        narration = new Narrator();
        narration.GameObject.transform.SetParent(canvas.transform);

        reader = new TimelineReader(timeline);
        timelineHead = reader.GetTimeLineHead();
        timelineBody = reader.GetTimeLineBody();
        headHandler = new HeadHandler(this, timelineHead);
        bodyHandler = new BodyHandler(this, timelineBody);
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Return)) {
            if (bodyHandler.GetIndex() < bodyHandler.GetLimit()) {
                bodyHandler.play();
            } else {
                setTitle("おしまい");
            }
        }
    }
}

