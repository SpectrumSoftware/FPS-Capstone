using UnityEngine;
using System.Collections;

public class minimapScript : MonoBehaviour {

    public GameObject player;
    public Vector3 followPlayer;
    public int offset;
	
    void Update ()
    {
        followPlayer = new Vector3(player.transform.position.x, player.transform.position.y + offset, player.transform.position.z);
        transform.position = followPlayer;
    }

}
