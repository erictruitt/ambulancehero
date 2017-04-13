using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenDropoff : MonoBehaviour {

    private const float DROPOFF_HOLD_TIMER = 3.0f;

    private float m_dropoffTime = 0;

    public GameObject m_DropoffActiveParticles;

    public void ActivateDropoff()
    {
        m_DropoffActiveParticles.SetActive(true);
    }

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
            m_DropoffActiveParticles.SetActive(false);
            //TODO: dropoff logic (game time remaining)
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player" || !other.gameObject.GetComponentInParent<PlayerController>().GetCarryingPatient())
            return;

        m_dropoffTime = 0.0f;
    }

}
