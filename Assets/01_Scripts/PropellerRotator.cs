using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerRotator : MonoBehaviour
{
    public float idleRPM = 900f;

    void Update()
    {
        float rpm = idleRPM;
        float degreesPerSecond = rpm * 6f;
        transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.Self);
    }
}
