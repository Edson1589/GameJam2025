using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public float speed = 100f;

	void Start()
	{
		Debug.Log("Rotator iniciado - Posición: " + transform.position);
		Debug.Log("Rotator iniciado - Rotación: " + transform.eulerAngles);
	}

	void Update()
	{
		// Prueba diferentes ejes:

		// Opción 1: Eje Y mundial (debería girar horizontalmente)
		transform.Rotate(0f, speed * Time.deltaTime, 0f, Space.World);

		// Opción 2: Descomenta para probar eje Z mundial
		// transform.Rotate(0f, 0f, speed * Time.deltaTime, Space.World);

		// Opción 3: Descomenta para probar eje X mundial  
		// transform.Rotate(speed * Time.deltaTime, 0f, 0f, Space.World);
	}
}