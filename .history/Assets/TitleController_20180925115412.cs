﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleController : MonoBehaviour, IPointerClickHandler {

    private Sprite BackgroundImage;
    private string textForStart = "タップして始める";
    private GameObject Background;
    private GameObject TitleText;

    void tapHandler (PointerEventData e) {
        Debug.Log(e);
    }
    // Use this for initialization
    void Start () {
        BackgroundImage = Resources.Load<Sprite>("title");
        Background = this.transform.Find("Background").gameObject;
        Background.GetComponent<Image>().sprite = BackgroundImage;
        TitleText = this.transform.Find("TitleText").gameObject;
        TitleText.GetComponent<Text>().text = textForStart;
        Background.SetActive(false);
        TitleText.SetActive(false);
        EventTrigger tapEvent = TitleText.AddComponent<EventTrigger>();
        tapEvent.OnPointerClick = tapHandler;


    }

    // Update is called once per frame
    void Update () {
    }
}
