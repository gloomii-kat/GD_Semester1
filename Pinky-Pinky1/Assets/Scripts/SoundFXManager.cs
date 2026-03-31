using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
    }


}
