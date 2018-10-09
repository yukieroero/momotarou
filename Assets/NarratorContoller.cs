using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Timeline;

public class NarratorContoller : MonoBehaviour {

    public bool isFading;
    private bool fadingIn;
    private bool fadingOut;
    private GameObject narrator;
    private GameObject narrationText;
    private CanvasGroup narrationMask;
    float fadeInSpeed = 0.03f;
    float targetOpacity = 0.7f;
    void Awake() {
        fadingIn = false;
        fadingOut = false;
    }
    void Start () {
        narrator = this.gameObject;
        narrationMask = narrator.GetComponent<CanvasGroup>();
        narrationText = narrator.transform.Find("Text").gameObject;
    }

    void fadeIn() {
        float currentOpacity = narrationMask.alpha;
        float newOpacity = currentOpacity + fadeInSpeed;
        // targetOpacityになったら更新を止める
        if (newOpacity >= targetOpacity) {
            fadingIn = false;
            fadingOut = false;
        }
        narrationMask.alpha = newOpacity;
    }
    void fadeOut() {
        float currentOpacity = narrationMask.alpha;
        float newOpacity = currentOpacity - fadeInSpeed;
        // 0になったら更新を止める
        if (newOpacity <= 0f) {
            fadingIn = false;
            fadingOut = false;
        }
        narrationMask.alpha = newOpacity;
    }
    public void show(){
        this.gameObject.SetActive(true);
        isFading = true;
        fadingIn = true;
        fadingOut = !fadingIn;
        StartCoroutine(OnShown(()=>{
            Debug.Log("あらわれた");
            isFading = false;
        }));
    }
    IEnumerator OnShown (System.Action callback) {
        while(true) {
            yield return new WaitForFixedUpdate();
            if (narrationMask.alpha >= targetOpacity) {
                callback();
                break;
            }
        }
    }
    public void hide (){
        isFading = true;
        fadingOut = true;
        fadingIn = !fadingOut;
        StartCoroutine(OnHiden(()=>{
            Debug.Log("きえた");
            isFading = false;
            this.gameObject.SetActive(false);
        }));
    }
    IEnumerator OnHiden (System.Action callback) {
        while(true) {
            yield return new WaitForFixedUpdate();
            if (narrationMask.alpha <= 0f) {
                callback();
                break;
            }
        }
    }
    // Update is called once per frame
    void Update () {
        if (fadingIn == true) fadeIn();
        if (fadingOut == true) fadeOut();
    }
}
