using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject gameView;
    [SerializeField] private MicrophoneInput microphoneInput;
    [SerializeField] private AudioSource songAudioSource;

    [SerializeField, Range(0f, 1f)] private float pitchTolerance = 0.3f;

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
        bool microphoneStarted = microphoneInput.StartSelectedMicrophone();

        if (!microphoneStarted)
        {
            return;
        }

        startMenu.SetActive(false);
        gameView.SetActive(true);

        songAudioSource.time = 0f;
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
            IsPlaying = false;
            return;
        }

        float detectedHz = microphoneInput.CurrentPitchHz;

        if (detectedHz <= 0f || float.IsNaN(detectedHz) || float.IsInfinity(detectedHz))
        {
            return;
        }

        HasPitch = true;
        CurrentMidi = FrequencyToPitch.ConvertedPitch(detectedHz);

        OnPitch = SongNotes.CheckPitch(CurrentTarget, CurrentMidi, pitchTolerance);

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