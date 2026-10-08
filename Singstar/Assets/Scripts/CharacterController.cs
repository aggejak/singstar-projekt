using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    // Game controller script
    [SerializeField] private GameController gameController;
    // Bilder på tommy
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite singingSprite;
    [SerializeField] private Sprite sadSprite;
    // delay innan bild ändras
    [SerializeField, Min(0f)] private float confirmationTime = 0.5f;
    [SerializeField, Min(0f)] private float neutralDelay = 1f;

    private enum Emotion { Neutral, Singing, Sad }

    private Image characterImage;
    private Emotion currentEmotion;
    private Emotion pendingEmotion;
    private double pendingSince;

    private bool inBreak;
    private double breakStarted;

    private void Awake()
    {
        characterImage = GetComponent<Image>();
        characterImage.preserveAspect = true;
        characterImage.raycastTarget = false;
    }

    private void OnEnable()
    {
        ResetToNeutral();
    }

    private void LateUpdate()
    {
        if (gameController == null ||
            !gameController.isActiveAndEnabled ||
            !gameController.IsPlaying)
        {
            ResetToNeutral();
            return;
        }
        double now = Time.timeAsDouble;

        if (gameController.CurrentTarget == null)
        {
            if (!inBreak)
            {
                inBreak = true;
                breakStarted = now;
            }

            // A gap interrupts pitch confirmation, but keeps the expression.
            pendingEmotion = currentEmotion;
            pendingSince = now;

            if (now - breakStarted >= neutralDelay)
            {
                SetEmotion(Emotion.Neutral);
                pendingEmotion = Emotion.Neutral;
            }

            return;
        }

        // A new note ends the break and starts fresh pitch confirmation.
        if (inBreak)
        {
            inBreak = false;
            pendingEmotion = currentEmotion;
            pendingSince = now;
        }

        bool correct = gameController.HasPitch && gameController.OnPitch;
        Emotion wanted = correct ? Emotion.Singing : Emotion.Sad;

        if (wanted == currentEmotion)
        {
            pendingEmotion = currentEmotion;
            pendingSince = now;
            return;
        }

        if (wanted != pendingEmotion)
        {
            pendingEmotion = wanted;
            pendingSince = now;
        }

        if (now - pendingSince >= confirmationTime)
        {
            SetEmotion(wanted);
        }
    }

    private void OnDisable()
    {
        ResetToNeutral();
    }

    private void ResetToNeutral()
    {
        inBreak = false;
        breakStarted = 0d;
        pendingEmotion = Emotion.Neutral;
        pendingSince = 0d;
        SetEmotion(Emotion.Neutral);
    }

    private void SetEmotion(Emotion emotion)
    {
        currentEmotion = emotion;

        if (characterImage == null)
            return;

        Sprite sprite = neutralSprite;

        if (emotion == Emotion.Singing)
            sprite = singingSprite;
        else if (emotion == Emotion.Sad)
            sprite = sadSprite;

        if (sprite == null)
            sprite = neutralSprite;

        if (characterImage.sprite != sprite)
            characterImage.sprite = sprite;
    }
}
