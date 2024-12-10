using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnRequestBread; //�� ������ �̺�Ʈ

    public static event Action<GameObject> OnBreadBaked; //���� �������� �� ������ �̺�Ʈ


    public static void RequestBread()
    {
        OnRequestBread?.Invoke();
    }

    public static void BreadBaked(GameObject bread)
    {
        Debug.Log("�� ��������");
        OnBreadBaked?.Invoke(bread);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
