using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenDropoff : MonoBehaviour {

    private const float DROPOFF_HOLD_TIMER = 3.0f;

    private float m_dropoffTime = 0;

    public GameObject m_DropoffActiveParticles;

    private AudioSource m_audioSource;

    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
    }

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
            m_audioSource.Play();
            FindObjectOfType<PlayerController>().DropOffSuccess();
            m_DropoffActiveParticles.SetActive(false);
            GameManager temp = FindObjectOfType<GameManager>();
            temp.IncreaseTimer(other.gameObject.GetComponentInParent<PlayerController>().GetCurrPatientRank() * 5.0f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player" || !other.gameObject.GetComponentInParent<PlayerController>().GetCarryingPatient())
            return;

        m_dropoffTime = 0.0f;
    }

}
