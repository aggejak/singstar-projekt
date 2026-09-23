using UnityEngine;

public class FrequencyToPitch
{
    public static float ConvertedPitch(float frequency)
    {
        // ogiltig frekvens
        if (frequency <= 0 || float.IsNaN(frequency) || float.IsInfinity(frequency))
        {
            return float.NaN;
        }

        return 69f + 12f * Mathf.Log(frequency / 440f, 2f);
        
    }
}
