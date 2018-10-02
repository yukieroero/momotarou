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
    AudioClip instance; // AudioClip本体
    float loopStart; // ループのスタート位置
    float loopEnd; // ループの終了位置
    float loopTime; // ループしている時間
    float destination; // 再生される音量 fedeする場合も目標値が入る
    float duration; // fadeにかける時間
    float gainPerSecond; // volume/秒
    float distance; // 総変化音量
    public AdvancedAudioClip(AudioClip clip) {
        instance = clip;
        // デフォルトでは曲頭から曲末までループする
        this.SetLoop(0f, clip.length);
    }
    public void SetLoop(float loopStart, float loopEnd) {
        this.loopStart = loopStart / 1000f;
        if (loopEnd == 0) loopEnd = this.instance.length * 1000;
        this.loopEnd = loopEnd / 1000f;
        this.loopTime = this.loopEnd - this.loopStart;
    }
    public void SetFadeMeta(float volume, float duration, float currentVolume) {
        // volume まで durationずつ音量を変化させたい
        this.destination = volume;
        this.duration = duration;
        // 毎秒変化させたい音量 = 変化させる音量 / 何秒で変化させるか
        this.gainPerSecond = (volume - currentVolume) / ((float)this.duration / 1000f);
        this.distance = volume - currentVolume;
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
    public float LoopTime
    {
        get
        {
            return loopTime;
        }
    }
    public float Destination
    {
        get
        {
            return destination;
        }
    }
    public float GainPerSecond
    {
        get
        {
            return gainPerSecond;
        }
    }
    public float Distance
    {
        get
        {
            return distance;
        }
    }
}

/// <summary>
/// BGMとSEの管理をするマネージャ。シングルトン。
/// </summary>
public class AudioManager : SingletonMonoBehaviour<AudioManager> {
    private float volume = 1;
    // BGM用 ループ用 SE用
    private List<AudioSource> AttachBGMSourceList, AttachSESourceList;
    // 全データ
    private Dictionary<string, AdvancedAudioClip> _bgmDic, _seDic;
    // 再生しているデータ
    private Dictionary<string, AudioSource> playingDic;
    /// <summary>
    /// ループ管理用
    /// {bgmId: nextLoopTime}
    /// </summary>
    private Dictionary<string, float> loopData;
    public AdvancedAudioClip getBGM(string path) {
        return _bgmDic[path];
    }
    private string nextBgmId;

    private static string LOOP_PREFIX = "LOOP___";
    AudioSource GetBGMSource(string bgmId) {
        AudioSource source;
        // 同一音源が再生中の場合は再生しているAudioSourceを取得, そうでない場合は、再生してないAudioSourceを取得
        if (playingDic.TryGetValue(bgmId, out source) == false) {
            foreach(AudioSource s in AttachBGMSourceList) {
                // ループ用音源の設定は、再生前のオリジナルの{s}が存在するので注意
                if (s.isPlaying == false && s.clip != getBGM(bgmId.Replace(LOOP_PREFIX, "")).Instance) {
                    source = s;
                    playingDic[bgmId] = source;
                    break;
                }
            }
        }
        // 空いてるAudioSourceがなければ追加する
        if (source == null) {
            source = this.gameObject.AddComponent<AudioSource>();
            AttachBGMSourceList.Add(source);
            playingDic[bgmId] = source;
        }
        // ボリュームは0にしておく
        if (source.isPlaying == false) source.volume = 0;
        source.playOnAwake = false;
        return source;
    }
    void fadeOutBGMAll(float duration=5000) {
        List<string> playingDicKeys = new List<string>(playingDic.Keys);
        foreach(string bgmId in playingDicKeys) {
            AudioSource source = GetBGMSource(bgmId);
            AdvancedAudioClip clip = this.getBGM(bgmId);
            clip.SetFadeMeta(volume, duration, source.volume);
        }
    }

