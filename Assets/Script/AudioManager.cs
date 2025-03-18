using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("AudioSource")]
    [SerializeField] AudioSource SFX;

    [Header("AudioClip")]
    public AudioClip shootSFX;
    public AudioClip reloadSFX;
    public void PlaySFX(AudioClip clip)
    {
        SFX.PlayOneShot(clip);
    }
}
