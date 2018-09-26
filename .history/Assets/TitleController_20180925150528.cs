using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour {

    private Sprite BackgroundImage;
    private AudioClip BackgroundMusic;
    private string textForStart = "タップして始める";
    private GameObject Background;
    private GameObject TitleText;
    void tapHandler (PointerEventData e) {
        Debug.Log(e);
        SceneManager.LoadScene("Main");
    }
    // Use this for initialization
    void Start () {
        BackgroundImage = Resources.Load<Sprite>("title");
        BackgroundMusic = Resources.Load<AudioClip>("bgm/op");

        Background = this.transform.Find("Background").gameObject;

        AudioSource bgm = Background.AddComponent<AudioSource>();
        bgm.clip = BackgroundMusic;
        bgm.loop = true;
        bgm.Play();

        Background.GetComponent<Image>().sprite = BackgroundImage;
        TitleText = this.transform.Find("TitleText/Text").gameObject;
        TitleText.GetComponent<Text>().text = textForStart;
        // Background.SetActive(false);
        // TitleText.SetActive(false);
        EventTrigger tapEvent = TitleText.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { tapHandler((PointerEventData)data); });
        tapEvent.triggers.Add(entry);
    }

    // Update is called once per frame
    void Update () {
    }
}
