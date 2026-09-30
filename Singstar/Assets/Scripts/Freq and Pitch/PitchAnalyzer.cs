using UnityEngine;

public class PitchAnalyzer : MonoBehaviour
{
    [SerializeField] private PitchLogger pitchLogger;

    private readonly PitchDetection pitchDetection = new PitchDetection();
    private readonly PitchSmoother pitchSmoother = new PitchSmoother();

    public float CurrentPitchHz { get; private set; }

    public void Analyze(float[] monoSamples, int sampleRate)
    {
        float rawPitchHz =
            pitchDetection.DetectPitch(monoSamples, sampleRate);

        CurrentPitchHz = pitchSmoother.Process(rawPitchHz);

        pitchLogger.RecordMeasurement(rawPitchHz, CurrentPitchHz);
    }

    public void ResetAnalysis()
    {
        pitchSmoother.Reset();
        CurrentPitchHz = 0f;
    }
}