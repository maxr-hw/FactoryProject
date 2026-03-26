using UnityEngine;
using System.Collections.Generic;

namespace Factory.Core
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Factory/SoundLibrary")]
    public class SoundLibrary : ScriptableObject
    {
        [Header("Music")]
        public AudioClip menuMusic;
        public List<AudioClip> playlist;

        [Header("SFX - Building")]
        public AudioClip placeSound;
        public AudioClip deleteSound;
        public AudioClip rotateSound;
        public AudioClip errorSound;

        [Header("SFX - UI")]
        public AudioClip clickSound;
        public AudioClip openUISound;
        public AudioClip closeUISound;
        
        [Header("SFX - Contracts")]
        public AudioClip contractStartedSound;
        public AudioClip contractCompletedSound;
        
        [Header("SFX - Machines")]
        public AudioClip itemDeliveredSound;
    }
}
