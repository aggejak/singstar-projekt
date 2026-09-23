using UnityEngine;

public class PitchDetection
{
    // Metod som tar in ljudets samplingar och samplingsfrekvens i Hz
    // Retunerar en frekvens om hittad
    public float DetectPitch(float[] samples, int sampleRate)
    {
        if (samples == null || samples.Length == 0 || sampleRate <= 0)
        {
            return 0f;
        }

        // Räkna ut RMS för våra samples

        float sumSquares = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sumSquares += samples[i] * samples[i];
        }

        float rms = Mathf.Sqrt(sumSquares / samples.Length);

        // Kollar om ljudnivån är tillräckligt stark.

        float silenceThreshold = 0.01f;

        if (rms < silenceThreshold)
        {
            return 0f;
        }

        // Sätter gränser för rimliga frekvenser (80 - 1000 Hz)
        // Och räknar ut intervall där Lag = (samplerate / frequency)

        float minFrequency = 80f;
        float maxFrequency = 1000f;

        int minLag = Mathf.CeilToInt(sampleRate / maxFrequency);
        int maxLag = Mathf.FloorToInt(sampleRate / minFrequency);

        //Kolla så maxLag inte är längre än halva blocket

        maxLag = Mathf.Min(maxLag, samples.Length / 2);


        //Autokorrelkationen

        float[] correlations = new float[maxLag + 1];

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float correlation = 0f;
            float energyOriginal = 0f;
            float energyShifted = 0f;

            for (int i = 0; i < samples.Length - lag; i++)
            {
                correlation += samples[i] * samples[i + lag];
                energyOriginal += samples[i] * samples[i];
                energyShifted += samples[i + lag] * samples[i + lag];
            }
            float normalization = Mathf.Sqrt(energyOriginal * energyShifted);

            if (normalization > 0f)
            {
                correlation /= normalization;
            }
            else
            {
                correlation = 0f;
            }
            correlations[lag] = correlation;
        }

        // Returnerar frekvens vid första peak över threshold

        float correlationThreshold = 0.8f;

        for (int lag = minLag + 1; lag < maxLag; lag++)
        {
            bool isPeak = correlations[lag] > correlations[lag - 1]
                       && correlations[lag] > correlations[lag + 1];

            if (isPeak && correlations[lag] >= correlationThreshold)
            {
                return (float)sampleRate / lag;
            }
        }

        return 0f;
    }

}
