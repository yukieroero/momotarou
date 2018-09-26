﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleController : MonoBehaviour {

    private Sprite BackgroundImage = Resources.Load<Sprite>("/Resources/title");
    private string TitleText = "ももたろう";
    // Use this for initialization
    void Start () {
        GameObject background = this.transform.Find("Background").gameObject;
        background.GetComponent<Image>().sprite = BackgroundImage;
        GameObject titleText = this.transform.Find("TitleText").gameObject;
        titleText.GetComponent<Text>().text = TitleText;
        background.SetActive(false);
        titleText.SetActive(false);
    }

    // Update is called once per frame
    void Update () {

    }
}