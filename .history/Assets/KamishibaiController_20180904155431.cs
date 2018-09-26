using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KamishibaiController : MonoBehaviour {

    public Image background;
    public Image backgroundInaka;
    public Image backgroundRiver;
    public Image backgroundMountain;
    public Image oldman;
    public Image oldwoman;
    // 与えられた画像を背景に設置
    void setBackground (Image background) {

    }

    // Use this for initialization
    void Start () {
        setBackground(backgroundInaka);
    }

    // Update is called once per frame
    void Update () {

    }
}
