using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public Transform player;
    //public GameObject player;
    Player playerScript;

    Canvas canvas;
    public GameObject joystick;
    GameObject lever;

    [SerializeField]
    Vector3 stickFirstPos;
    Vector3 joyVec;
    float Radius;
    bool MoveFlag;

    float playerMoveSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        playerScript = player.GetComponent<Player>();

        canvas = FindObjectOfType<Canvas>();
        lever = joystick.transform.Find("lever").gameObject;

        float baseRadius = joystick.GetComponent<RectTransform>().sizeDelta.y * 0.5f;

        float can = canvas.GetComponent<RectTransform>().localScale.x;
        Radius = baseRadius * can * 2f;

        MoveFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (MoveFlag)
            player.transform.Translate(Vector3.forward * Time.deltaTime * playerMoveSpeed);
    }

    public void PointerDown(BaseEventData eventData)
    {
        PointerEventData Data = eventData as PointerEventData;
        Vector3 clickPos = Data.position;

        joystick.SetActive(true);
        stickFirstPos = clickPos;
        joystick.transform.position = clickPos;

        MoveFlag = true;
        playerScript.SetMoving(true);
    }

    public void Drag(BaseEventData baseEventData)
    {
        PointerEventData Data = baseEventData as PointerEventData;
        Vector3 pos = Data.position;

        joyVec = (pos - stickFirstPos).normalized;

        float dis = Vector3.Distance(pos, stickFirstPos);

        if (dis < Radius)
            lever.transform.position = stickFirstPos + joyVec * dis;
        else
            lever.transform.position = stickFirstPos + joyVec * Radius;

        player.eulerAngles = new Vector3(0,
            Mathf.Atan2(joyVec.x, joyVec.y) * Mathf.Rad2Deg - 90, 0);
    }

    public void DragEnd()
    {
        joystick.transform.position = stickFirstPos;
        joyVec = Vector3.zero;
        MoveFlag = false;

        playerScript.SetMoving(false);
        joystick.SetActive(false);
    }

}
