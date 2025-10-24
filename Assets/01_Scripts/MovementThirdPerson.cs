using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementThirdPerson : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 3f;
    public float velocidadCorrer = 6f;
    public float velocidadRotacion = 10f;

    [Header("Salto")]
    public float fuerzaSalto = 5f;
    public LayerMask capaSuelo;
    public float distanciaRaycastSuelo = 0.6f;
    public float offsetRaycast = 0.1f;
    public float saltoCooldown = 0.3f;
    private bool esperandoPermiso = false;

    [Header("Disparo")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;
    public ParticleSystem muzzleFlash;
    public AudioClip shootSound;
    public LayerMask layersAIgnorar;

    [Header("Cámara")]
    public Transform camara;
    public float distanciaCamara = 3.5f;
    public float alturaCamara = 2f;
    public float sensibilidadCamara = 2f;
    public float suavizadoCamara = 5f;

    [Header("UI")]
    public Crosshair crosshair;

    private Animator animator;
    private Rigidbody rb;
    private AudioSource audioSource;
    private PauseManager pauseManager;
    private float anguloHorizontalCamara = 0f;
    private float anguloVerticalCamara = 10f;
    private bool estaEnSuelo;
    private bool puedeVolverASaltar = true;
    private float ultimoSaltoTime = -10f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        pauseManager = FindObjectOfType<PauseManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (camara == null)
        {
            camara = Camera.main.transform;
        }
    }

    void Update()
    {
        if (pauseManager != null && pauseManager.IsPaused())
        {
            return;
        }

        CheckGround();
        RotarCamara();
        RotarPersonaje();
        ActualizarAnimacion();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            IntentarSaltar();
        }

        if (Input.GetButtonDown("Fire1") && !IsPointerOverUI())
        {
            Disparar();
        }
    }

    void FixedUpdate()
    {
        if (pauseManager != null && pauseManager.IsPaused())
        {
            return;
        }

        Mover();
    }

    void CheckGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * offsetRaycast;
        RaycastHit hit;
        bool estabaEnSuelo = estaEnSuelo;

        bool raycastDetectoSuelo = Physics.Raycast(rayOrigin, Vector3.down, out hit, distanciaRaycastSuelo, capaSuelo);

        if (raycastDetectoSuelo)
        {
            float distanciaAlSuelo = hit.distance;
            bool velocidadCasiCero = rb.velocity.y > -0.8f && rb.velocity.y < 0.3f;
            bool muyMuyCerca = distanciaAlSuelo < 0.12f;

            estaEnSuelo = muyMuyCerca && velocidadCasiCero;
        }
        else
        {
            estaEnSuelo = false;
        }

        if (estaEnSuelo && !estabaEnSuelo && Mathf.Abs(rb.velocity.y) < 0.3f && !esperandoPermiso)
        {
            esperandoPermiso = true;
            StartCoroutine(PermitirSaltarDespuesDeAterrizar());
        }

        if (!estaEnSuelo && estabaEnSuelo)
        {
            puedeVolverASaltar = false;
            esperandoPermiso = false;
        }

        animator.SetBool("IsGrounded", estaEnSuelo);
    }

    IEnumerator PermitirSaltarDespuesDeAterrizar()
    {
        yield return new WaitForSeconds(0.2f);

        if (estaEnSuelo && Mathf.Abs(rb.velocity.y) < 0.3f && transform.position.y < 0.15f)
        {
            puedeVolverASaltar = true;
        }

        esperandoPermiso = false;
    }

    void IntentarSaltar()
    {
        bool cooldownCompleto = Time.time > ultimoSaltoTime + saltoCooldown;
        bool velocidadVerticalBaja = Mathf.Abs(rb.velocity.y) < 0.5f;

        bool puedesSaltar = estaEnSuelo &&
                            puedeVolverASaltar &&
                            cooldownCompleto &&
                            velocidadVerticalBaja;

        if (puedesSaltar)
        {
            Saltar();
        }
    }

    void Saltar()
    {
        if (Time.time < ultimoSaltoTime + 0.15f)
        {
            return;
        }

        rb.velocity = new Vector3(rb.velocity.x, fuerzaSalto, rb.velocity.z);
        animator.SetBool("IsJumping", true);

        puedeVolverASaltar = false;
        esperandoPermiso = false;
        estaEnSuelo = false;
        animator.SetBool("IsGrounded", false);

        ultimoSaltoTime = Time.time;

        StartCoroutine(DesactivarIsJumping());
    }

    IEnumerator DesactivarIsJumping()
    {
        yield return new WaitForEndOfFrame();
        animator.SetBool("IsJumping", false);
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
            {
                return true;
            }
        }

        return false;
    }

    void Disparar()
    {
        Ray ray = camara.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 1000f, ~layersAIgnorar))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 1000f;
        }

        Vector3 direccionDisparo = (targetPoint - firePoint.position).normalized;

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direccionDisparo));

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.velocity = direccionDisparo * bulletSpeed;
            }

            Destroy(bullet, 3f);
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // ✨ CAMBIO: Usar Bool en vez de Trigger
        animator.SetBool("IsShooting", true);
        StartCoroutine(DesactivarDisparo());

        if (crosshair != null)
        {
            crosshair.OnShoot();
        }
    }

    // ✨ NUEVA Coroutine para desactivar disparo
    IEnumerator DesactivarDisparo()
    {
        yield return new WaitForSeconds(0.3f); // Duración del disparo
        animator.SetBool("IsShooting", false);
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
        if (hasFocus && (pauseManager == null || !pauseManager.IsPaused()))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Visualización del raycast en el editor (solo en Scene View)
    void OnDrawGizmosSelected()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * offsetRaycast;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayOrigin, Vector3.down * distanciaRaycastSuelo);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(rayOrigin, 0.05f);
    }
}