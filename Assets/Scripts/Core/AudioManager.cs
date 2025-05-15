using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("Music Clips")]
    public AudioClip menuTheme;
    public AudioClip battleTheme;

    [Header("UI Sound Effects")]
    public AudioClip buttonClick;
    public AudioClip cardPlaced;

    [Header("Unit Sound Effects")]
    // Player unit sounds
    public AudioClip archerAttack;
    public AudioClip swordHitArmor;
    public AudioClip swordHitFlesh;
    public AudioClip swordHitMetal;
    public AudioClip abilitySpell;
    public AudioClip fireballCast;

    // Monster sounds
    public AudioClip monsterGrowl;

    [Header("Game Event Sounds")]
    public AudioClip playerLost;
    public AudioClip playerWon;
    public AudioClip humanGrowl;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float uiVolume = 0.7f;

    private void Start()
    {
        // Configure audio sources
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (uiSource == null)
            uiSource = gameObject.AddComponent<AudioSource>();

        // Set initial volumes
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        uiSource.volume = uiVolume;

        // Configure music source
        musicSource.loop = true;

        // Start with menu music if in setup state
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Setup)
            PlayMusic(menuTheme);
    }

    public void PlayMusic(AudioClip music)
    {
        if (music == null) return;

        // Only change if different music
        if (musicSource.clip != music)
        {
            musicSource.clip = music;
            musicSource.Play();
        }
    }

    public void PlayGameStateMusic(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Setup:
                PlayMusic(menuTheme);
                break;
            case GameManager.GameState.PlayerTurn:
            case GameManager.GameState.MonsterTurn:
                if (musicSource.clip != battleTheme)
                    PlayMusic(battleTheme);
                break;
            case GameManager.GameState.Victory:
                PlaySFX(playerWon);
                // Optionally switch music or keep battle music
                break;
            case GameManager.GameState.Defeat:
                PlaySFX(playerLost);
                // Optionally switch music or keep battle music
                break;
        }
    }

    public void PlaySFX(AudioClip sfx)
    {
        if (sfx == null) return;
        sfxSource.PlayOneShot(sfx, sfxVolume);
    }

    public void PlayUISound(AudioClip sound)
    {
        if (sound == null) return;
        uiSource.PlayOneShot(sound, uiVolume);
    }

    // Player unit attack sounds
    public void PlayArcherAttackSound()
    {
        PlaySFX(archerAttack);
    }

    public void PlayWarriorAttackSound()
    {
        // Chain sword hit sounds with slight delay
        StartCoroutine(PlaySwordHitSequence());
    }

    private IEnumerator PlaySwordHitSequence()
    {
        PlaySFX(swordHitArmor);
        yield return new WaitForSeconds(0.2f);
        PlaySFX(swordHitFlesh);
        yield return new WaitForSeconds(0.2f);
        PlaySFX(swordHitMetal);
    }

    public void PlayMageAbilitySound()
    {
        PlaySFX(fireballCast);
    }

    public void PlayGenericAbilitySound()
    {
        PlaySFX(abilitySpell);
    }

    // Monster sounds
    public void PlayMonsterAttackSound()
    {
        PlaySFX(monsterGrowl);
    }

    // Game event sounds
    public void PlayVictorySound()
    {
        PlaySFX(playerWon);
    }

    public void PlayDefeatSound()
    {
        PlaySFX(playerLost);
    }

    public void PlayCardPlacedSound()
    {
        PlayUISound(cardPlaced);
    }

    public void PlayButtonClickSound()
    {
        PlayUISound(buttonClick);
    }

    public void PlayHumanSound()
    {
        PlaySFX(humanGrowl);
    }

    // Volume controls
    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = volume;
        uiSource.volume = volume;
    }

    // Playback controls
    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    // Method to play appropriate unit attack sound based on unit type
    public void PlayUnitAttackSound(string unitType)
    {
        switch (unitType.ToLower())
        {
            case "archer":
                PlaySFX(archerAttack);
                break;
            case "warrior":
            case "knight":
                StartCoroutine(PlaySwordHitSequence());
                break;
            case "mage":
                PlaySFX(fireballCast);
                break;
            default:
                // Generic sword hit as fallback
                PlaySFX(swordHitFlesh);
                break;
        }
    }

    // Method to play appropriate unit ability sound based on unit type
    public void PlayUnitAbilitySound(string unitType)
    {
        switch (unitType.ToLower())
        {
            case "mage":
                PlaySFX(fireballCast);
                break;
            case "archer":
                PlaySFX(archerAttack);
                break;
            default:
                PlaySFX(abilitySpell);
                break;
        }
    }
}