using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null) instance = new();
            return instance;
        }
    }

    //public enum SoundTypes
    //{
    //    BGM,
    //    SFX,
    //}

    public enum BgmTypes
    {
        TITLE,
        GAME,
    }

    public enum SfxTypes
    {
        BUTTON,
        MOVE,
        CLEAR,
        LOSE,
    }

    //[SerializeField] AudioMixer mixer;
    [SerializeField] AudioClip[] bgms;
    [SerializeField] AudioClip[] sfxs;

    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource sfxSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void PlayBGM(BgmTypes type)
    {
        bgmSource.clip = bgms[(int)type];
        bgmSource.Play();
    }

    public void PlaySFX(SfxTypes type)
    {
        sfxSource.PlayOneShot(sfxs[(int)type]);
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    //public void SetVolume(SoundTypes type, float value)
    //{
    //    mixer.SetFloat(type.ToString(), value);
    //}
}
