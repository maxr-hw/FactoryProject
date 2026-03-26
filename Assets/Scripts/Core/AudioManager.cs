using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Factory.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private SoundLibrary library;
        
        private AudioSource musicSource;
        private AudioSource sfxSource;
        
        private int currentTrackIndex = -1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSources()
        {
            // Music Source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = false; // We handle looping between tracks manually
            musicSource.playOnAwake = false;

            // SFX Source
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            UpdateVolumes();
            if (scene.name == "MainMenu")
            {
                PlayMenuMusic();
            }
            else
            {
                // Only start the playlist if we aren't already playing it
                if (musicSource.clip != library.menuMusic && musicSource.isPlaying) return;
                
                PlayNextMusicTrack();
            }
        }

        private void Start()
        {
            UpdateVolumes();
        }

        public void PlayMenuMusic()
        {
            if (library == null || library.menuMusic == null) return;
            
            StopAllCoroutines(); // Stop any playlist logic
            musicSource.clip = library.menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        private void Update()
        {
            // If music finished, play next track
            if (musicSource != null && !musicSource.isPlaying && library != null && library.playlist.Count > 0)
            {
                PlayNextMusicTrack();
            }
        }

        public void UpdateVolumes()
        {
            if (SettingsManager.Instance == null) return;
            var s = SettingsManager.Instance.settings;

            // Apply volumes
            // musicSource.volume = s.masterVolume * s.musicVolume;
            // sfxSource.volume = s.masterVolume * s.sfxVolume;
            
            // Re-apply to the components
            if (musicSource) musicSource.volume = s.masterVolume * s.musicVolume;
            if (sfxSource) sfxSource.volume = s.masterVolume * s.sfxVolume;

            Debug.Log($"Audio Volumes Updated: Music={musicSource.volume}, SFX={sfxSource.volume}");
        }

        private void PlayNextMusicTrack()
        {
            if (library == null || library.playlist == null || library.playlist.Count == 0) return;

            currentTrackIndex = (currentTrackIndex + 1) % library.playlist.Count;
            musicSource.clip = library.playlist[currentTrackIndex];
            musicSource.Play();
            Debug.Log($"Now playing: {musicSource.clip.name}");
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip);
        }

        // Convenience methods using the library
        public void PlayClick() => PlaySFX(library?.clickSound);
        public void PlayPlace() => PlaySFX(library?.placeSound);
        public void PlayDelete() => PlaySFX(library?.deleteSound);
        public void PlayRotate() => PlaySFX(library?.rotateSound);
        public void PlayError() => PlaySFX(library?.errorSound);
        public void PlayOpenUI() => PlaySFX(library?.openUISound);
        public void PlayCloseUI() => PlaySFX(library?.closeUISound);
        public void PlayContractStarted() => PlaySFX(library?.contractStartedSound);
        public void PlayContractCompleted() => PlaySFX(library?.contractCompletedSound);
    }
}
