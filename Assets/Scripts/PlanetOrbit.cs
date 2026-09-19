using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    public Transform sun;

    public float orbitSpeed;

    public float rotationSpeed = 50.0f;

    // Store the original speed of each planet
    [HideInInspector]
    public float baseOrbitSpeed;

    void Awake()
    {
        baseOrbitSpeed = orbitSpeed;
    }

    void Update()
    {
        if (sun != null)
        {
            transform.RotateAround(
                sun.position,
                Vector3.up,
                orbitSpeed * Time.deltaTime
            );

            transform.Rotate(
                Vector3.up,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // Called by the speed controller
    public void SetSpeed(float multiplier)
    {
        orbitSpeed = baseOrbitSpeed * multiplier;
    }
}