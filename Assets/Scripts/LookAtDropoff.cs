using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtDropoff : MonoBehaviour
{
    public GameObject dropoff;
    private void FixedUpdate()
    {
        dropoff = GameObject.FindGameObjectWithTag("Dropoff");
        transform.LookAt(dropoff.transform);
    }
}
