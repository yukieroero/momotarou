using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audio {
    class Player {

        private AudioClip track;
        internal bool playing;
        internal bool loop;
        // main source and sub source for loop
        AudioSource[] audioSources;
        int sourceIndex;
        float volume;
        // ループ再生の始まりの位置
        float loopStart;
        // ループ再生の最後の位置
        float loopEnd;
        // ループ再生時間
        float loopTime;
        // 次のループの開始タイミング
        float loopTiming;
        bool firstLoop;

        public float Volume
        {
            get
            {
                return volume;
            }

            set
            {
                volume = value;
                AudioSources[0].volume = volume;
                AudioSources[1].volume = volume;
            }
        }
        public float LoopTiming
        {
            get
            {
                return loopTiming;
            }

            set
            {
                loopTiming = value;
            }
        }

        public AudioSource[] AudioSources
        {
            get
            {
                return audioSources;
            }

            set
            {
                audioSources = value;
            }
        }

        internal AudioClip Track
        {
            get
            {
                return track;
            }

            set
            {
                track = value;
                AudioSources[0].clip = track;
                AudioSources[1].clip = track;
            }
        }

        public void setLoop(bool loop, float loopStart, float loopEnd) {
            this.loop = loop;
            audioSources[0].time = loopStart;
            this.loopStart = loopStart;
            this.loopEnd = loopEnd;
            this.loopTime = loopEnd - loopStart;
        }

        public Player () {
            // 基本再生されるのはindex=0のaudioSource
            sourceIndex = 0;
            AudioSources = new AudioSource[2];
            firstLoop = true;
        }
        internal void play() {
            playing = true;
            // 通常再生 loopの場合はloopPlay
            if (loop == false) AudioSources[sourceIndex].Play();
        }
        internal void stop() {
            playing = false;
            AudioSources[sourceIndex].Stop();
            AudioSources[1-sourceIndex].Stop();
        }
        internal void loopPlay () {
            // 再生の1秒前から準備を開始しておく
            double currentTime = AudioSettings.dspTime;
            if (currentTime + 1f > this.loopTiming) {
                // 再生対象のaudiosouceを切り替える
                sourceIndex = 1 - sourceIndex;
                if (!firstLoop) {
                    // 再生開始位置を設定
                    this.AudioSources[sourceIndex].time = this.loopStart;
                    firstLoop = false;
                }
                // 再生をスケジュールする
                this.AudioSources[sourceIndex].PlayScheduled(loopTiming);
                // 次のループは何秒後か
                loopTiming += loopTime;
                // 初回ループの場合、loopTimingをずらす
                if (firstLoop) {
                    loopTiming += loopEnd;
                }
                // 停止をスケジュールする
                this.AudioSources[sourceIndex].SetScheduledEndTime(loopTiming);
            }
        }
    }
    public class AudioController : MonoBehaviour {

        Player player;
        bool fadingPlay;
        int fadeDuration;
        bool fadingStop;
        float targetVolume;
        float volumePerFrame;

        void setTrack(AudioClip track=null) {
            this.player.Track = track;
        }
        public void setLoop(bool loop, float loopStart, float loopEnd) {
            // ms => s
            loopStart = loopStart / 1000f;
            loopEnd = loopEnd / 1000f;
            this.player.setLoop(loop, loopStart, loopEnd);
        }
        float calcVPF() {
            // 毎秒変化させたい音量 = 目的の音量 / 何秒で変化させるか
            float volumePerSecond = targetVolume / ((float)fadeDuration / 1000f);
            // 毎フレーム変化させる音量 = 毎秒変化させたい音量 * (1f/秒) <= 1fあたりの秒数
            volumePerFrame = volumePerSecond * Time.deltaTime;
            return volumePerFrame;
        }
        public void setFadeIn(int fadeDuration, float fadeInVolume) {
            fadingPlay = true;
            this.fadeDuration = fadeDuration;
            // fade inする場合は最初は音量0
            this.player.Volume = 0;
            this.targetVolume = fadeInVolume;
            this.volumePerFrame = calcVPF();
        }
        void fadeIn() {
            float currentVolume = this.player.Volume;
            if (currentVolume < targetVolume) {
                this.player.Volume += volumePerFrame;
            } else {
                // 終了
                fadingPlay = false;
            }
        }
        public void setFadeOut(int fadeDuration) {
            fadingStop = true;
            this.fadeDuration = fadeDuration;
            // fade outする場合は音量0にする
            this.targetVolume = 0;
            this.volumePerFrame = calcVPF();
        }
        void fadeOut() {
            float currentVolume = this.player.Volume;
            if (currentVolume > targetVolume) {
                this.player.Volume -= volumePerFrame;
            } else {
                // 終了
                fadingStop = false;
                this.setTrack();
                this.player.stop();
            }
        }
        public void play(AudioClip track=null) {
            if (track) this.setTrack(track);
            this.player.play();
        }
        public void stop() {
            this.player.stop();
        }
        // Use this for initialization
        void Awake () {
            player = new Player();
            this.player.AudioSources[0] = gameObject.AddComponent<AudioSource>();
            this.player.AudioSources[1] = gameObject.AddComponent<AudioSource>();
            fadingPlay = false;
            fadingStop = false;
            // 1秒後に再生を開始するように設定
            player.LoopTiming = (float)AudioSettings.dspTime + 1f;
        }
        void Start () {
        }

        // Update is called once per frame
        void Update () {
            if (this.player.playing == true && this.player.loop) {
                this.player.loopPlay();
            }
            if (this.player.playing == true && fadingPlay) fadeIn();
            if (this.player.playing == false && fadingStop) fadeOut();
        }
    }
}
