using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public GameObject bagPrefab;
    public AudioClip cashSound;
    AudioSource audioSource;

    //Queue<Customer> waitingCustomers = new Queue<Customer>();
    Customer cashingCustomer;
    bool playerIsCashing = false;
    bool alreadyCashedCustomer = false;

    GameObject bag;

    Collider boxCol;
    Collider circleCol;

    // Start is called before the first frame update
    void Start()
    {
        //EventManager.OnCustomerAtCounter += addCustomer;

        boxCol = GetComponent<BoxCollider>();
        circleCol = GetComponent<SphereCollider>();

        audioSource = GetComponent<AudioSource>();

        //EventManager.OnNextCustomerArrivedAtCounterPosition += handleCustomerArrival;
    }

    // Update is called once per frame
    void Update()
    {
        //if(playerIsCashing && cashingCustomer!=null)
        //{
        //    checkingOut();
        //}

        if(playerIsCashing && cashingCustomer != null && !alreadyCashedCustomer)
        {
            if(cashingCustomer.currentState is CheckOutState)
            {
                Debug.Log("�̰� ��� ��");
                checkingOut();
                alreadyCashedCustomer = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EventManager.OnArrowAction(3);
            playerIsCashing = true;
        }
        else if(other.gameObject.CompareTag("Customer"))
        {
            cashingCustomer = other.gameObject.GetComponent<Customer>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            playerIsCashing = false;
        }
    }

    void checkingOut()
    {
        Debug.Log("checking out");
        audioSource.PlayOneShot(cashSound);

        alreadyCashedCustomer = true;

        List<GameObject> breads = cashingCustomer.GetBreads();

        bag = Instantiate(bagPrefab,
            new Vector3(0, 1.4f, 1.6f), Quaternion.identity);
        Animator bagAnim = bag.GetComponent<Animator>();
        bagAnim.SetBool("isOpen", true);

        StartCoroutine(MoveBreadsToBag(breads));
    }

    IEnumerator MoveBreadsToBag(List<GameObject> breads)
    {
        //Vector3 lastBreadPos = Vector3.zero;

        while (breads.Count > 0)
        {
            // ���� ������ ���� ��������
            GameObject bread = breads[breads.Count - 1];
            bread.transform.rotation = Quaternion.Euler(0, 90f, 0);

            Vector3 startPos = bread.transform.position;
            Vector3 targetPos = bag.transform.position;

            //lastBreadPos = startPos;

            float elapsedTime = 0f;
            float duration = 0.2f;

            // 1�� ���� ���� �ε巴�� �̵�
            while (elapsedTime < duration)
            {
                bread.transform.position = Vector3.Lerp(startPos,
                    targetPos, (elapsedTime / duration));

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // ���� ��Ȯ�� ��ġ�� ����
            bread.transform.position = targetPos;

            // ������ ���� ����
            //customer.RemoveBread(bread);
            cashingCustomer.RemoveBread(bread);

            // �� ����
            Destroy(bread);
        }

        Animator bagAnim = bag.GetComponent<Animator>();
        bagAnim.SetBool("isOpen", false);

        cashingCustomer.GetBag(bag);

        StartCoroutine(delayCheckingOut());
    }

    IEnumerator delayCheckingOut()
    {
        yield return new WaitForSeconds(2f);


        //Debug.Log("delay checking out");
        cashingCustomer.SetCustomerCheckOutEnd();
        bag = null;
        cashingCustomer = null;
        alreadyCashedCustomer = false;
    }

}
