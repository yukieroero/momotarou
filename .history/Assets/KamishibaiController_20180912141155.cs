﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class KamishibaiController : MonoBehaviour {
    public Canvas canvas;
    public GameObject jibunBalloon ;
    public GameObject aiteBalloon ;
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

    public void action(Person actor, string position, string serif) {
        Sprite actorImage = Resources.Load<Sprite>(actor.GetImage());
        GameObject Actor = new GameObject(); //Create the GameObject
        Actor.name = actor.GetIdentifier();
        Image ActorImage = Actor.AddComponent<Image>(); //Add the Image Component script
        RectTransform ActorRectTrans = Actor.GetComponent<RectTransform>();

        Vector2 anchoredPosition = new Vector2(0, 0);
        if (position == "left") {
            ActorImage.transform.Rotate(new Vector3(0f, 180f, 0f));
            anchoredPosition.x = -(canvasWidth - ActorImage.rectTransform.rect.width) / 2;
        } else if (position == "right") {
            anchoredPosition.x = (canvasWidth - ActorImage.rectTransform.rect.width) / 2;
        }

        // セリフの表示
        GameObject balloon = jibunBalloon;
        if (position == "right") {
            balloon = jibunBalloon;
        } else if (position == "left") {
            balloon = aiteBalloon;
        }
        balloon.GetComponentInChildren<Text>().text = serif;
        balloon = Instantiate(balloon);
        balloon.transform.SetParent(Actor.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
        balloon.SetActive(true);
        // TODO: 位置調整 pos x = -150
        Actor.GetComponent<RectTransform>().SetParent(canvas.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
        Actor.SetActive(true);


        ActorImage.sprite = actorImage; //Set the Sprite of the Image Component on the new GameObject
        ActorImage.preserveAspect = true;

        ActorRectTrans.anchoredPosition = anchoredPosition;
        ActorRectTrans.localScale = new Vector3(1f, 1f, 1f);

        actor.Actor = Actor;
    }

    public void act(Person actor, string serif, string position) {
        // positionに対応する値は(left|right)
        // 画像を表示させる
        action(actor, position, serif);
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
            bodyHandler.play();
        }
    }
}
