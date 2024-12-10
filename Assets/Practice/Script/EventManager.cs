using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnRequestBread; //빵 구우라는 이벤트

    public static event Action<GameObject> OnBreadBaked; //빵이 구워졌을 때 들어오는 이벤트


    public static void RequestBread()
    {
        OnRequestBread?.Invoke();
    }

    public static void BreadBaked(GameObject bread)
    {
        Debug.Log("빵 구워졌음");
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
