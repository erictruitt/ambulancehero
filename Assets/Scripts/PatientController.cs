using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientController : MonoBehaviour {

    private const float PICKUP_HOLD_TIMER = 3.0f;

    private float m_pickupTime;
    int m_citizenID;
    bool m_active = false;

    private Transform m_startPosition;

    private void Start()
    {
        m_startPosition = transform;
    }

    public void Activate(int _id)
    {
        m_citizenID = _id;
        m_pickupTime = 0;
        m_active = true;

        //TODO: have animation go into death
        //TODO: enable particles around patient
    }

    public void Deactivate()
    {
        m_active = false;

        //TODO: have animation go back into walk/idle
        //TODO: disable particles;

    }

    public void ReEnableCitizen()
    {
        gameObject.transform.position = m_startPosition.position;
        gameObject.transform.rotation = m_startPosition.rotation;
        gameObject.SetActive(true);

        //TODO: have animation go back into walk/idle
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player" || !m_active)
            return;

        m_pickupTime = Time.time + PICKUP_HOLD_TIMER;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag != "Player" || !m_active)
            return;

        if (Time.time > m_pickupTime)
        {
            gameObject.SetActive(false);

            other.gameObject.GetComponentInParent<PlayerController>().PickedUpPatient();
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().DeactivateUnchosenPatients(m_citizenID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player" || !m_active)
            return;

        m_pickupTime = 0;
    }
}
