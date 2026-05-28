using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource soundEffectSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip buttonPressedSoundEffect;

    private void Start()
    {
        PlayBackgroundMusic();
    }

    private void PlayBackgroundMusic()
    {
        if (musicSource && backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (soundEffectSource && clip)
        {
            soundEffectSource.PlayOneShot(clip);
        }
    }

}
