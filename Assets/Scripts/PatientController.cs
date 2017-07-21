using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenStats
{
    //int m_citizenID;
    //bool m_active = false;
    //public int m_pickupRank = 0;
}

public class PatientController : MonoBehaviour {

    private const float PICKUP_HOLD_TIMER = 3.0f;
    private const int NUM_ICONS = 3;

    private float m_pickupTime;
    int m_citizenID;
    bool m_active = false;
    public int m_pickupRank = 0;

    private Transform m_startPosition;

    public GameObject m_citizenActiveParticles;
    public GameObject[] m_citizenActiveIcons;

    private void Start()
    {
        m_startPosition = transform;
    }

    public void Activate(int _id, int _difficulty = 3)
    {
        m_citizenID = _id;
        m_pickupTime = 0;
        m_active = true;
        SetPickupRank(_difficulty);

        //TODO: have animation go into death

        m_citizenActiveParticles.SetActive(true);

        for (int i = 0; i < _difficulty; i++)
        {
            m_citizenActiveIcons[i].SetActive(true);
        }
    }

    public void Deactivate()
    {
        m_active = false;
        SetPickupRank(0);

        //TODO: have animation go back into walk/idle

        m_citizenActiveParticles.SetActive(false);

        for (int i = 0; i < m_citizenActiveIcons.Length; i++)
        {
            m_citizenActiveIcons[i].SetActive(false);
        }
    }

    public void ReEnableCitizen()
    {
        gameObject.transform.position = m_startPosition.position;
        gameObject.transform.rotation = m_startPosition.rotation;

        m_citizenActiveParticles.SetActive(false);
        for (int i = 0; i < m_citizenActiveIcons.Length; i++)
        {
            m_citizenActiveIcons[i].SetActive(false);
        }

        gameObject.SetActive(true);

        //TODO: have animation go back into walk/idle
    }

    public void SetPickupRank(int _rank)
    {
        m_pickupRank = _rank;
    }

    public int GetPickupRank()
    {
        return m_pickupRank;
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
            other.gameObject.GetComponentInParent<PlayerController>().PickedUpPatient(m_pickupRank); //send player pickup info to add to timer

            GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
            gameManager.GetComponent<GameManager>().DeactivateUnchosenPatients(m_citizenID);
            gameManager.GetComponent<GameManager>().PlayPickupClip();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player" || !m_active)
            return;

        m_pickupTime = 0;
    }
}
