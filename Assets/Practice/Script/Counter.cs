using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public GameObject bagPrefab;

    Queue<Customer> waitingCustomers = new Queue<Customer>();
    bool playerIsCashing = false;

    GameObject bag;

    Collider boxCol;
    Collider circleCol;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.OnCustomerAtCounter += addCustomer;
        //bagAnim = bagPrefab.GetComponent<Animator>();

        boxCol = GetComponent<BoxCollider>();
        circleCol = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerIsCashing)
        {
            checkingOut();
        }
    }

    void addCustomer(Customer customer)
    {
        waitingCustomers.Enqueue(customer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //playerIsCashing=true;
            if (waitingCustomers.Count > 0)
            {
                playerIsCashing = true;
            }
            else
                playerIsCashing = false;
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
        if(waitingCustomers.Count > 0)
        {
            Customer customer = waitingCustomers.Dequeue();
            List<GameObject> breads = customer.GetBreads();

            bag = Instantiate(bagPrefab, new Vector3(0, 1.4f, 1.6f), Quaternion.identity);
            Animator bagAnim = bag.GetComponent<Animator>();
            bagAnim.SetBool("isOpen", true);

            StartCoroutine(MoveBreadsToBag(customer, breads));
            //StartCoroutine(CustomerCashingTime(customer, breads));
            StartCoroutine(WaitToProcessNextCustomer());
        }

        playerIsCashing = false;
    }

    IEnumerator WaitToProcessNextCustomer()
    {
        yield return new WaitForSeconds(2f);

        if (waitingCustomers.Count > 0)
            playerIsCashing = true;
    }

    IEnumerator MoveBreadsToBag(Customer customer, List<GameObject> breads)
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
            customer.RemoveBread(bread);

            // �� ����
            Destroy(bread);
        }

        Animator bagAnim = bag.GetComponent<Animator>();
        bagAnim.SetBool("isOpen", false);
        //StartCoroutine(GiveCustomerBag(customer));

        //EventManager.BagReady(bag);
        customer.GetBag(bag);
        bag = null;
    }

}
