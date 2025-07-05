using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource backgroundMusic;
    public AudioClip defeated;
    private void Awake()
    {
        instance = this;
    }
}
