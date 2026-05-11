using UnityEngine;
using UnityEngine.Audio;

namespace CoreGameplay.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        private const string MUSIC_VOLUME_PARAM = "MusicVolume";
        private const string SFX_VOLUME_PARAM = "SFXVolume";

        [Header("Music")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip backgroundMusic;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip laneChangeClip;
        [SerializeField] private AudioClip collisionClip;
        [SerializeField] private AudioClip pickupActivatedClip;

        public static AudioManager Instance { get; private set; }

        private float musicVolume = 1f;
        private float sfxVolume = 1f;
        private bool musicMuted = false;
        private bool sfxMuted = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            LoadSettings();

            ApplyMusicVolume();
            ApplySFXVolume();

            if (backgroundMusic != null)
                PlayMusic(backgroundMusic);

            // Проверка параметров AudioMixer при старте
            ValidateMixerParameters();
        }

        #region Music Control

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();
        public void PauseMusic() => musicSource.Pause();
        public void ResumeMusic() => musicSource.UnPause();

        #endregion

        #region SFX Control

        public void PlaySFX(AudioClip clip, Vector3? position = null)
        {
            if (clip == null) return;

            if (position.HasValue)
                AudioSource.PlayClipAtPoint(clip, position.Value, sfxMuted ? 0f : sfxVolume);
            else
                PlaySFXOneShot(clip);
        }

        private void PlaySFXOneShot(AudioClip clip)
        {
            GameObject tempGO = new GameObject("TempSFX");
            tempGO.transform.position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = sfxMuted ? 0f : sfxVolume;
            tempSource.Play();
            Destroy(tempGO, clip.length);
        }

        public void PlayLaneChange() => PlaySFX(laneChangeClip);
        public void PlayCollision(Vector3 position) => PlaySFX(collisionClip, position);
        public void PlayPickupActivated() => PlaySFX(pickupActivatedClip);

        #endregion

        #region Volume & Mute Settings

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            ApplyMusicVolume();
            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplySFXVolume();
            SaveSettings();
        }

        public void ToggleMusicMute(bool mute)
        {
            musicMuted = mute;
            ApplyMusicVolume();
            SaveSettings();
        }

        public void ToggleSFXMute(bool mute)
        {
            sfxMuted = mute;
            ApplySFXVolume();
            SaveSettings();
        }

        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        public bool IsMusicMuted() => musicMuted;
        public bool IsSFXMuted() => sfxMuted;

        private void ApplyMusicVolume()
        {
            float db = musicMuted ? -80f : LinearToDecibel(musicVolume);

            // Устанавливаем параметр в AudioMixer (если он доступен)
            if (audioMixer != null)
            {
                if (!audioMixer.SetFloat(MUSIC_VOLUME_PARAM, db))
                {
                    Debug.LogWarning($"AudioMixer: не удалось установить параметр '{MUSIC_VOLUME_PARAM}'. " +
                                     "Проверьте, что он экспонирован в AudioMixer.");
                }
            }
            else
            {
                Debug.LogWarning("AudioMixer не назначен в AudioManager.");
            }

            // Громкость источника музыки теперь управляется ТОЛЬКО через Mixer.
            // Если AudioMixer отсутствует, используем прямое управление как запасной вариант.
            if (musicSource != null)
            {
                if (audioMixer != null)
                {
                    // Если AudioMixer назначен, громкость источника должна быть 1, чтобы миксер управлял ей.
                    musicSource.volume = 1f;
                }
                else
                {
                    // Запасной вариант без миксера
                    musicSource.volume = musicMuted ? 0f : musicVolume;
                }
            }
        }

        private void ApplySFXVolume()
        {
            float db = sfxMuted ? -80f : LinearToDecibel(sfxVolume);

            if (audioMixer != null)
            {
                if (!audioMixer.SetFloat(SFX_VOLUME_PARAM, db))
                {
                    Debug.LogWarning($"AudioMixer: не удалось установить параметр '{SFX_VOLUME_PARAM}'. " +
                                     "Проверьте, что он экспонирован в AudioMixer.");
                }
            }
            else
            {
                Debug.LogWarning("AudioMixer не назначен в AudioManager.");
            }
        }

        private float LinearToDecibel(float linear)
        {
            return linear > 0.0001f ? 20f * Mathf.Log10(linear) : -80f;
        }

        private void ValidateMixerParameters()
        {
            if (audioMixer == null) return;

            float dummy;
            if (!audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out dummy))
            {
                Debug.LogError($"Параметр '{MUSIC_VOLUME_PARAM}' не найден в AudioMixer! " +
                               "Откройте окно Audio Mixer, выберите нужную группу, кликните правой кнопкой по " +
                               "названию параметра в инспекторе (верхняя панель) и добавьте exposed параметр с таким именем.");
            }
            if (!audioMixer.GetFloat(SFX_VOLUME_PARAM, out dummy))
            {
                Debug.LogError($"Параметр '{SFX_VOLUME_PARAM}' не найден в AudioMixer! " +
                               "Добавьте его аналогично MusicVolume.");
            }
        }

        #endregion

        #region Persistence

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetInt("MusicMuted", musicMuted ? 1 : 0);
            PlayerPrefs.SetInt("SFXMuted", sfxMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
            sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        }

        #endregion
    }
}