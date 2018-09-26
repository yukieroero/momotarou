using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleController : MonoBehaviour {

    private Sprite BackgroundImage = Resources.Load<Sprite>("/Resources/title");
    private string titleText = "ももたろう";
    // Use this for initialization
    void Start () {
        GameObject background = this.transform.Find("Background").gameObject;
    }

    // Update is called once per frame
    void Update () {

    }
}
