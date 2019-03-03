using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1.00
/// </summary>
namespace Binity {

    public class AudioManager : MonoBehaviour {
        public static AudioManager instance;
        public enum AudioChannel { Master, Sfx, Music };

        public float masterVolumePercent { get; private set; }
        public float sfxVolumePercent { get; private set; }
        public float musicVolumePercent { get; private set; }

        private AudioSource sfx2DSource;
        private Transform audioListener;
        private Dictionary<string, AudioClip[]> groupDictionary = new Dictionary<string, AudioClip[]>();
        private Dictionary<string, float> audioClipTimer;

        /// <summary>
        /// 싱글턴
        /// </summary>
        private void Awake() {
            if (instance != null) {
                Destroy(gameObject);
            }
            else {
                instance = this;
                DontDestroyOnLoad(gameObject);

                // AudioListener를 가져온다.
                audioListener = FindObjectOfType<AudioListener>().transform;

                //2D 소스는 AudioListener와 같은 곳에
                GameObject newSfx2Dsource = new GameObject("2D sfx source");
                sfx2DSource = newSfx2Dsource.AddComponent<AudioSource>();
                newSfx2Dsource.transform.parent = audioListener;

                masterVolumePercent = PlayerPrefs.GetFloat("master vol", 1);
                sfxVolumePercent = PlayerPrefs.GetFloat("sfx vol", 1);
                musicVolumePercent = PlayerPrefs.GetFloat("music vol", 1);
            }
        }

        /// <summary>
        /// 여기서 오디오 클립을 로드한다.
        /// </summary>
        private void Start() {
            LoadResources("Zombie", "Voice/Zombie");
            LoadResources("Zombie@Hit", "Voice/Zombie@Hit");
            LoadResources("Splat", "Voice/Splat");
            LoadResources("Male@Choose", "Voice/Male@Choose");
            LoadResources("Male@Move", "Voice/Male@Move");
            LoadResources("Male@Engage", "Voice/Male@Engage");
        }

        public void SetVolume(float volumePercent, AudioChannel channel) {
            switch (channel) {
                case AudioChannel.Master:
                    masterVolumePercent = volumePercent;
                    break;

                case AudioChannel.Sfx:
                    sfxVolumePercent = volumePercent;
                    break;

                case AudioChannel.Music:
                    musicVolumePercent = volumePercent;
                    break;
            }

            PlayerPrefs.SetFloat("master vol", masterVolumePercent);
            PlayerPrefs.SetFloat("sfx vol", sfxVolumePercent);
            PlayerPrefs.SetFloat("music vol", musicVolumePercent);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 실질적인 재생 메서드
        /// </summary>
        public void PlaySound(string soundName, Vector3 pos, float volume = 1f, bool playWhenPriviousClipEnds = false) {
            AudioClip clip = GetClipFromName(soundName);
            if (clip == null) return;

            pos += Vector3.up * GameObject.Find("CameraWrapper").transform.position.y * .9f;

            // 전 재생이 끝난 이후에 다시 재생
            if (playWhenPriviousClipEnds) {
                // 재생한 적이 없다면
                if (!audioClipTimer.ContainsKey(soundName)) {
                    audioClipTimer.Remove(soundName);
                    audioClipTimer.Add(soundName, Time.time + clip.length);
                    AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent * volume);
                }
                // 재생한 적이 있다면
                else if (audioClipTimer[soundName] < Time.time) {
                    audioClipTimer.Remove(soundName);
                    audioClipTimer.Add(soundName, Time.time + clip.length);
                    AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent * volume);
                }
            }
            else {
                AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent * volume);
            }
        }

        public void PlaySound2D(string soundName, float volume = 1f, bool playWhenPriviousClipEnds = false) {
            AudioClip clip = GetClipFromName(soundName);
            if (clip == null) return;

            // 전 재생이 끝난 이후에 다시 재생
            if (playWhenPriviousClipEnds) {
                if (!audioClipTimer.ContainsKey(soundName)) {
                    audioClipTimer.Remove(soundName);
                    audioClipTimer.Add(soundName, Time.time + clip.length);
                    sfx2DSource.PlayOneShot(clip, sfxVolumePercent * masterVolumePercent);
                }
                // 재생한 적이 있다면
                else if (audioClipTimer[soundName] < Time.time) {
                    audioClipTimer.Remove(soundName);
                    audioClipTimer.Add(soundName, Time.time + clip.length);
                    sfx2DSource.PlayOneShot(clip, sfxVolumePercent * masterVolumePercent);
                }
            }
            // 그냥 재생
            else {
                sfx2DSource.PlayOneShot(clip, sfxVolumePercent * masterVolumePercent);
            }
        }

        public void LoadResources(string clipName, string path) {
            AudioClip[] sounds;
            sounds = Resources.LoadAll<AudioClip>(path);
            if (sounds.Length > 0) {
                groupDictionary.Add(clipName, sounds);
            }
            else {
                Debug.Log("Error - LoadResources : " + path + " is not exist");
            }
        }

        /// <summary>
        /// 이름으로 오디오 클립 가져오기.
        /// </summary>
        public AudioClip GetClipFromName(string name) {
            if (groupDictionary.ContainsKey(name)) {
                AudioClip[] sounds = groupDictionary[name];
                return sounds[Random.Range(0, sounds.Length)];
            }
            return null;
        }

        public class SoundGroup {
            public string groupName;
            public AudioClip[] audioClips;
        }
    }
}