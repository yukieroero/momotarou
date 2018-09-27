using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audio {
    class Player {

        private AudioClip track;
        internal bool playing;
        AudioSource audioSource;
        float volume;

        public float Volume
        {
            get
            {
                return volume;
            }

            set
            {
                volume = value;
                AudioSource.volume = volume;
            }
        }

        public AudioSource AudioSource
        {
            get
            {
                return audioSource;
            }

            set
            {
                audioSource = value;
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
                audioSource.clip = track;
            }
        }

        public Player () {
        }
        internal void play() {
            playing = true;
            audioSource.Play();
        }
    }
    public class AudioController : MonoBehaviour {

        Player player;
        public AudioSource audioSource;
        bool fadingPlay;
        int fadeInTime;
        bool fadingStop;
        float targetVolume;
        float volumePerFrame;

        void setTrack(AudioClip track) {
            this.player.Track = track;
        }
        public void setFadeIn(int fadeInTime, float fadeInVolume) {
            fadingPlay = true;
            this.fadeInTime = fadeInTime;
            // fade inする場合は最初は音量0
            this.player.Volume = 0;
            this.targetVolume = fadeInVolume;
            // fadeInTimeはミリ秒なので秒に変換
            // 毎秒増加させたい音量 = 目的の音量 / 何秒であげるか
            float volumePerSecond = targetVolume / ((float)fadeInTime / 1000f);
            // 毎フレーム増加する音量 = 毎秒増加させたい音量 * (1f/秒) <= 1fあたりの秒数
            volumePerFrame = volumePerSecond * Time.deltaTime;
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
        public void play(AudioClip track=null) {
            if (track) this.setTrack(track);
            this.player.play();
        }
        // Use this for initialization
        void Awake () {
            player = new Player();
            fadingPlay = false;
            fadingStop = false;
        }
        void Start () {
            audioSource = gameObject.AddComponent<AudioSource>();
            this.player.AudioSource = audioSource;
        }

        // Update is called once per frame
        void Update () {
            if (fadingPlay) {
                fadeIn();
            }
        }
    }
}
