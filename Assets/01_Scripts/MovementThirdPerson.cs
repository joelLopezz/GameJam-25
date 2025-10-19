using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MovementThirdPerson : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 3f;
    public float velocidadCorrer = 6f;
    public float velocidadRotacion = 10f;

    [Header("Salto")]
    public float fuerzaSalto = 5f;
    public LayerMask capaSuelo;
    public Transform groundCheck; // Punto para detectar suelo
    public float distanciaGroundCheck = 0.2f;

    [Header("Disparo")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public ParticleSystem muzzleFlash;
    public AudioClip shootSound;

    [Header("Cámara")]
    public Transform camara;
    public float distanciaCamara = 3.5f;
    public float alturaCamara = 2f;
    public float sensibilidadCamara = 2f;
    public float suavizadoCamara = 5f;

    private Animator animator;
    private Rigidbody rb;
    private AudioSource audioSource;
    private float anguloHorizontalCamara = 0f;
    private float anguloVerticalCamara = 10f;
    private bool estaEnSuelo;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (camara == null)
        {
            camara = Camera.main.transform;
        }

        // Crear groundCheck si no existe
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.parent = transform;
            gc.transform.localPosition = new Vector3(0, 0, 0);
            groundCheck = gc.transform;
        }
    }

    void Update()
    {
        CheckGround();
        RotarCamara();
        RotarPersonaje();
        ActualizarAnimacion();

        // ✨ SALTO
        if (Input.GetKeyDown(KeyCode.Space) && estaEnSuelo)
        {
            Saltar();
        }

        // ✨ DISPARO
        if (Input.GetButtonDown("Fire1")) // Cambié GetButton por GetButtonDown para un disparo por click
        {
            Disparar();
        }
    }

    void FixedUpdate()
    {
        Mover();
    }

    void CheckGround()
    {
        // Detectar si está en el suelo
        estaEnSuelo = Physics.CheckSphere(groundCheck.position, distanciaGroundCheck, capaSuelo);
        animator.SetBool("IsGrounded", estaEnSuelo);
    }

    void Saltar()
    {
        // Aplicar fuerza de salto
        rb.velocity = new Vector3(rb.velocity.x, fuerzaSalto, rb.velocity.z);

        // Activar animación
        animator.SetTrigger("Jump");

        Debug.Log("¡Saltando!");
    }

    void Disparar()
    {
        // Crear la bala
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.velocity = firePoint.forward * bulletSpeed;
            }

            Destroy(bullet, 3f);
        }

        // Sonido de disparo
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Animación de disparo
        animator.SetTrigger("Shoot");
    }

    void Mover()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direccionCamara = camara.forward;
        direccionCamara.y = 0;
        direccionCamara.Normalize();

        Vector3 direccionDerecha = camara.right;
        direccionDerecha.y = 0;
        direccionDerecha.Normalize();

        Vector3 direccionMovimiento = (direccionDerecha * horizontal + direccionCamara * vertical).normalized;

        float velocidadActual = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        if (direccionMovimiento.magnitude >= 0.1f)
        {
            Vector3 movimiento = direccionMovimiento * velocidadActual;
            rb.velocity = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void RotarPersonaje()
    {
        Vector3 direccionCamara = camara.forward;
        direccionCamara.y = 0;

        if (direccionCamara.magnitude > 0.1f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionCamara);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);
        }
    }

    void RotarCamara()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadCamara;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadCamara;

        anguloHorizontalCamara += mouseX;
        anguloVerticalCamara -= mouseY;
        anguloVerticalCamara = Mathf.Clamp(anguloVerticalCamara, -20f, 60f);

        Quaternion rotacion = Quaternion.Euler(anguloVerticalCamara, anguloHorizontalCamara, 0);
        Vector3 direccion = rotacion * Vector3.back;

        Vector3 posicionDeseada = transform.position + Vector3.up * alturaCamara + direccion * distanciaCamara;

        camara.position = Vector3.Lerp(camara.position, posicionDeseada, suavizadoCamara * Time.deltaTime);
        camara.LookAt(transform.position + Vector3.up * alturaCamara);
    }

    void ActualizarAnimacion()
    {
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");

        Vector3 direccionCamara = camara.forward;
        direccionCamara.y = 0;
        direccionCamara.Normalize();

        Vector3 direccionDerecha = camara.right;
        direccionDerecha.y = 0;
        direccionDerecha.Normalize();

        Vector3 movimientoMundo = (direccionDerecha * inputHorizontal + direccionCamara * inputVertical);
        Vector3 movimientoLocal = transform.InverseTransformDirection(movimientoMundo);

        float horizontalNormalizado = Mathf.Clamp(movimientoLocal.x, -1f, 1f);
        float verticalNormalizado = Mathf.Clamp(movimientoLocal.z, -1f, 1f);

        animator.SetFloat("Horizontal", horizontalNormalizado);
        animator.SetFloat("Vertical", verticalNormalizado);

        bool estaMoviendose = Mathf.Abs(inputHorizontal) > 0.1f || Mathf.Abs(inputVertical) > 0.1f;
        bool estaCorriendo = Input.GetKey(KeyCode.LeftShift) && estaMoviendose;

        animator.SetBool("IsRunning", estaCorriendo);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Para visualizar el groundCheck en el editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, distanciaGroundCheck);
        }
    }
}