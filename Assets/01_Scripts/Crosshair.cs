using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    [Header("Configuraci�n")]
    public Image crosshairImage;
    public float expandAmount = 5f;        // Cu�nto se expande
    public float expandDuration = 0.1f;    // Duraci�n de la expansi�n
    public Color shootColor = Color.white; // Color al disparar

    private Vector2 originalSize;
    private Color originalColor;

    void Start()
    {
        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
        }

        if (crosshairImage != null)
        {
            originalSize = crosshairImage.rectTransform.sizeDelta;
            originalColor = crosshairImage.color;
        }
    }

    public void OnShoot()
    {
        if (crosshairImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(ExpandCrosshair());
        }
    }

    IEnumerator ExpandCrosshair()
    {
        // Expandir y cambiar color
        crosshairImage.rectTransform.sizeDelta = originalSize + Vector2.one * expandAmount;
        crosshairImage.color = shootColor;

        yield return new WaitForSeconds(expandDuration);

        // Volver al tama�o y color original
        crosshairImage.rectTransform.sizeDelta = originalSize;
        crosshairImage.color = originalColor;
    }
}