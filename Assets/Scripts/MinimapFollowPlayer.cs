using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFollowPlayer : MonoBehaviour {

    public Transform m_player;

	void Update () {
        transform.position = new Vector3(m_player.position.x, transform.position.y, m_player.position.z);
	}
}
