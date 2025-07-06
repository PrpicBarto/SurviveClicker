using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource backgroundMusic;
    public AudioClip defeated;
    public AudioClip won;
    private void Awake()
    {
        instance = this;
    }
}
