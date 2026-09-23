using UnityEngine;

public static class PitchScoring
{
    public static float CalculateSemitoneDeviation(float currentFrequency, float targetFrequency) 
    {
        // Förhindra krasch om frekvensen råkar vara 0 (log(0) är ogiltigt)
        if (currentFrequency <= 0f || targetFrequency <= 0f) 
        {
            return 0f;
        }

        float semitonesDiff = 12f * Mathf.Log(currentFrequency / targetFrequency, 2f);

        return semitonesDiff; 
    }
}