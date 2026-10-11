
using System.Collections;
using UnityEngine;

public class AssemblyAnimation : MonoBehaviour
{
    [Header("Bauteile in der Reihenfolge Boden, Lüfter, Deckel")]
    public Transform[] parts;

    [Header("Passende finale Montagepositionen")]
    public Transform[] finalPositions;

    [Header("Animationsdauer in Sekunden")]
    public float animationDuration = 1.5f;

    private Vector3[] explodedPositions;
    private Quaternion[] explodedRotations;

    private int currentStep = 0;
    private bool isAnimating = false;

    void Start()
    {
        if (parts == null || finalPositions == null ||
            parts.Length != 3 || finalPositions.Length != 3)
        {
            Debug.LogError(
                "Bitte genau drei Bauteile und drei Zielpositionen zuweisen."
            );
            enabled = false;
            return;
        }

        explodedPositions = new Vector3[parts.Length];
        explodedRotations = new Quaternion[parts.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            explodedPositions[i] = parts[i].localPosition;
            explodedRotations[i] = parts[i].localRotation;
        }
    }

    public void NextStep()
    {
        if (isAnimating || currentStep >= parts.Length)
            return;

        StartCoroutine(MovePart(currentStep));
    }

    public void ResetAssembly()
    {
        if (!isAnimating)
            StartCoroutine(ResetParts());
    }

    private IEnumerator MovePart(int index)
    {
        isAnimating = true;

        Transform part = parts[index];
        Transform target = finalPositions[index];

        Vector3 startPosition = part.localPosition;
        Quaternion startRotation = part.localRotation;

        // Zielposition in den lokalen Koordinaten des Bauteil-Elternobjekts
        Vector3 endPosition =
            part.parent.InverseTransformPoint(target.position);

        Quaternion endRotation =
            Quaternion.Inverse(part.parent.rotation) * target.rotation;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / animationDuration);
            // Weiches Anfahren und Abbremsen
            t = t * t * (3f - 2f * t);

            part.localPosition =
                Vector3.Lerp(startPosition, endPosition, t);

            part.localRotation =
                Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        part.localPosition = endPosition;
        part.localRotation = endRotation;

        currentStep++;
        isAnimating = false;
    }

    private IEnumerator ResetParts()
    {
        isAnimating = true;

        for (int i = 0; i < parts.Length; i++)
        {
            Transform part = parts[i];

            Vector3 startPosition = part.localPosition;
            Quaternion startRotation = part.localRotation;

            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / animationDuration);
                t = t * t * (3f - 2f * t);

                part.localPosition = Vector3.Lerp(
                    startPosition, explodedPositions[i], t
                );

                part.localRotation = Quaternion.Slerp(
                    startRotation, explodedRotations[i], t
                );

                yield return null;
            }

            part.localPosition = explodedPositions[i];
            part.localRotation = explodedRotations[i];
        }

        currentStep = 0;
        isAnimating = false;
    }
}
