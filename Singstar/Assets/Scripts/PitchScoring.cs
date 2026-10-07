using UnityEngine;

public static class PitchScoring
{
    public static float GetPitchDifference(
        float detectedMidi,
        float targetMidi)
    {
        if (float.IsNaN(detectedMidi) || float.IsInfinity(detectedMidi) ||
            float.IsNaN(targetMidi) || float.IsInfinity(targetMidi))
        {
            return float.NaN;
        }

        float difference = detectedMidi - targetMidi;

        // Flytta skillnaden till närmaste oktav.
        while (difference > 6f) difference -= 12f;
        while (difference < -6f) difference += 12f;

        return difference;
    }

    public static float CalculateAccuracyFromMidi(
        float detectedMidi,
        float targetMidi,
        float tolerance)
    {
        float difference = GetPitchDifference(detectedMidi, targetMidi);

        if (float.IsNaN(difference))
        {
            return 0f;
        }

        float absDifference = Mathf.Abs(difference);
        const float zeroPointsAt = 1.0f;

        // Full poäng inom toleransen.
        if (absDifference <= tolerance)
        {
            return 1f;
        }

        // Noll poäng vid minst en halvtons avvikelse.
        if (absDifference >= zeroPointsAt)
        {
            return 0f;
        }

        // Linjär minskning däremellan.
        return 1f - (absDifference - tolerance)
                   / (zeroPointsAt - tolerance);
    }
}