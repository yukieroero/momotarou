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
    /// <summary>
    /// AdvancedAudioClipのコンストラクタ
    /// </summary>
    /// <param name="clip">AudioClip</param>
    public AdvancedAudioClip(AudioClip clip) {
        instance = clip;
        // デフォルトでは曲頭から曲末までループする
        this.SetLoop(0f, clip.length);
    }
    /// <summary>
    /// headに書かれた情報を元にループの設定をする
    /// </summary>
    /// <param name="loopStart">ループの開始地点</param>
    /// <param name="loopEnd">ループの終了地点</param>
    public void SetLoop(float loopStart, float loopEnd) {
        this.loopStart = loopStart / 1000f;
        if (loopEnd == 0) loopEnd = this.instance.length * 1000;
        this.loopEnd = loopEnd / 1000f;
        this.loopTime = this.loopEnd - this.loopStart;
    }
    /// <summary>
    /// fadein fadeoutの設定
    /// </summary>
    /// <param name="volume">最終的なボリューム値 0 ~ 1 </param>
    /// <param name="duration">フェードにかかる時間 ms</param>
    /// <param name="currentVolume">現在のボリューム</param>
    public void SetFadeMeta(float volume, float duration, float currentVolume) {
        // volume まで durationずつ音量を変化させたい
        this.destination = volume;
        this.duration = duration;
        // 毎秒変化させたい音量 = 変化させる音量 / 何秒で変化させるか
        this.gainPerSecond = (volume - currentVolume) / ((float)this.duration / 1000f);
        this.distance = volume - currentVolume;
    }

    /// <summary>
    /// AudioClip 本体
    /// </summary>
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
    // ループ再生用のprefix
    private static string LOOP_PREFIX = "LOOP___";
    // BGM再生用のAudioSourceのリストとSE再生用のAudioSource
    private List<AudioSource> AttachSourceList;
    // 全データ
    private Dictionary<string, AdvancedAudioClip> AudioDic;
    // 現在再生しているデータ
    private Dictionary<string, AudioSource> playingDic;
    // ループ管理用 {bgmId: nextLoopTime}
    private Dictionary<string, float> loopData;
    private Animation anim;
    /// <summary>
    /// idをkeyにAdvancedAudioClipをゲットする
    /// </summary>
    /// <param name="id">取得したいAdvancedAudioClipのID = Resources/Audio以下のパス</param>
    /// <returns></returns>
    public AdvancedAudioClip getClip(string id) {
        return AudioDic[id];
    }
    AudioSource GetSource(string id, bool duplicate = false) {
        AudioSource source = null;
        // seの場合はおんなじ音源を同時に鳴らす可能性はあるので
        // bgmの場合のみ同一音源が再生中の場合は再生しているAudioSourceを取得
        // そうでない場合は、再生してないAudioSourceを取得

        // 重複を許可しない場合に限り再生中の音源がある場合はそれを取得
        if (duplicate == false) playingDic.TryGetValue(id, out source);
        if (source == null) {
            // アタッチされているAudioSourceで使用されていないのがあればそれを使い回す
            foreach(AudioSource s in AttachSourceList) {
                // ループ用音源の設定は、再生前のオリジナルの{s}が存在するので注意
                if (s.isPlaying == false && s.clip != getClip(id.Replace(LOOP_PREFIX, "")).Instance) {
                    source = s;
                    if (duplicate == false) {
                        // 別のキーで同一sourceを保持してる場合があるので,そのキーを削除
                        foreach(string key in playingDic.Keys) {
                            if (playingDic[key] == source) {
                                playingDic.Remove(key);
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }
        // 空いてるAudioSourceがなければ追加する
        if (source == null) {
            source = this.gameObject.AddComponent<AudioSource>();
            AttachSourceList.Add(source);
        }
        // 重複を許可しない場合は重複チェックをしたいので管理用のdicに登録
        if(duplicate == false && playingDic.ContainsKey(id) == false) playingDic[id] = source;
        // ボリュームは0にしておく
        if (source.isPlaying == false) source.volume = 0;
        source.playOnAwake = false;
        return source;
    }
    void OnFadeUpdate(System.Action callback) {
        callback();
    }
    void OnFadeEnd(System.Action callback) {
        callback();
    }
    void syncVolume(string bgmId, string loopId) {
        AudioSource loopSource = null;
        // ループする時のみオリジナルのSourceと音量を合わせる
        if (playingDic.TryGetValue(loopId, out loopSource)) {
            AudioSource originalSource = playingDic[bgmId];
            loopSource.volume = originalSource.volume;
        };
    }
    /// <summary>
    /// BGMフェードインフェードアウトの実装
    /// <param name="bgmId">対象ファイル名</param>
    /// <param name="volume">音量 0だとフェードアウトになる</param>
    /// <param name="duration">フェードインフェードアウトにかかる時間 0だと即時再生、停止</param>
    /// <param name="loop">ループの可否</param>
    /// </summary>
    public void fadeBGM(string bgmId, float duration, float volume, bool loop=false) {
        AudioSource source = GetSource(bgmId);
        AdvancedAudioClip clip = this.getClip(bgmId);
        if (source.clip != clip.Instance) source.clip = clip.Instance;

        string loopId = LOOP_PREFIX + bgmId;
        if (loop) prepareLoop(bgmId);

        Hashtable hash = new Hashtable(){
            {"audiosource", source},
            {"volume", volume},
            {"pitch", 1f},
            {"time", duration / 1000f},
            // 毎回の変化後にジッコスウル
            {"onupdatetarget", gameObject},
            {"onupdate", "OnFadeUpdate"},
            {"onupdateparams", new System.Action(() => {
                syncVolume(bgmId, loopId);
            })},
            // 変化が終了したらthis.gameObject.OnFadeEnd(action)を実行する
            {"oncompletetarget", gameObject},
            {"oncomplete", "OnFadeEnd"},
            {"oncompleteparams", new System.Action(() => {
                syncVolume(bgmId, loopId);
                if (loop == false) {
                    Debug.LogFormat("Playing End: {0} {1}", bgmId, source.clip.name);
                    source.Stop();
                    playingDic.Remove(bgmId);
                    /* ここからループ用AudioSourceの終了イベント */
                    if (playingDic.TryGetValue(loopId, out source)) {
                        source.Stop();
                        playingDic.Remove(loopId);
                    }
                    if (loopData.ContainsKey(bgmId)) loopData.Remove(bgmId);
                    /* ここまでループ用AudioSource終了イベント */
                    iTween.Stop(gameObject);
                }
            })},
        };
        iTween.AudioTo(gameObject, hash);
    }
    /// <summary>
    /// ループ再生の準備を行う
    /// 再生の1秒前から準備を開始しておく
    /// </summary>
    /// <param name="bgmId">再生するファイルID</param>
    void prepareLoop(string bgmId) {
        AudioSource loopSource = GetSource(LOOP_PREFIX + bgmId);
        AdvancedAudioClip clip = getClip(bgmId);
        loopSource.clip = clip.Instance;
        // スケジュールがずれないようにすでにスケジュールされている場合は変更しないようにする
        if (loopData.ContainsKey(bgmId) == false) {
            // オリジナルを止める時間を設定
            AudioSource originalSource = GetSource(bgmId);
            float currentTime = (float)AudioSettings.dspTime + 1f;
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
                    AdvancedAudioClip clip = getClip(originalId);
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
    /// <param name="seId">再生したいSEのID</param>
    public void playSE(string seId, float volume) {
        // SEは同時に同じ音源を再生可能
        AudioSource source = GetSource(seId, true);
        AdvancedAudioClip clip = this.getClip(seId);
        if (source.clip != clip.Instance) source.clip = clip.Instance;
        if (source.isPlaying == false) {
            source.volume = volume;
            source.Play();
            this.OnPlayEnd(source, () => {
                Debug.Log("このっはげーーー");
                playingDic.Remove(seId);
                AttachSourceList.Remove(source);
                Destroy(source);
            });
        }
    }

    void OnPlayEnd(AudioSource source, System.Action callback) {
        StartCoroutine(OnPlayEndHandler(source, callback));
    }
    private IEnumerator OnPlayEndHandler (AudioSource source, System.Action callback) {
        while (true) {
            yield return new WaitForFixedUpdate();
            if (source.isPlaying == false) {
                callback();
                break;
            }
        }
    }
    // Use this for initialization
    void Start () {
        playingDic = new Dictionary<string, AudioSource>();
        AttachSourceList = new List<AudioSource>();
        loopData = new Dictionary<string, float>();
        // リソース読み込み
        AudioDic = new Dictionary<string, AdvancedAudioClip>();
        object[] bgmList = Resources.LoadAll("Audio/BGM", typeof(AudioClip));
        object[] seList = Resources.LoadAll("Audio/SE", typeof(AudioClip));

        foreach (AudioClip bgm in bgmList) {
            AudioDic[string.Format("BGM/{0}", bgm.name)] = new AdvancedAudioClip(bgm);
        }
        foreach (AudioClip se in seList) {
            AdvancedAudioClip clip = new AdvancedAudioClip(se);
            AudioDic[string.Format("SE/{0}", se.name)] = new AdvancedAudioClip(se);
        }

        // fadeのためにAnimatorを用意する
        anim = gameObject.AddComponent<Animation>();
        // シーンをまたいでも削除されないようにする
        DontDestroyOnLoad (this.gameObject);
    }

    // Update is called once per frame
    void Update () {
        loop();
    }
}
