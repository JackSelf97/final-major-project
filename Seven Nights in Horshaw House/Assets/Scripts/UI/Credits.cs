using System.Collections;
using UnityEngine;

public class Credits : MonoBehaviour
{
    private UIManager UIManager = null;

    [SerializeField] private GameObject creditsPanel = null;
    [SerializeField] private float animationDuration = 40f;
    private float startPositionOffset = -1140f;
    private float endPosition = 1140f;

    private Coroutine creditsAnimationCoroutine;

    private void Start()
    {
        UIManager = GetComponent<UIManager>();

        // Initially set the panel position to the off-screen start position
        creditsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, startPositionOffset, 0);

        // Deactivate the credits panel
        InitialiseCredits(false);
    }

    public void ShowCredits()
    {
        // Activate the credits panel
        InitialiseCredits(true);

        if (creditsAnimationCoroutine == null)
        {
            creditsAnimationCoroutine = StartCoroutine(AnimateCredits());
        }
    }

    public void SkipCredits()
    {
        if (creditsAnimationCoroutine != null)
        {
            StopCoroutine(creditsAnimationCoroutine);
            ResetCreditsPosition();
            creditsAnimationCoroutine = null;

            // Deactivate the credits panel
            InitialiseCredits(false);
        }
        Debug.Log("Skipping credits");
    }

    private IEnumerator AnimateCredits()
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;

            // Interpolate position using Lerp
            creditsPanel.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(0, startPositionOffset, 0), new Vector3(0, endPosition, 0), t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is set
        creditsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, endPosition, 0);

        creditsAnimationCoroutine = null;

        // Deactivate the credits panel
        InitialiseCredits(false);

        // Restart the credits
        UIManager.ResetCredits();
    }

    private void ResetCreditsPosition()
    {
        // Set the panel position back to the off-screen start position
        creditsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, startPositionOffset, 0);
    }

    private void InitialiseCredits(bool state)
    {
        creditsPanel.SetActive(state);
    }
}