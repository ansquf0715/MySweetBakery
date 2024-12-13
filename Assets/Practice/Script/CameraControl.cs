using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform player;
    Vector3 followOffset = new Vector3(10, 7.5f, 0);
    public float smoothSpeed = 0.125f;

    bool isFollowingPlayer = true;
    bool isFocusingOnQuest = false;
    Vector3 questPos;
    float focusTime = 2f;

    // Start is called before the first frame update
    void Start()
    {
        //EventManager.OnQuestIsAvailable += handleQuestAvailable;
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
        Vector3 desiredPosition = player.position + followOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(player);
    }

    public void handleQuestAvailable(Vector3 pos)
    {
        Debug.Log("isfocusing" + isFocusingOnQuest);
        Debug.Log("이건 왜 호출이 안돼?");
        questPos = pos;
        isFocusingOnQuest = true;
    }

    void showQuest()
    {
        Vector3 targetPos = questPos + followOffset;
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
