using UnityEngine;

public class GirlSounds : MonoBehaviour
{
    public AudioSource breathSound;
    public AudioSource screamSound;
    public AudioSource otherSound; // for any other sounds

    public void PlayDeepBreath()
    {
        breathSound.Play();
    }

    public void PlayMultipleBreath()
    {
        screamSound.Play();
    }

    public void PlayScream()
    {
        screamSound.Play();
    }
}
