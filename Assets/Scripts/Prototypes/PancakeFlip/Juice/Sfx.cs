using System.Collections.Generic;
using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class Sfx : MonoBehaviour
    {
        public static Sfx Instance { get; private set; }

        [SerializeField] AudioSource source;
        [SerializeField] AudioSource musicSource;
        [SerializeField] AudioClip flip;
        [SerializeField] AudioClip serve;
        [SerializeField] AudioClip cook;
        [SerializeField] AudioClip levelUp;
        [SerializeField] AudioClip unlock;
        [SerializeField] AudioClip click;
        [SerializeField] AudioClip denied;
        [SerializeField] AudioClip buy;
        [SerializeField] AudioClip coin;
        [SerializeField] AudioClip car;
        [SerializeField] AudioClip[] cityMusic;

        int _currentMusic = -1;
        readonly Dictionary<int, AudioClip> _blips = new Dictionary<int, AudioClip>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            if (source == null) source = GetComponent<AudioSource>();
            if (source != null) source.playOnAwake = false;
            if (musicSource != null) { musicSource.playOnAwake = false; musicSource.loop = true; }
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        public void PlayFlip(float pitch) => Play(flip, pitch, 520f);
        public void PlayServe() => Play(serve, 1f, 680f);
        public void PlayCook() => Play(cook, 1f, 300f);
        public void PlayLevelUp() => Play(levelUp, 1f, 880f);
        public void PlayUnlock() => Play(unlock, 1f, 760f);
        public void Click() => Play(click, 1f, 440f);
        public void PlayDenied() => Play(denied, 1f, 180f);
        // Без блип-фолбэка: пока нет файла — молчим.
        public void PlayBuy() { if (buy != null) Play(buy, 1f, 0f); }
        public void PlayCoin() { if (coin != null) Play(coin, 1f, 0f); }
        public void PlayCar() { if (car != null) Play(car, 1f, 0f); }

        public void PlayCityMusic(int index)
        {
            if (musicSource == null || cityMusic == null) return;
            if (index < 0 || index >= cityMusic.Length) return;
            if (index == _currentMusic && musicSource.isPlaying) return;
            var clip = cityMusic[index];
            if (clip == null) return;
            _currentMusic = index;
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        void Play(AudioClip clip, float pitch, float blipHz)
        {
            if (source == null) return;
            source.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
            var c = clip != null ? clip : GetBlip(blipHz);
            if (c != null) source.PlayOneShot(c);
        }

        AudioClip GetBlip(float hz)
        {
            int key = Mathf.RoundToInt(hz);
            if (_blips.TryGetValue(key, out var existing)) return existing;
            const int sampleRate = 44100;
            const float dur = 0.09f;
            int count = (int)(sampleRate * dur);
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float tt = (float)i / sampleRate;
                float envelope = 1f - (float)i / count; // linear decay
                data[i] = Mathf.Sin(2f * Mathf.PI * hz * tt) * 0.35f * envelope;
            }
            var clip = AudioClip.Create($"blip{key}", count, 1, sampleRate, false);
            clip.SetData(data, 0);
            _blips[key] = clip;
            return clip;
        }
    }
}
