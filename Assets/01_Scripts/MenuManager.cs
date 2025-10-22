using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "SampleScene"; // Nombre de tu escena de juego

    [Header("Audio")]
    public AudioSource musicSource;      // Para la música de fondo
    public AudioSource sfxSource;        // Para efectos de sonido
    public AudioClip buttonClickSound;   // Sonido del click
    public float transitionDelay = 0.3f; // Tiempo de espera antes de cambiar escena

    private bool isTransitioning = false; // Para evitar múltiples clicks

    void Start()
    {
        // Verificar que los Audio Sources estén asignados
        if (musicSource == null || sfxSource == null)
        {
            Debug.LogError("¡Audio Sources no asignados en MenuManager!");
        }

        // La música ya debería estar sonando por Play On Awake
        // Pero podemos verificarlo:
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    // Función para el botón PLAY
    public void PlayGame()
    {
        if (isTransitioning) return; // Evitar múltiples clicks

        Debug.Log("Play button clicked!");
        StartCoroutine(LoadSceneWithDelay(gameSceneName));
    }

    // Función para el botón SETTINGS
    public void OpenSettings()
    {
        if (isTransitioning) return;

        Debug.Log("Settings button clicked!");
        PlayButtonSound();
        // Aquí puedes abrir un panel de settings
    }

    // Función para el botón QUIT
    public void QuitGame()
    {
        if (isTransitioning) return;

        Debug.Log("Quit button clicked!");
        StartCoroutine(QuitWithDelay());
    }

    // ✨ Coroutine para cargar escena con delay
    IEnumerator LoadSceneWithDelay(string sceneName)
    {
        isTransitioning = true;

        // Reproducir sonido de click
        PlayButtonSound();

        // Esperar a que termine el sonido
        yield return new WaitForSeconds(transitionDelay);

        // Detener la música (opcional, para transición suave)
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        // Cargar la escena
        SceneManager.LoadScene(sceneName);
    }

    // ✨ Coroutine para salir con delay
    IEnumerator QuitWithDelay()
    {
        isTransitioning = true;

        // Reproducir sonido de click
        PlayButtonSound();

        // Esperar a que termine el sonido
        yield return new WaitForSeconds(transitionDelay);

        // Salir del juego
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ✨ Función auxiliar para reproducir sonido de click
    void PlayButtonSound()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
        else
        {
            Debug.LogWarning("Button click sound not assigned or SFX source missing!");
        }
    }

    // Función pública para ajustar volumen de música (opcional)
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    // Función pública para ajustar volumen de SFX (opcional)
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
}