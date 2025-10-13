using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 2f;                     // Velocidad de movimiento
    public Transform[] patrolPoints;            // Puntos de patrulla
    private int currentPointIndex = 0;

    [Header("Detección del jugador")]
    public Transform player;
    public float detectionRange = 5f;           // Rango de detección del jugador
    private bool chasingPlayer = false;

    private void Start()
    {
        if (patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[0].position;
        }
    }

    private void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Si el jugador está dentro del rango, lo persigue
            if (distanceToPlayer <= detectionRange)
            {
                chasingPlayer = true;
            }
            else
            {
                chasingPlayer = false;
            }
        }

        if (chasingPlayer)
        {
            PerseguirJugador();
        }
        else
        {
            Patrullar();
        }
    }

    private void Patrullar()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            currentPointIndex++;
            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0; // vuelve al primer punto
            }
        }
    }

    private void PerseguirJugador()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        // Muestra el rango de detección en la escena
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