    /// <summary>
    /// BGMフェードインフェードアウトの実装
    /// <param name="bgmId">対象ファイル名</param>
    /// <param name="volume">音量 0だとフェードアウトになる</param>
    /// <param name="duration">フェードインフェードアウトにかかる時間 0だと即時再生、停止</param>
    /// <param name="loop">ループの可否</param>
    /// </summary>
    public void fadeBGM(string bgmId, float duration, float volume, bool loop=false) {
        // 再生中のものがあったらフェードアウト
        // fadeOutBGMAll();
        AudioSource source = GetBGMSource(bgmId);
        AdvancedAudioClip clip = this.getBGM(bgmId);
        clip.SetFadeMeta(volume, duration, source.volume);
        if (source.clip != clip.Instance) source.clip = clip.Instance;
        if (loop) prepareLoop(bgmId);
        if (source.isPlaying == false) {
            // 変化量が正であればfadeinなので再生を開始
            if (clip.Distance > 0) source.Play();
        }
    }
    void fade() {
        List<string> playingBGMKeys = new List<string>(playingDic.Keys);
        foreach(string bgmId in playingBGMKeys) {
            string originalId = bgmId.Replace(LOOP_PREFIX, "");
            string loopId = LOOP_PREFIX + originalId;
            AdvancedAudioClip clip = getBGM(originalId);
            AudioSource source = playingDic[originalId];
            float startVolume = clip.Destination - clip.Distance;
            float currentVolume = source.volume;
            // 毎フレーム変化させる音量 = 毎秒変化させたい音量 * (秒/1f) <= 1fあたりの秒数
            float gainPerFrame = clip.GainPerSecond * Time.deltaTime;
            float nextVolume = currentVolume + gainPerFrame;

            if (Mathf.Abs(nextVolume - startVolume) < Mathf.Abs(clip.Distance)) {
                source.volume = nextVolume;
                // ループ用の音源があればそれも音量を変更しておく
                if (playingDic.TryGetValue(loopId, out source)) {
                    source.volume = nextVolume;
                }
            } else {
                // source.volume = nextVolume;
                if (source.isPlaying && clip.Destination <= 0) {
                    source.Stop();
                    playingDic.Remove(bgmId);
                    // ループ用
                    if (playingDic.TryGetValue(loopId, out source)) {
                        source.Stop();
                        playingDic.Remove(loopId);
                    }
                    if (loopData.ContainsKey(originalId)) loopData.Remove(originalId);
                }
            }
        }
    }
    /// <summary>
    /// ループ再生の準備を行う
    /// 再生の1秒前から準備を開始しておく
    /// </summary>
    /// <param name="bgmId">再生するファイルID</param>
    void prepareLoop(string bgmId) {
        AudioSource loopSource = GetBGMSource(LOOP_PREFIX + bgmId);
        AdvancedAudioClip clip = getBGM(bgmId);
        loopSource.clip = clip.Instance;
        // スケジュールがずれないようにすでにスケジュールされている場合は変更しないようにする
        if (loopData.ContainsKey(bgmId) == false) {
            // オリジナルを止める時間を設定
            AudioSource originalSource = GetBGMSource(bgmId);
            float currentTime = (float)AudioSettings.dspTime;
            originalSource.PlayScheduled(currentTime);
            // まだスケジュールされていない場合は規定の時間でスケジュールする
            float nextLoopTime = clip.LoopEnd + currentTime;
            // ループ用音源の再生時間を設定
            loopData[bgmId] = nextLoopTime;
            originalSource.SetScheduledEndTime(nextLoopTime);
        }
    }
    void loop() {
        double currentTime = AudioSettings.dspTime;

        List<string> playingBGMKeys = new List<string>(playingDic.Keys);
        foreach(string bgmId in playingBGMKeys) {
            if (bgmId.StartsWith(LOOP_PREFIX)) {
                string originalId = bgmId.Replace(LOOP_PREFIX, "");
                float nextLoopTime = loopData[originalId];
                // 1秒前からスケジュールしておく
                if (currentTime + 1f > nextLoopTime) {
                    AdvancedAudioClip clip = getBGM(originalId);
                    AudioSource loopSource = playingDic[bgmId];
                    // ループ用のが再生されている場合
                    if (loopSource.isPlaying) {
                        // オリジナルをスケジュール
                        AudioSource originalSource = playingDic[originalId];
                        originalSource.time = clip.LoopStart;
                        // 1秒後に再生
                        originalSource.PlayScheduled(nextLoopTime);
                        originalSource.SetScheduledEndTime(nextLoopTime + clip.LoopTime);
                    } else {
                        loopSource.time = clip.LoopStart;
                        // ループ用をスケジュール
                        loopSource.PlayScheduled(nextLoopTime);
                        loopSource.SetScheduledEndTime(nextLoopTime + clip.LoopTime);
                    }
                    // 次のループ時間を設定
                    loopData[originalId] += clip.LoopTime;
                }
            }
        }
    }

    /// <summary>
    /// SEの再生
    /// </summary>
    /// <param name="filename">再生したいSEのファイル名</param>
    /// <param name="volume">音量</param>
    public void playSE(string filename, int volume) {}
    // Use this for initialization
    void Start () {
        playingDic = new Dictionary<string, AudioSource>();
        AttachBGMSourceList = new List<AudioSource>();
        AttachSESourceList = new List<AudioSource>();
        loopData = new Dictionary<string, float>();
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
    }

    // Update is called once per frame
    void Update () {
        loop();
        fade();
    }
}
