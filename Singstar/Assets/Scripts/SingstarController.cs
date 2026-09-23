using UnityEngine;

public class SingstarController : MonoBehaviour
{
    [Header("Inställningar")]
    public float targetFrequency = 220f; // Exempelvis er fasta målton
    
    [Header("Referenser")]
    // Dra in ditt InputSampling-script här via Unity-inspektorn
    public InputSampling inputSampling; 

    private readonly PitchDetection pitchDetector = new PitchDetection();

    // Avvikelsen i halvtoner (n)
    private float currentDeviation;

    void Update()
    {
        if (inputSampling == null) return;

        //  Hämta float-array och samplingsfrekvens från InputSampling (Exempel på namn)
        // (Kräver att du har publika metoder eller variabler för detta i InputSampling)
        float[] audioBlock = inputSampling.GetAudioSamples();
        int sampleRate = inputSampling.GetSampleRate();

        // Om vi inte har fått någon data än, avbryt för denna frame
        if (audioBlock == null || audioBlock.Length == 0) return;

        // Nuvarande detektor returnerar Hz, eller 0 om ingen ton hittas.
        float frequency = pitchDetector.DetectPitch(audioBlock, sampleRate);
        bool toneFound = frequency > 0f;

        //  Om en ton hittades, jämför med måltonen via PitchScoring
        if (toneFound)
        {
            // Beräkna n (avvikelse i halvtoner)
            currentDeviation = PitchScoring.CalculateSemitoneDeviation(frequency, targetFrequency);
            
            Debug.Log($"Sjungen frekvens: {frequency:F1} Hz. Avvikelse (n): {currentDeviation:F2} halvtoner.");
            
            // Här skickar du sedan 'currentDeviation' vidare till UI-scriptet 
            // för att flytta markören på skärmen upp eller ner!
        }
        else
        {
            // vad ska hända om Tonen inte hittades? fundera ihop. 
        }
    }
}