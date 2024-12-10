using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform player;
    Vector3 followOffset = new Vector3(10, 7.5f, 0);
    public float smoothSpeed = 0.125f;

    bool isFollowingPlayer = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isFollowingPlayer)
            FollowPlayer();
    }

    void FollowPlayer()
    {
        Vector3 desiredPosition = player.position + followOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(player);
    }
}
