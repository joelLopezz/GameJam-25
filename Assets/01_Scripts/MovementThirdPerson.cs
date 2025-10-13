using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementThirdPerson : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 3f;
    public float velocidadCorrer = 6f;
    public float velocidadRotacion = 10f;

    [Header("Cámara")]
    public Transform camara;
    public float distanciaCamara = 3.5f;
    public float alturaCamara = 2f;
    public float sensibilidadCamara = 2f;
    public float suavizadoCamara = 5f;

    private Animator animator;
    private Rigidbody rb;
    private float anguloHorizontalCamara = 0f;
    private float anguloVerticalCamara = 10f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Bloquear cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Si no asignaste la cámara, búscala
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
        // Input del jugador
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Dirección relativa a la cámara
        Vector3 direccionCamara = camara.forward;
        direccionCamara.y = 0; // No queremos movimiento vertical
        direccionCamara.Normalize();

        Vector3 direccionDerecha = camara.right;
        direccionDerecha.y = 0;
        direccionDerecha.Normalize();

        // Combinar direcciones
        Vector3 direccionMovimiento = (direccionDerecha * horizontal + direccionCamara * vertical).normalized;

        if (direccionMovimiento.magnitude >= 0.1f)
        {
            // Rotar el personaje hacia donde se mueve
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionMovimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.fixedDeltaTime);

            // Mover
            float velocidadActual = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;
            Vector3 movimiento = direccionMovimiento * velocidadActual;
            rb.velocity = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        }
        else
        {
            // Detener movimiento horizontal
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void RotarCamara()
    {
        // Rotación de la cámara con el mouse
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadCamara;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadCamara;

        anguloHorizontalCamara += mouseX;
        anguloVerticalCamara -= mouseY;
        anguloVerticalCamara = Mathf.Clamp(anguloVerticalCamara, -20f, 60f); // Limitar ángulo vertical

        // Posicionar la cámara detrás y arriba del personaje
        Quaternion rotacion = Quaternion.Euler(anguloVerticalCamara, anguloHorizontalCamara, 0);
        Vector3 direccion = rotacion * Vector3.back;

        Vector3 posicionDeseada = transform.position + Vector3.up * alturaCamara + direccion * distanciaCamara;

        // Suavizar el movimiento de la cámara
        camara.position = Vector3.Lerp(camara.position, posicionDeseada, suavizadoCamara * Time.deltaTime);
        camara.LookAt(transform.position + Vector3.up * alturaCamara);
    }

    void ActualizarAnimacion()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool estaMoviendose = horizontal != 0 || vertical != 0;

        animator.SetBool("IsWalking", estaMoviendose);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}