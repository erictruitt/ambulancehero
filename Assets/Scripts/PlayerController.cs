using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}

[System.Serializable]
public class PlayerStatistics
{
    public int m_livesSaved = 0;
    public int m_livesLost = 0;
    public int m_totalPlaytime = 0;
    public int m_topSpeed = 0;
    public float m_avgSpeed = 0;

    private float m_startTime = 0;
    private List<float> m_speedTracker = new List<float>();

    public void SetStartTime(float _time) { m_startTime = _time; }
    public void TrackSpeed(float _speed) { m_speedTracker.Add(_speed); }

    public void CalculateTopSpeed()
    {
        float tempTopSpeed = 0.0f;

        for (int i = 0; i < m_speedTracker.Count - 1; i++)
            if (m_speedTracker[i] > tempTopSpeed)
                tempTopSpeed = m_speedTracker[i];

        m_topSpeed = (int)tempTopSpeed;
    }

    public float CalculateAvgSpeed()
    {
        float avgSpeed = 0.0f, totSpeed = 0.0f;

        for (int i = 0; i < m_speedTracker.Count - 1; i++)
            totSpeed += m_speedTracker[i];

        avgSpeed = totSpeed / m_speedTracker.Count;

        return avgSpeed;
    }

    public void CalculateTotalPlaytime()
    {
        m_totalPlaytime = Mathf.FloorToInt(Time.time - m_startTime) - 1;
    }
}

public class PlayerController : MonoBehaviour
{
    private const float MAX_SPEED = 100.0f;

    public List<AxleInfo> m_axleInfos;
    public PlayerStatistics m_playerStats;

    public float m_maxMotorTorque;
    public float m_currSpeed;
    public float m_maxSteeringAngle;

    public Text m_speedUI;
    public Text m_livesSavedUI;


    public Transform m_centerofMass;

    private float m_currentTorque;
    private AudioSource m_audioSource;
    private bool m_needsPatient;
    private bool m_carryingPatient;
    private bool m_needToResetBrake;
    private bool m_braking;

    public GameObject m_wayfindingIcon;
    public GameObject m_brakeLights;

    public int m_currPatientRank = 0;

    private void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = m_centerofMass.localPosition;
        m_audioSource = GetComponent<AudioSource>();
        m_needsPatient = true;
        m_carryingPatient = false;
        m_needToResetBrake = false;

        m_playerStats = new PlayerStatistics();
        m_playerStats.SetStartTime(Time.time);
        m_braking = false;

        m_brakeLights.SetActive(false);
    }

    private void OnDisable()
    {
        m_audioSource.pitch = 1.0f;

        foreach (AxleInfo axleInfo in m_axleInfos)
        {
            axleInfo.leftWheel.motorTorque = 0.0f;
            axleInfo.rightWheel.motorTorque = 0.0f;

            axleInfo.leftWheel.brakeTorque = 1000.0f;
            axleInfo.rightWheel.brakeTorque = 1000.0f;
        }

    }

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        //if (collider.transform.childCount == 0)
        //{
        //    Debug.Log(collider.transform.localPosition);
        //    return;
        //}

        //Transform visualWheel = collider.transform.GetChild(0);

        //Vector3 position;
        //Quaternion rotation;
        //collider.GetWorldPose(out position, out rotation);


        //transform.localRotation = collider.transform.localRotation;
        //Debug.Log(position);

        //visualWheel.transform.position = position;
        //visualWheel.transform.rotation = rotation;
    } //CURRENTLY DISABLED

    void EngineSound()
    {
        m_audioSource.pitch = m_currSpeed / MAX_SPEED + 1.0f;

        if (m_audioSource.pitch < 1.0f)
            m_audioSource.pitch = 1.0f;

    }

    void UpdateHUD()
    {
        int toText = (int)m_currSpeed;
        m_speedUI.text = toText.ToString();

        m_livesSavedUI.text = m_playerStats.m_livesSaved.ToString();
    }

    void CheckPatient()
    {
        if (m_needsPatient)
        {
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().ActivatePatients();
            m_needsPatient = false;
        }

    }

    void CheckBreak()
    {
        if (Input.GetButton("Brake"))
        {
            m_brakeLights.SetActive(true);

            for (int i = 0; i < m_axleInfos.Count; i++)
            {
                m_axleInfos[i].leftWheel.brakeTorque += 50.0f;
                m_axleInfos[i].rightWheel.brakeTorque += 50.0f;
                m_needToResetBrake = true;
            }

            m_braking = true;
        }

        if (Input.GetButtonUp("Brake") && m_needToResetBrake)
        {
            m_brakeLights.SetActive(false);

            for (int i = 0; i < m_axleInfos.Count; i++)
            {
                m_axleInfos[i].leftWheel.brakeTorque = 0.0f;
                m_axleInfos[i].rightWheel.brakeTorque = 0.0f;
                m_needToResetBrake = false;
            }

            m_braking = false;
        }
    }

    private void Update()
    {
        EngineSound();
        UpdateHUD();
        CheckPatient();
        CheckBreak();
    }



    public void FixedUpdate()
    {

        float steering = m_maxSteeringAngle * Input.GetAxis("Horizontal");
        float accelerator = Input.GetAxis("Vertical");

        m_currentTorque = m_maxMotorTorque * accelerator;
        m_currSpeed = GetComponent<Rigidbody>().velocity.magnitude * 2.237f;
        m_playerStats.TrackSpeed(m_currSpeed);

        //handles applying steering while braking
        if (m_braking)
        {
            foreach (AxleInfo axleInfo in m_axleInfos)
            {
                if (axleInfo.steering)
                {
                    axleInfo.leftWheel.steerAngle = steering;
                    axleInfo.rightWheel.steerAngle = steering;
                }

                axleInfo.leftWheel.motorTorque = 0.0f;
                axleInfo.rightWheel.motorTorque = 0.0f;
            }
            return;
        }

        //handles applying steering and acceleration
        foreach (AxleInfo axleInfo in m_axleInfos)
        {

            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = m_currentTorque;
                axleInfo.rightWheel.motorTorque = m_currentTorque;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);

            //slow down gradually when accelerator isn't pressed
            if (m_currentTorque == 0)
            {
                axleInfo.leftWheel.brakeTorque += 0.1f;
                axleInfo.rightWheel.brakeTorque += 0.1f;
            }
            else
            {
                axleInfo.leftWheel.brakeTorque = 0.0f;
                axleInfo.rightWheel.brakeTorque = 0.0f;
            }

        }

//END FIXED UPDATE
    }

    public void DropOffSuccess()
    {
        m_needsPatient = true;
        m_playerStats.m_livesSaved += 1;
        m_carryingPatient = false;
        m_wayfindingIcon.SetActive(false);
    }

    public void DropOffFail()
    {
        m_needsPatient = true;
        m_playerStats.m_livesLost += 1;
        m_carryingPatient = false;
        m_wayfindingIcon.SetActive(false);
    }

    public void PickedUpPatient(int _rank)
    {
        m_carryingPatient = true;
        m_wayfindingIcon.SetActive(true);
        m_currPatientRank = _rank;
    }

    public bool GetCarryingPatient()
    {
        return m_carryingPatient;
    }

    public int GetCurrPatientRank()
    {
        return m_currPatientRank;
    }

    public PlayerStatistics GetPlayerStats()
    {
        m_playerStats.CalculateTotalPlaytime();
        m_playerStats.CalculateTopSpeed();
        this.enabled = false;
        return m_playerStats;
    }

}
