using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 3f;
    public float velocidadCorrer = 6f;
    public float suavizado = 10f;

    [Header("Rotación de Cámara")]
    public float sensibilidad = 2f;
    public Transform camara;
    public float limiteVerticalMin = -60f;
    public float limiteVerticalMax = 60f;

    private Animator animator;
    private Rigidbody rb;
    private float rotacionVertical = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Bloquear y ocultar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Si no asignaste la cámara en el inspector, búscala automáticamente
        if (camara == null)
        {
            camara = Camera.main.transform;
        }
    }

    void Update()
    {
        RotarCamara();
        ActualizarAnimacion();
    }

    void FixedUpdate()
    {
        Mover();
    }

    void Mover()
    {
        // Obtener input
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D o flechas
        float vertical = Input.GetAxisRaw("Vertical");     // W/S o flechas

        // Crear dirección de movimiento relativa a donde mira el personaje
        Vector3 direccion = transform.right * horizontal + transform.forward * vertical;
        direccion.Normalize();

        // Velocidad según si corre o camina
        float velocidadActual = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        // Aplicar movimiento
        Vector3 movimiento = direccion * velocidadActual;
        rb.velocity = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
    }

    void RotarCamara()
    {
        // Rotación horizontal (personaje completo)
        float mouseX = Input.GetAxis("Mouse X") * sensibilidad;
        transform.Rotate(Vector3.up * mouseX);

        // Rotación vertical (solo la cámara)
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad;
        rotacionVertical -= mouseY;
        rotacionVertical = Mathf.Clamp(rotacionVertical, limiteVerticalMin, limiteVerticalMax);
        camara.localRotation = Quaternion.Euler(rotacionVertical, 0f, 0f);
    }

    void ActualizarAnimacion()
    {
        // Detectar si hay movimiento
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool estaMoviendose = horizontal != 0 || vertical != 0;

        animator.SetBool("IsWalking", estaMoviendose);
    }

    // Para desbloquear el cursor (por ejemplo, al pausar)
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}