using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenDropoff : MonoBehaviour {

    private const float DROPOFF_HOLD_TIMER = 3.0f;

    private float m_dropoffTime = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player" || !other.gameObject.GetComponentInParent<PlayerController>().GetCarryingPatient())
            return;

        m_dropoffTime = Time.time + DROPOFF_HOLD_TIMER;

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag != "Player" || !other.gameObject.GetComponentInParent<PlayerController>().GetCarryingPatient())
            return;

        if (Time.time > m_dropoffTime)
        {
            FindObjectOfType<PlayerController>().DropOffSuccess();
            //TODO: dropoff logic (game time remaining, activate new patients, particle effects for player feedback)
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player" || !other.gameObject.GetComponentInParent<PlayerController>().GetCarryingPatient())
            return;

        m_dropoffTime = 0.0f;
    }

}
