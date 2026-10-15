using UnityEngine;

/// <summary>
/// Simple Sound Manager (Singleton)
/// 
/// SETUP INSTRUCTIONS:
/// 1. Make the SoundManager a CHILD of your Player GameObject (so it follows the player).
/// 2. Add TWO AudioSource components to this GameObject manually in the Inspector:
///    - First AudioSource  → used for SFX (jump, coin collect, etc.)
///    - Second AudioSource → used for Background Music (set Loop = true).
/// 3. Drag this script onto the same GameObject.
/// 4. In the Inspector, assign the two AudioSource references and your AudioClips.
/// 5. Any script can now call SoundManager.Instance.PlayJumpSound() or PlayCoinCollectSound().
/// </summary>
public class SoundManager : MonoBehaviour
{
    // ==================== SINGLETON ====================

    // Static reference so any script can access this manager easily
    public static SoundManager Instance;

    // ==================== AUDIO SOURCES (Assign in Inspector) ====================

    [Header("Audio Sources (Add manually in Inspector)")]
    [Tooltip("AudioSource used for playing sound effects (jump, coin, etc.)")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("AudioSource used for playing background music (set Loop = true on this)")]
    [SerializeField] private AudioSource musicSource;

    // ==================== SOUND CLIPS ====================

    [Header("Sound Clips")]
    [Tooltip("Sound effect played when the player jumps")]
    [SerializeField] private AudioClip jumpSound;

    [Tooltip("Sound effect played when a coin is collected")]
    [SerializeField] private AudioClip coinCollectSound;

    [Tooltip("Background music clip")]
    [SerializeField] private AudioClip backgroundMusic;

    // ==================== VOLUME SETTINGS ====================

    [Header("Volume")]
    [Tooltip("Volume for the jump sound")]
    [SerializeField] [Range(0f, 1f)] private float jumpVolume = 0.8f;

    [Tooltip("Volume for the coin collect sound")]
    [SerializeField] [Range(0f, 1f)] private float coinCollectVolume = 0.8f;

    [Tooltip("Volume for the background music")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

    private void Awake()
    {
        // Singleton pattern — only one SoundManager should exist
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Start playing background music if a clip is assigned
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Play the jump sound effect. Called from PlayerController when the player jumps.
    /// </summary>
    public void PlayJumpSound()
    {
        if (sfxSource != null && jumpSound != null)
        {
            sfxSource.PlayOneShot(jumpSound, jumpVolume);
        }
    }

    /// <summary>
    /// Play the coin collect sound effect. Called from Coin script when a coin is picked up.
    /// </summary>
    public void PlayCoinCollectSound()
    {
        if (sfxSource != null && coinCollectSound != null)
        {
            sfxSource.PlayOneShot(coinCollectSound, coinCollectVolume);
        }
    }
}
