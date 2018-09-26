using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

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
    private Person previousActor;
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

    public void action(Person actor, string serif, string position) {
        // positionに対応する値は(left|right)
        // すでに登場して、表示されていたら削除
        GameObject Actor = new GameObject(); //Create the GameObject
        if (actor.Actor && actor.BalloonList.Count > 0) {
            Destroy(actor.Actor);
        }
        Sprite actorImage = Resources.Load<Sprite>(actor.GetImage());
        Actor.name = actor.GetIdentifier();
        Image ActorImage = Actor.AddComponent<Image>(); //Add the Image Component script
        RectTransform ActorRectTrans = Actor.GetComponent<RectTransform>();
        ActorRectTrans.anchorMin = new Vector2(0.5f, 0f);
        ActorRectTrans.anchorMax = new Vector2(0.5f, 0f);

        Vector2 anchoredPosition = new Vector2(0, (float)ActorImage.rectTransform.rect.height / 2 * 5);
        if (position == "left") {
            ActorImage.transform.Rotate(new Vector3(0f, 180f, 0f));
            anchoredPosition.x = -(canvasWidth - ActorImage.rectTransform.rect.width) / 2;
        } else if (position == "right") {
            anchoredPosition.x = (canvasWidth - ActorImage.rectTransform.rect.width) / 2;
        }
        // セリフの表示
        Balloon balloon = new Balloon(serif, position);
        // サイズ調整
        // balloon.GameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(canvasWidth, 0f);
        // chatviewにセリフを表示
        GameObject chatview = GameObject.Find("/Canvas/ChatView/Viewport/Content");
        balloon.GameObject.transform.SetParent(chatview.transform);
        balloon.GameObject.SetActive(true);
        balloon.GameObject.transform.localScale = new Vector3(1f, 1f, 1f);

        Actor.GetComponent<RectTransform>().SetParent(canvas.transform);
        Actor.SetActive(true);

        RectTransform balloonUIRect = balloon.GameObject.transform.Find("serif/balloonUI").GetComponent<RectTransform>();
        ContentSizeFitter balloonUIFitter = balloon.GameObject.transform.Find("serif/balloonUI").GetComponent<ContentSizeFitter>();
        float maxWidth = 400f;
        if (balloonUIRect.sizeDelta.x > maxWidth) {
            balloonUIFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            balloonUIRect.sizeDelta = new Vector2(maxWidth, balloonUIRect.sizeDelta.y);
        }

        ActorImage.sprite = actorImage;
        ActorImage.preserveAspect = true;

        ActorRectTrans.anchoredPosition = anchoredPosition;
        ActorRectTrans.localScale = new Vector3(5f, 5f, 1f);

        actor.BalloonList.Add(balloon);
        actor.Actor = Actor;
        previousActor = actor;
        // balloonが描画された後でないとスクロール位置がずれるので、
        // coroutineを利用して画面のスクロール
        StartCoroutine(ScrollToBottom(chatview));
    }

    public void setTitle (string value) {
        title.text = value;
    }

    // Use this for initialization
    void Start () {
        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.GetComponent<RectTransform>().rect.height;
        background = GameObject.Find("Canvas/Background").GetComponent<Image>();
        title = GameObject.Find("Canvas/Title").GetComponent<Text>();
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

