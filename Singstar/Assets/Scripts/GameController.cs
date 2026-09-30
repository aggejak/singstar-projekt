using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject gameView;
    [SerializeField] private MicrophoneInput microphoneInput;
    [SerializeField] private AudioSource songAudioSource;
    [SerializeField] private PitchLogger pitchLogger;
    [SerializeField] private PitchAnalyzer pitchAnalyzer;

    [SerializeField, Range(0f, 0.9f)] private float pitchTolerance = 0.5f;
    public enum InputMode
    {
        Microphone,
        AudioFile
    }

    [SerializeField] private InputMode inputMode;
    [SerializeField] private AudioFileInput audioFileInput;

    [Header("Scoring")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float maxPointsPerSecond = 100f;

    public bool IsPlaying { get; private set; }
    public bool HasPitch { get; private set; }
    public float CurrentMidi { get; private set; }
    public TargetNote CurrentTarget { get; private set; }
    public bool OnPitch { get; private set; }
    public float SongTime => songAudioSource.time;

    public float Score { get; private set; }

    private void Start()
    {
        startMenu.SetActive(true);
        gameView.SetActive(false);
        UpdateScoreUI();
    }

    public void StartGame()
    {
        // Stoppa eventuell tidigare källa innan en ny startas.
        audioFileInput.StopInput();
        microphoneInput.enabled = false;

        bool inputStarted;

        if (inputMode == InputMode.Microphone)
        {
            microphoneInput.enabled = true;
            inputStarted = microphoneInput.StartSelectedMicrophone();
        }
        else
        {
            inputStarted = audioFileInput.StartInput();
        }

        if (!inputStarted)
        {
            return;
        }

        startMenu.SetActive(false);
        gameView.SetActive(true);

        songAudioSource.time = 0f;

        pitchLogger.BeginRecording(); // Spara ljudsamples

        songAudioSource.Play();

        Score = 0f;
        UpdateScoreUI();

        IsPlaying = true;
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        HasPitch = false;
        OnPitch = false;
        CurrentMidi = float.NaN;
        CurrentTarget = SongNotes.GetCurrentNote(SongTime);

        if (!songAudioSource.isPlaying)
        {
            audioFileInput.StopInput();
            microphoneInput.enabled = false;

            pitchLogger.SaveRecording();
            IsPlaying = false;
            return;
        }

        float detectedHz = pitchAnalyzer.CurrentPitchHz;

        if (detectedHz <= 0f || float.IsNaN(detectedHz) || float.IsInfinity(detectedHz))
        {
            return;
        }

        HasPitch = true;

        CurrentMidi = FrequencyToPitch.ConvertedPitch(detectedHz);




        //OnPitch = SongNotes.CheckPitch(CurrentTarget, CurrentMidi, pitchTolerance);


        OnPitch = CurrentTarget != null &&
            PitchScoring.CalculateAccuracyFromMidi(
                CurrentMidi,
                CurrentTarget.targetMidi,
                pitchTolerance
            ) > 0f;

        UpdateScore();

        Debug.Log(
            $"Tid: {SongTime:F2} s | " +
            $"MIDI: {CurrentMidi:F2} | " +
            $"Rätt ton: {OnPitch} | " +
            $"Score: {Score:F0}"
        );
    }

    private void UpdateScore()
    {
        if (CurrentTarget == null || !HasPitch)
        {
            return;
        }

        float accuracy = PitchScoring.CalculateAccuracyFromMidi(
            CurrentMidi,
            CurrentTarget.targetMidi,
            pitchTolerance
        );

        float pointsThisFrame =
            accuracy * maxPointsPerSecond * Time.deltaTime;

        Score += pointsThisFrame;

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)  
        {
            scoreText.text = $"Score: {Mathf.RoundToInt(Score)}";
        }
    }
}