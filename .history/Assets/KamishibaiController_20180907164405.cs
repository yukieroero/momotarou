using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class KamishibaiController : MonoBehaviour {
    public Canvas canvas;
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

    public HeadHandler GetHeadHandler()
    {
        return headHandler;
    }

    // 与えられた画像を背景に設置
    public void setBackground (string path) {
        title.text = "";
        background.sprite = Resources.Load<Sprite>(path);
    }

    public void act(Person actor, string serif, string position) {
        Sprite actorImage = Resources.Load<Sprite>(actor.GetImage());
        GameObject NewObj = new GameObject(); //Create the GameObject
        Image NewImage = NewObj.AddComponent<Image>(); //Add the Image Component script
        NewImage.preserveAspect = true;
        RectTransform NewRectTrans = NewObj.AddComponent<RectTransform>(); //Add the Image Component script
        Vector2 anchoredPosition = new Vector2(0, 0);
        if (position == "left") {
            NewImage.transform.Rotate(new Vector3(0f, 90f, 0f));
            anchoredPosition.x = -canvasWidth / 2 + NewImage.rectTransform.rect.width;
        } else if (position == "right") {
            anchoredPosition.x = canvasWidth / 2 - NewImage.rectTransform.rect.width;
        }
        NewImage.sprite = actorImage; //Set the Sprite of the Image Component on the new GameObject
        NewObj.GetComponent<RectTransform>().SetParent(canvas.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
        NewRectTrans.anchoredPosition = anchoredPosition;
        NewObj.SetActive(true);
    }
    public void setTitle (string value) {
        title.text = value;
    }

    // Use this for initialization
    void Start () {
        canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
        canvasHeight = canvas.GetComponent<RectTransform>().rect.heigt;
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
            bodyHandler.play();
        }
    }
}

