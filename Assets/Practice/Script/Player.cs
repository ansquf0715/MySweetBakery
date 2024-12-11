using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    Rigidbody rb;
    Animator anim;
    Vector3 m_Movement;

    public float moveSpeed = 5f;
    public float turnSpeed = 5f;

    bool isNearOven = false;
    bool hasRequestedBread = false;

    int maxBreadCount = 8;
    int currentBreadCount = 0;
    public List<GameObject> breads = new List<GameObject>();

    Transform breadSlot;
    float breadHeight = 0.2f;
    float currentTopHeight = 0f;

    Vector3 destination;

    public TextMeshProUGUI maxText;

    enum State
    {
        Idle,
        Move,
        StackIdle,
        StackMove,
    }
    State state;

    // Start is called before the first frame update
    void Start()
    {
        state = State.Idle;

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        breadSlot = transform.Find("BreadStackPoint");

        //GameObject maxTextObject = GameObject.Find("Canvas/PlayerMax");
        //maxText = maxTextObject.GetComponent<TextMeshPro>();
        maxText.gameObject.SetActive(false);

        //���� ��ó�� �Դ��� ���޹޵���
        EventManager.OnPlayerNearOven += nearOvenStatus;
        //���� �ִ��� �˵���
        EventManager.OnPlayerReceiveBreads += ReceiveBreads;

        //sell box�� ���� �޶�� ��û�ϴ���
        EventManager.OnSellBoxRequestBread += ReceiveSellBoxBreadRequest;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        anim.SetInteger("State", (int)state);
    }

    public void SetMoving(bool isMoving)
    {
        State previous = state;
        if(breads.Count > 0)
        {
            if (isMoving)
                state = State.StackMove;
            else
                state = State.StackIdle;
        }
        else
        {
            if (isMoving)
                state = State.Move;
            else
                state = State.Idle;
        }

        if(previous != state)
            UpdateAnimation();
        //UpdateAnimation();
    }

    void nearOvenStatus(bool isNear)
    {
        isNearOven = isNear;

        if(isNearOven && !hasRequestedBread)
        {
            StartCoroutine(RequestBread());
        }
        else if(!isNearOven)
        {
            hasRequestedBread = false;
        }
    }

    IEnumerator RequestBread()
    {
        int requestBreadCount = maxBreadCount - currentBreadCount;
        if(requestBreadCount > 0)
        {
            EventManager.PlayerBreadRequest(requestBreadCount);
            hasRequestedBread = true;
        }

        yield return null;
    }

    void ReceiveBreads(List<GameObject> receivedBreads)
    {
        int remainingSpace = maxBreadCount - breads.Count;
        Debug.Log(remainingSpace);
        if (remainingSpace <= 0)
        {
            Debug.Log("�ڸ� ����");
            return;
        }

        foreach(GameObject bread in receivedBreads)
        {
            if(breads.Count < maxBreadCount)
            {
                bread.transform.parent = breadSlot;
                bread.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                bread.transform.localPosition = new Vector3(0, breadHeight * breads.Count, 0);
                bread.transform.localEulerAngles = new Vector3(0, 90, 0);
                breads.Add(bread);
            }
            else
            {
                Debug.Log("�ڸ� �������");
                break;
            }
        }

        if (breads.Count >= maxBreadCount)
            ShowMaxText();
        
        //state = State.StackIdle;
        //UpdateAnimation();
        hasRequestedBread = false;
    }

    void ShowMaxText()
    {
        if (maxText != null)
        {
            maxText.gameObject.SetActive(true);

            // �÷��̾��� ���� ��ǥ�� ȭ�� ��ǥ�� ��ȯ
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(breadSlot.position + new Vector3(0, (breadHeight * breads.Count + 1), 0));

            RectTransform rectTransform = maxText.GetComponent<RectTransform>();
            rectTransform.position = screenPosition;
        }
    }

    void HideMaxText()
    {
        maxText.gameObject.SetActive(false);
    }

    void ReceiveSellBoxBreadRequest(int amount)
    {
        Debug.Log("sellbox���� " + amount);
        List<GameObject> breadsToGive = new List<GameObject>();

        int breadCountToGive;
        if(amount < breads.Count)
            breadCountToGive = amount;
        else
            breadCountToGive = breads.Count;

        for(int i=0; i<breadCountToGive; i++)
        {
            GameObject bread = breads[breads.Count - 1];
            breadsToGive.Add(bread);
            breads.RemoveAt(breads.Count - 1);
        }

        EventManager.PlayerGiveBreadToSellBox(breadsToGive);

        if (breads.Count < maxBreadCount)
            HideMaxText();
    }

}
