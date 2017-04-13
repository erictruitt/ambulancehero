using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinIcon : MonoBehaviour {

    private const float SPIN_SPEED = 1.0f;

    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(0.0f, SPIN_SPEED, 0.0f));
    }
}
