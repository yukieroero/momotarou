using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AudioClipの機能拡張
/// ループのポイントや
/// 1フレームごとに増加する音量
/// を記憶している
/// </summary>
public class AdvancedAudioClip {
    AudioClip instance;
    float loopStart;
    float loopEnd;
    float volume; // 再生される音量 fedeする場合も目標値が入る
    float duration;
    float gainPerSecond;
    public AdvancedAudioClip(AudioClip clip) {
        instance = clip;
        // デフォルトでは曲頭から曲末までループする
        this.SetLoop(0f, clip.length);
    }
    public void SetLoop(float loopStart, float loopEnd) {
        this.loopStart = loopStart;
        if (loopEnd == 0) loopEnd = this.instance.length;
        this.loopEnd = loopEnd;
    }
    public void SetFadeMeta(float volume, float duration, float currentVolume) {
        // volume まで durationずつ音量を変化させたい
        this.volume = volume;
        this.duration = duration;
        // 毎秒変化させたい音量 = 変化させる音量 / 何秒で変化させるか
        this.gainPerSecond = (volume - currentVolume) / ((float)this.duration / 1000f);
    }

    public AudioClip Instance
    {
        get
        {
            return instance;
        }
    }

    public float LoopStart
    {
        get
        {
            return loopStart;
        }
    }
    public float LoopEnd
    {
        get
        {
            return loopEnd;
        }
    }
    public float Volume
    {
        get
        {
            return volume;
        }
    }
    public float GainPerSecond
    {
        get
        {
            return gainPerSecond;
        }
    }
}

/// <summary>
/// BGMとSEの管理をするマネージャ。シングルトン。
/// </summary>
public class AudioManager : SingletonMonoBehaviour<AudioManager> {
    private float volume = 1;
    // BGM用 ループ用 SE用
    private AudioSource AttachBGMSource, AttachBGMSourceSub, AttachSESource;
    // BGM用クリップ ループ用クリップ SE用クリップ
    private AdvancedAudioClip AttachBGMSourceClip, AttachBGMSourceSubClip, AttachSESourceClip;
    // 全データ
    private Dictionary<string, AdvancedAudioClip> _bgmDic, _seDic;
    public AdvancedAudioClip getBGM(string path) {
        return _bgmDic[path];
    }

    private bool isPlaying;
    private bool isFadeIn;
    private bool isFadeOut;

    /// <summary>
    /// BGMフェードインフェードアウトの実装
    /// <param name="bgmId">対象ファイル名</param>
    /// <param name="volume">音量 0だとフェードアウトになる</param>
    /// <param name="duration">フェードインフェードアウトにかかる時間 0だと即時再生、停止</param>
    /// <param name="loop">ループの可否</param>
    /// </summary>
    public void fadeBGM(string bgmId, float duration, float volume, bool loop=false) {
        AdvancedAudioClip clip = this.getBGM(bgmId);
        if (volume > 0) {
            isPlaying = true;
            isFadeIn = true;
            isFadeOut = false;
            AttachBGMSourceClip = clip;
            AttachBGMSource.clip = clip.Instance;
            // フェードインはボリューム0スタート
            AttachBGMSource.volume = 0;
            AttachBGMSource.playOnAwake = false;
            AttachBGMSource.Play();
        } else {
            isFadeIn = false;
            isFadeOut = true;
        }
        clip.SetFadeMeta(volume, duration, AttachBGMSource.volume);
    }
    void fadeIn() {
        float targetVolume = AttachBGMSourceClip.Volume;
        // 毎フレーム変化させる音量 = 毎秒変化させたい音量 * (秒/1f) <= 1fあたりの秒数
        float gainPerFrame = AttachBGMSourceClip.GainPerSecond * Time.deltaTime;
        if (AttachBGMSource.volume < targetVolume) AttachBGMSource.volume += gainPerFrame;
        // 終了
        else isFadeIn = false;
    }
    void fadeOut() {
        // 毎フレーム変化させる音量 = 毎秒変化させたい音量 * (秒/1f) <= 1fあたりの秒数
        float gainPerFrame = AttachBGMSourceClip.GainPerSecond * Time.deltaTime;
        if (AttachBGMSource.volume > 0) AttachBGMSource.volume += gainPerFrame;
        // 終了
        else isFadeOut = false; isPlaying = false; AttachBGMSource.Stop();
    }

    /// <summary>
    /// SEの再生
    /// </summary>
    /// <param name="filename">再生したいSEのファイル名</param>
    /// <param name="volume">音量</param>
    public void playSE(string filename, int volume) {}
    // Use this for initialization
    void Start () {
        // リソース読み込み
        _bgmDic = new Dictionary<string, AdvancedAudioClip>();
        _seDic = new Dictionary<string, AdvancedAudioClip>();
        object[] bgmList = Resources.LoadAll("Audio/BGM", typeof(AudioClip));
        object[] seList = Resources.LoadAll("Audio/SE", typeof(AudioClip));

        // loopの設定とかしたいのでAudioClipクラス拡張した方が良いかも
        // http://tsubakit1.hateblo.jp/entry/20131201/1385909300
        foreach (AudioClip bgm in bgmList) {
            _bgmDic[string.Format("BGM/{0}", bgm.name)] = new AdvancedAudioClip(bgm);
        }
        foreach (AudioClip se in seList) {
            AdvancedAudioClip clip = new AdvancedAudioClip(se);
            _seDic[string.Format("SE/{0}", se.name)] = new AdvancedAudioClip(se);
        }
        // シーンをまたいでも削除されないようにする
        DontDestroyOnLoad (this.gameObject);
        // sourceのアタッチ
        AttachBGMSource = this.gameObject.AddComponent<AudioSource>();
        AttachBGMSourceSub = this.gameObject.AddComponent<AudioSource>();
        AttachSESource = this.gameObject.AddComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {
        if (isFadeIn) {
            fadeIn();
        }
        if (isFadeOut) {
            fadeOut();
        }
    }
}
