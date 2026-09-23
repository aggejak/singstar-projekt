using UnityEngine;

public class SingstarController : MonoBehaviour
{
    [Header("Inställningar")]
    public float targetFrequency = 220f; // Exempelvis er fasta målton
    
    [Header("Referenser")]
    // Dra in ditt InputSampling-script här via Unity-inspektorn
    public InputSampling inputSampling; 

    // Avvikelsen i halvtoner (n)
    private float currentDeviation;

    void Update()
    {
        //  Hämta float-array och samplingsfrekvens från InputSampling (Exempel på namn)
        // (Kräver att du har publika metoder eller variabler för detta i InputSampling)
        float[] audioBlock = inputSampling.GetAudioSamples();
        int sampleRate = inputSampling.GetSampleRate();

        // Om vi inte har fått någon data än, avbryt för denna frame
        if (audioBlock == null || audioBlock.Length == 0) return;

        //  Skicka ljudblocket till PitchDetector
        // (Använder det statiska funktionsanropet vi skapade tidigare)
        PitchResult result = PitchDetector.DetectPitch(audioBlock, sampleRate);

        //  Om en ton hittades, jämför med måltonen via PitchScoring
        if (result.ToneFound)
        {
            // Beräkna n (avvikelse i halvtoner)
            currentDeviation = PitchScoring.CalculateSemitoneDeviation(result.Frequency, targetFrequency);
            
            Debug.Log($"Sjungen frekvens: {result.Frequency:F1} Hz. Avvikelse (n): {currentDeviation:F2} halvtoner.");
            
            // Här skickar du sedan 'currentDeviation' vidare till UI-scriptet 
            // för att flytta markören på skärmen upp eller ner!
        }
        else
        {
            // vad ska hända om Tonen inte hittades? fundera ihop. 
        }
    }
}