using UnityEngine;
using SurgeAudio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Related")]
    public Base baseAudio;
    public Ranges targetRanges;

    private void Start()
    {
        instance = this;

    }
}