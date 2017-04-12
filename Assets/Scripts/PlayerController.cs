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
    public int livesSaved = 0;
    public int livesLost = 0;
    public float totalPlaytime = 0;
    public float topSpeed = 0;
    public float avgSpeed = 0;

    private float startTime = 0;
    private List<float> speedTracker = new List<float>();

    public void SetStartTime(float _time) { startTime = _time; }
    public void TrackSpeed(float _speed) { speedTracker.Add(_speed); }

    public float CalculateTopSpeed()
    {
        float topSpeed = 0.0f;

        for (int i = 0; i < speedTracker.Count - 1; i++)
            if (speedTracker[i] > topSpeed)
                topSpeed = speedTracker[i];

        return topSpeed;
    }

    public float CalculateAvgSpeed()
    {
        float avgSpeed = 0.0f, temp = 0.0f;

        for (int i = 0; i < speedTracker.Count - 1; i++)
            temp += speedTracker[i];

        avgSpeed = temp / speedTracker.Count;

        return avgSpeed;
    }

    public void CalculateTotalPlaytime()
    {
        totalPlaytime = Time.time - startTime;
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


    public Transform m_centerofMass;

    private float accelerator;
    private float m_currentTorque;
    private AudioSource m_audioSource;
    private bool m_needsPatient;
    private bool m_carryingPatient;

    private void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = m_centerofMass.localPosition;
        m_audioSource = GetComponent<AudioSource>();
        m_needsPatient = true;
        m_carryingPatient = false;

        m_playerStats = new PlayerStatistics();
        m_playerStats.SetStartTime(Time.time);
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
    }

    void EngineSound()
    {
        m_audioSource.pitch = m_currSpeed / MAX_SPEED + 1.0f;

        if (m_audioSource.pitch < 1.0f)
            m_audioSource.pitch = 1.0f;

    }

    void UpdateHUD()
    {
        int speedToText = (int)m_currSpeed;
        m_speedUI.text = speedToText.ToString();

    }

    void CheckPatient()
    {
        if (m_needsPatient)
        {
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().ActivatePatients();
            m_needsPatient = false;
        }

    }


    private void Update()
    {
        EngineSound();
        UpdateHUD();
        CheckPatient();

    }

    public void FixedUpdate()
    {
        accelerator = Input.GetAxis("Vertical");
        m_currentTorque = m_maxMotorTorque * accelerator;

        float steering = m_maxSteeringAngle * Input.GetAxis("Horizontal");

        m_currSpeed = GetComponent<Rigidbody>().velocity.magnitude * 2.237f;
        m_playerStats.TrackSpeed(m_currSpeed);

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

    }

    public void DropOffSuccess()
    {
        m_needsPatient = true;
        m_playerStats.livesSaved += 1;
        m_carryingPatient = false;
    }

    public void DropOffFail()
    {
        m_needsPatient = true;
        m_playerStats.livesLost += 1;
        m_carryingPatient = false;
    }

    public void PickedUpPatient()
    {
        m_carryingPatient = true;
    }

    public bool GetCarryingPatient()
    {
        return m_carryingPatient;
    }

}
