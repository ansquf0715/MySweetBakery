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
    bool isRequestingBread = false;
    //bool hasRequestedBread = false;

    int maxBreadCount = 8;
    int currentBreadCount = 0;
    public List<GameObject> breads = new List<GameObject>();

    Transform breadSlot;
    float breadHeight = 0.2f;
    float currentTopHeight = 0f;

    Vector3 destination;

    public TextMeshProUGUI maxText;

    AudioSource audioSource;
    public AudioClip getBreadSound;

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

        maxText.gameObject.SetActive(false);

        //오븐 근처에 왔는지 전달받도록
        EventManager.OnPlayerNearOven += UpdateTriggerStatus;
        //빵을 주는지 알도록
        EventManager.OnPlayerReceiveBreads += ReceivedBread;
        //sell box가 빵을 달라고 요청하는지
        EventManager.OnSellBoxRequestBread += GiveBreadToSellBox;

        audioSource = GetComponent<AudioSource>();

        //arrowIsAvailable = FindObjectOfType<PlayerArrow>().arrowIsAvailable;
    }



    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
        //checkArrowAvailable();
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
    }

    void UpdateTriggerStatus(bool isNear)
    {
        isNearOven = isNear;

        if (isNearOven && !isRequestingBread)
        {
            StartCoroutine(RequestBread());
        }
        else if (!isNearOven)
        {
            StopCoroutine(RequestBread());
            isRequestingBread = false;
        }
    }

    IEnumerator RequestBread()
    {
        isRequestingBread = true;
        while (isNearOven && breads.Count < maxBreadCount)
        {
            EventManager.PlayerBreadRequest();
            yield return new WaitForSeconds(0.1f);
        }
        isRequestingBread = false;
    }

    void ReceivedBread(GameObject bread)
    {
        if (bread != null && breads.Count < maxBreadCount)
        {
            audioSource.PlayOneShot(getBreadSound);

            Rigidbody breadRb = bread.GetComponent<Rigidbody>();
            breadRb.isKinematic = true;
            breadRb.useGravity = false;

            Collider breadCol = bread.GetComponent<Collider>();
            breadCol.enabled = false;

            StartCoroutine(MoveBreadToStack(bread));
        }
    }

    IEnumerator MoveBreadToStack(GameObject bread)
    {
        Vector3 startPos = bread.transform.position;
        Vector3 endPos = breadSlot.position + new Vector3(0, breadHeight * breads.Count, 0);

        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bread.transform.position
                = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        bread.transform.parent = breadSlot;
        bread.transform.localPosition = new Vector3(0, breadHeight * breads.Count, 0);
        bread.transform.localRotation = Quaternion.Euler(0, 90, 0);
        breads.Add(bread);

        if (breads.Count >= maxBreadCount)
            ShowMaxText();
    }

    void ShowMaxText()
    {
        if (maxText != null)
        {
            maxText.gameObject.SetActive(true);

            // 플레이어의 월드 좌표를 화면 좌표로 변환
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(breadSlot.position + new Vector3(0, (breadHeight * breads.Count + 1), 0));

            RectTransform rectTransform = maxText.GetComponent<RectTransform>();
            rectTransform.position = screenPosition;
        }
    }

    void HideMaxText()
    {
        maxText.gameObject.SetActive(false);
    }

    void GiveBreadToSellBox()
    {
        if(breads.Count > 0)
        {
            GameObject bread = breads[breads.Count-1];
            breads.RemoveAt(breads.Count - 1);

            EventManager.PlayerGiveBreadToSellBox(bread);

            if(breads.Count < maxBreadCount)
                HideMaxText();
        }
        else
        {
            return;
        }
    }
}
