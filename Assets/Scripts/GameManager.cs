using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const float GAME_TIMER = 90.0f;
    private const int PATIENTS_TO_ACTIVATE = 3;
    private const int MAX_UNAVAILABLE_PATIENTS = 10;

    public GameObject[] m_citizens;
    public Queue<int> m_unavailableCitizens;
    public List<int> m_currentlyActivatedPatients;

    public Text m_gameTimerUI;

    private float m_gameEndTime;
    private int m_timeLeft;

    private bool m_gameOver;
    private int m_gameOverInfoController;

    public GameObject m_HUD;
    public GameObject m_timeUpPanel;
    public GameObject m_interactivePanel;

    public Text m_livesCountUI_TXT;
    public Text m_timeCountUI_TXT;
    public Text m_speedCountUI_TXT;

    public PlayerStatistics m_playerStats;

    private AudioSource m_audioSource;
    public AudioClip m_gameOverCounterSFX;
    public AudioClip m_citizenPickupSFX;
    //public AudioClip m_timeUpClip;

    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.clip = m_citizenPickupSFX;

        m_gameTimerUI.text = GAME_TIMER.ToString();
        m_gameEndTime = Time.time + GAME_TIMER;

        m_citizens = GameObject.FindGameObjectsWithTag("Citizen");

        m_unavailableCitizens = new Queue<int>();
        m_currentlyActivatedPatients = new List<int>();

        m_gameOver = false;
        m_gameOverInfoController = 0;
    }

    private void OnGUI()
    {
        if (!m_gameOver)
        {
            UpdateGameTime();
            return;
        }

        GameOverState();
    }

    private void Update()
    {
        if (Input.GetKey("escape"))
            Application.Quit();
    }

    private void UpdateGameTime()
    {
        m_timeLeft = (int)(m_gameEndTime - Time.time) + 1;

        m_gameTimerUI.text = m_timeLeft.ToString();

        if (m_timeLeft <= 0)
        {
            EndGame();
        }
    }


    void EndGame()
    {
        m_gameOver = true;

        m_gameEndTime += 3.0f;

        m_playerStats = FindObjectOfType<PlayerController>().GetPlayerStats();
    }

    public void IncreaseTimer(float _seconds)
    {
        m_gameEndTime += _seconds;
    }

    public void PlayPickupClip()
    {
        m_audioSource.Play();
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
        if (m_unavailableCitizens.Count > MAX_UNAVAILABLE_PATIENTS)
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

        GameObject.FindGameObjectWithTag("Dropoff").GetComponent<CitizenDropoff>().ActivateDropoff();
    }

    //FOLLOWING FUNCTIONS CONTROL DISPLAY OF GAME OVER MENUS
    private void GameOverState()
    {
        if (m_gameOverInfoController == 0)
        {
            DisplayTimesUp();
        }
        else if (m_gameOverInfoController == -1)
        {
            StopCoroutine(DisplayTopSpeed());
        }

    }

    private void DisplayTimesUp()
    {
        if (!m_timeUpPanel.activeInHierarchy)
        {
            //m_audioSource.clip = m_timeUpClip;
            m_HUD.SetActive(false);
            m_timeUpPanel.SetActive(true);
        }

        if (Time.time > m_gameEndTime)
        {
            m_audioSource.clip = m_gameOverCounterSFX;
            m_gameOverInfoController = 1;
            m_timeUpPanel.SetActive(false);
            m_interactivePanel.SetActive(true);
            StartCoroutine(DisplayLivesSaved());
        }

        return;
    }

    IEnumerator DisplayLivesSaved()
    {
        int livesSavedCounter = 0;
        while (livesSavedCounter != m_playerStats.m_livesSaved)
        {
            livesSavedCounter++;

            if (m_audioSource.isPlaying)
                m_audioSource.Stop();

            m_audioSource.Play();

            m_livesCountUI_TXT.text = livesSavedCounter.ToString();

            yield return new WaitForSeconds(0.1f);
        }

        StartCoroutine(DisplayTimePlayed());
        yield return null;

    }

    IEnumerator DisplayTimePlayed()
    {
        StopCoroutine(DisplayLivesSaved());

        int timeCounter = 0;
        while (timeCounter < m_playerStats.m_totalPlaytime)
        {
            timeCounter++;

            if (m_audioSource.isPlaying)
                m_audioSource.Stop();

            m_audioSource.Play();

            m_timeCountUI_TXT.text = timeCounter.ToString();

            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(DisplayTopSpeed());
        yield return null;

    }

    IEnumerator DisplayTopSpeed()
    {
        StopCoroutine(DisplayTimePlayed());

        int speedCounter = 0;
        while (speedCounter < m_playerStats.m_topSpeed)
        {
            speedCounter++;

            if (m_audioSource.isPlaying)
                m_audioSource.Stop();

            m_audioSource.Play();

            m_speedCountUI_TXT.text = speedCounter.ToString();

            yield return new WaitForSeconds(0.01f);
        }

        m_gameOverInfoController = -1;
        yield return null;
    }

    //FOLLOWING FUNCTIONS CALLED FROM UI BUTTONS
    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
