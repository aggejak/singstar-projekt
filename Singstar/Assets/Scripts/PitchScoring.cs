using UnityEngine;

public static class PitchScoring
{
    public static float CalculateAccuracyFromMidi(
        float detectedMidi,
        float targetMidi,
        float tolerance)
    {
        if (float.IsNaN(detectedMidi) || float.IsInfinity(detectedMidi))
        {
            return 0f;
        }

        float difference = detectedMidi - targetMidi;

        // Flytta skillnaden till närmaste oktav
        while (difference > 6f)
        {
            difference -= 12f;
        }

        while (difference < -6f)
        {
            difference += 12f;
        }

        float absDifference = Mathf.Abs(difference);

        if (absDifference > tolerance)
        {
            return 0f;
        }

        // 1 = perfekt träff, 0 = utanför tolerans
        return 1f - (absDifference / tolerance);
    }
}