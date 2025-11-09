using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
	public float force = 15f;
	public float stunTime = 0.5f;
	public bool debugMode = true;

	void OnCollisionEnter(Collision collision)
	{
		if (debugMode) Debug.Log($"Colisión detectada con: {collision.gameObject.name}");

		// Verificar si es el player
		if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponent<PlayerController>() != null)
		{
			Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

			if (debugMode)
			{
				Debug.Log($"Rigidbody encontrado: {playerRb != null}");
				if (playerRb != null) Debug.Log($"Velocidad actual del player: {playerRb.velocity}");
			}

			if (playerRb != null)
			{
				// Calcular dirección del bounce (opuesta a la normal de colisión)
				Vector3 bounceDirection = -collision.contacts[0].normal.normalized;

				if (debugMode)
				{
					Debug.Log($"Normal de colisión: {collision.contacts[0].normal}");
					Debug.Log($"Dirección de bounce: {bounceDirection}");
					Debug.Log($"Fuerza aplicada: {bounceDirection * force}");
				}

				// Método 1: Resetear y aplicar fuerza (más efectivo)
				playerRb.velocity = new Vector3(0, playerRb.velocity.y, 0); // Solo resetear velocidad horizontal
				playerRb.AddForce(bounceDirection * force, ForceMode.VelocityChange);

				// Método 2: Alternativa más agresiva
				// Vector3 newVelocity = bounceDirection * force;
				// newVelocity.y = playerRb.velocity.y; // Mantener velocidad Y para gravedad
				// playerRb.velocity = newVelocity;

				if (debugMode) Debug.Log("BOUNCE APLICADO - Nueva velocidad: " + playerRb.velocity);

				// Opcional: Añadir efecto visual/sonido
				StartCoroutine(VisualFeedback());
			}
		}
	}

	private IEnumerator VisualFeedback()
	{
		// Efecto visual temporal
		Renderer renderer = GetComponent<Renderer>();
		Color originalColor = Color.white;

		if (renderer != null)
		{
			originalColor = renderer.material.color;
			renderer.material.color = Color.red;
			yield return new WaitForSeconds(0.3f);
			renderer.material.color = originalColor;
		}
	}
}