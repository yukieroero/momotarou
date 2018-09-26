using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleController : MonoBehaviour {

    private Sprite BackgroundImage;
    private string TitleText = "タップして始める";
    // Use this for initialization
    void Start () {
        BackgroundImage = Resources.Load<Sprite>("/Resources/title");
        Debug.Log(BackgroundImage);
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
