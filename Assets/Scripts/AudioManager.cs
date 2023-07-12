using FMODUnity;
using UnityEngine;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private void Awake()
    {
        instance ??= this;
    }
    public void PlaySound(string eventRef)
    {
        RuntimeManager.PlayOneShot(eventRef);
    }
}
