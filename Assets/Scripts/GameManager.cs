using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const float GAME_TIMER = 90.0f;
    private const int PATIENTS_TO_ACTIVATE = 3;

    public GameObject[] m_citizens;
    public Queue<int> m_unavailableCitizens;
    public List<int> m_currentlyActivatedPatients;

    public Text m_gameTimerUI;

    private float m_gameEndTime;
    private int m_timeLeft;

    private void Start()
    {
        m_gameTimerUI.text = GAME_TIMER.ToString();
        m_gameEndTime = Time.time + GAME_TIMER;

        m_citizens = GameObject.FindGameObjectsWithTag("Citizen");

        m_unavailableCitizens = new Queue<int>();
        m_currentlyActivatedPatients = new List<int>();
    }

    private void UpdateGameTime()
    {
        m_timeLeft = (int)(m_gameEndTime - Time.time) +1;

        m_gameTimerUI.text = m_timeLeft.ToString();

        if (m_timeLeft <= 0)
        {
            EndGame();
        }
    }

    private void OnGUI()
    {
        UpdateGameTime();
    }

    void EndGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IncreaseTimer(float _seconds)
    {
        m_gameEndTime += _seconds;
    }

    public void ActivatePatients()
    {
        for (int i = 0; i < PATIENTS_TO_ACTIVATE; i++)
        {
            int patientToActivate = Random.Range(0, m_citizens.Length - 1);

            if (m_unavailableCitizens.Contains(patientToActivate) || m_currentlyActivatedPatients.Contains(patientToActivate))
            {
                i--;
                continue;
            }

            m_currentlyActivatedPatients.Add(patientToActivate);
            m_citizens[patientToActivate].GetComponent<PatientController>().Activate(patientToActivate);
        }
    }

    public void DeactivateUnchosenPatients(int _chosenPatient)
    {
        m_unavailableCitizens.Enqueue(_chosenPatient);
        if (m_unavailableCitizens.Count > 10)
        {
            int patientToReEnable = m_unavailableCitizens.Dequeue();
            m_citizens[patientToReEnable].GetComponent<PatientController>().ReEnableCitizen();
        }

        m_currentlyActivatedPatients.Remove(_chosenPatient);

        int numActivatedPatients = m_currentlyActivatedPatients.Count;

        for (int i = 0; i < numActivatedPatients; i++)
        {
            m_citizens[m_currentlyActivatedPatients[i]].GetComponent<PatientController>().Deactivate();
        }

        m_currentlyActivatedPatients.Clear();
    }

}
