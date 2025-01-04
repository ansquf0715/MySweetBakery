using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform player;
    Vector3 PlayerfollowOffset = new Vector3(6.5f, 7.9f, 0);
    Vector3 followOffset = new Vector3(10, 7.5f, 0);
    public float smoothSpeed = 0.125f;

    bool isFollowingPlayer = true;
    bool isFocusingOnQuest = false;
    Vector3 questPos;
    float focusTime = 2f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isFollowingPlayer && !isFocusingOnQuest)
            FollowPlayer();
        else if (isFocusingOnQuest)
            showQuest();
    }

    void FollowPlayer()
    {
        Vector3 desiredPosition = player.position + PlayerfollowOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(player);
    }

    public void handleQuestAvailable(Vector3 pos)
    {
        questPos = pos;
        isFocusingOnQuest = true;
    }

    void showQuest()
    {
        Vector3 targetPos = questPos + PlayerfollowOffset;
        targetPos.x -= 5f;
        Vector3 smoothedPos = Vector3.Lerp(transform.position,
            targetPos, smoothSpeed);
        transform.position = smoothedPos;
        transform.LookAt(questPos);

        focusTime -= Time.deltaTime;
        if(focusTime <= 0)
        {
            isFocusingOnQuest = false;
            focusTime = 2f;
        }
    }
}
