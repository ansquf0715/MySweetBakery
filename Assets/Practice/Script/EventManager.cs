using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnRequestBake; //빵 구우라는 이벤트
    public static event Action<GameObject> OnBreadBaked; //빵이 구워졌을 때 들어오는 이벤트

    public static event Action<bool> OnPlayerNearOven; //player가 oven 근처라는 이벤트
    public static event Action OnPlayerBreadRequest; //player가 빵을 달라고 요청하는 이벤트
    public static event Action<GameObject> OnPlayerReceiveBreads; //player가 빵을 받는 이벤트

    public static event Action OnSellBoxRequestBread; //sellbox가 player한테 빵 달라고 요청
    public static event Action<GameObject> OnPlayerGiveBreadToSellBox; //player가 sellbox에 빵을 줌

    public static event Action<int> OnCustomerRequestToSellBox; //customer가 sellbox에서 빵 달라고 요청
    public static event Action OnSellboxHaveNotEnoughBread; //sellbox에 충분한 빵이 없을 때
    public static event Action<List<GameObject>> OnCustomerReceiveBreads; //customer가 sellbox에서 빵을 가져감
    
    public static void RequestBread()
    {
        OnRequestBake?.Invoke();
    }

    public static void BreadBaked(GameObject bread)
    {
        OnBreadBaked?.Invoke(bread);
    }

    public static void SetPlayerNearOven(bool isNear)
    {
        OnPlayerNearOven?.Invoke(isNear);
    }

    public static void PlayerBreadRequest()
    {
        OnPlayerBreadRequest?.Invoke();
    }

    public static void DeliverBreadToPlayer(GameObject bread)
    {
        OnPlayerReceiveBreads?.Invoke(bread);
    }
    
    public static void SellBoxRequestBread()
    {
        OnSellBoxRequestBread?.Invoke();
    }

    public static void PlayerGiveBreadToSellBox(GameObject bread)
    {
        OnPlayerGiveBreadToSellBox?.Invoke(bread);
    }

    public static void CustomerRequestToSellBox(int count)
    {
        OnCustomerRequestToSellBox?.Invoke(count);
    }

    public static void SellboxHaveNotEnoughBread()
    {
        OnSellboxHaveNotEnoughBread?.Invoke();
    }

    public static void CustomerReceiveBreads(List<GameObject> breads)
    {
        OnCustomerReceiveBreads?.Invoke(breads);
    }
}
