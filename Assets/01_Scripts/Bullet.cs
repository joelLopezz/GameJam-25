using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    public GameObject impactEffect; // Opcional

    void OnTriggerEnter(Collider other)
    {
        // Ignorar colisi�n con el jugador
        if (other.gameObject.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Bala impact�: " + other.gameObject.name);

        // Efecto de impacto (si lo tienes)
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }

        // Si tiene componente de salud, hacer da�o
        //Health enemyHealth = other.GetComponent<Health>();
        //if (enemyHealth != null)
        //{
        //    enemyHealth.TakeDamage(damage);
        //}

        // Destruir la bala
        Destroy(gameObject);
    }
}