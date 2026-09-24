using UnityEngine;

public class PitchDetectionTest : MonoBehaviour
{
    [Range(100f, 900f)]
    public float testFrequency = 200f;
    void Start()
    {
        int sampleRate = 48000;
        float[] samples = new float[2048];

        for (int i = 0; i < samples.Length; i++)
        {
            float time = (float)i / sampleRate;
            samples[i] = 0.5f * Mathf.Sin(
                2f * Mathf.PI * testFrequency * time);
        }

        PitchDetection detector = new PitchDetection();
        float detectedFrequency = detector.DetectPitch(samples, sampleRate);

        Debug.Log("Hittad frekvens: " + detectedFrequency + " Hz");
    }
}
