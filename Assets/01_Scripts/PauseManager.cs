using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;
using static UnityEditor.ShaderData;
using static UnityEngine.UIElements.VisualElement;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;  // Panel completo de pausa

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip pauseSound;
    public AudioClip resumeSound;
    public AudioClip buttonClickSound;

    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape; // Tecla para pausar
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Start()
    {
        // Asegurarse de que el panel esté oculto al inicio
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Asegurarse de que el tiempo esté corriendo
        Time.timeScale = 1f;

        // Desbloquear el cursor al inicio (para el juego)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Detectar tecla de pausa (ESC)
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // ✨ Pausar el juego
    public void PauseGame()
    {
        if (isPaused) return; // Ya está pausado

        isPaused = true;

        // Mostrar el panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // DETENER EL TIEMPO (esto pausa todo el juego)
        Time.timeScale = 0f;

        // Mostrar y desbloquear el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Sonido de pausa
        PlaySound(pauseSound);

        Debug.Log("Juego pausado");
    }

    // ✨ Reanudar el juego
    public void ResumeGame()
    {
        if (!isPaused) return; // Ya está corriendo

        isPaused = false;

        // Ocultar el panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // REANUDAR EL TIEMPO
        Time.timeScale = 1f;

        // Bloquear y ocultar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Sonido de resume
        PlaySound(resumeSound);

        Debug.Log("Juego reanudado");
    }

    // ✨ Ir a opciones (dentro del pause)
    public void OpenSettings()
    {
        PlaySound(buttonClickSound);
        Debug.Log("Abriendo opciones...");
        // Aquí puedes activar otro panel de opciones
        // settingsPanel.SetActive(true);
    }

    // ✨ Volver al menú principal
    public void BackToMainMenu()
    {
        PlaySound(buttonClickSound);

        // IMPORTANTE: Reanudar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;

        // Cargar el menú principal
        Debug.Log("Volviendo al menú principal...");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Función auxiliar para reproducir sonidos
    void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            // IMPORTANTE: usar PlayOneShot con Time.timeScale = 0
            // porque Play() no funciona cuando el juego está pausado
            sfxSource.PlayOneShot(clip);
        }
    }

    // Función pública para saber si está pausado (útil para otros scripts)
    public bool IsPaused()
    {
        return isPaused;
    }
}