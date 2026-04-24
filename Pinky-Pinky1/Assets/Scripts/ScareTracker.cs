using UnityEngine;

public class ScareTracker : MonoBehaviour
{
    private string lastScareType = "";

    public int RegisterScare(string currentScareType, int repeatPenalty)
    {
        int bonusSuspicion = 0;

        if (lastScareType == currentScareType)
        {
            bonusSuspicion = repeatPenalty;
            Debug.Log("Repeated scare used back to back: +" + repeatPenalty + " suspicion");
        }

        lastScareType = currentScareType;
        return bonusSuspicion;
    }
}
