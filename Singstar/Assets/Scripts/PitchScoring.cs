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
        const float zeroPointsAt = 1.0f;

        // Full poäng inom toleransen.
        if (absDifference <= tolerance)
        {
            return 1f;
        }

        // Noll poäng från en halvtons avstånd på 1,0.
        if (absDifference >= zeroPointsAt)
        {
            return 0f;
        }

        // Linjär minskning mellan toleransen och yttergränsen.
        return 1f - (absDifference - tolerance)
                   / (zeroPointsAt - tolerance);
    }
}