using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<bool> OnPlayerNearOven; //player가 oven 근처라는 이벤트
    public static event Action OnPlayerBreadRequest; //player가 빵을 달라고 요청하는 이벤트
    public static event Action<GameObject> OnPlayerReceiveBreads; //player가 빵을 받는 이벤트

    public static event Action OnSellBoxRequestBread; //sellbox가 player한테 빵 달라고 요청
    public static event Action<GameObject> OnPlayerGiveBreadToSellBox; //player가 sellbox에 빵을 줌

    public static event Action<Customer> OnCustomerAtCounter; // customerManager가 들어온 customer가 있다고 전달
    //public static event Action<GameObject> OnBagReady; //계산이 완료된 빵
    public static event Action<Customer, int> OnCustomerPay; //고객이 돈을 지불했다는 이벤트

    public static event Action OnFirstQuestIsReady; //첫번째 quest를 생성할 준비가 됨

    public static event Action<GameObject> OnNewSeatAvailable; //seat이 생성됐을 때

    public static event Action<Seat> OnSeatDirty; //seat가 더럽다는 이벤트
    public static event Action<Seat> OnSeatCleaned; //seat가 청소됐다는 이벤트

    public static Action<int> OnArrowAction; //화살표 위치를 지정하기 위해

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

    public static void CustomerAtCounter(Customer customer)
    {
        OnCustomerAtCounter?.Invoke(customer);
    }

    public static void CustomerPay(Customer customer, int amount)
    {
        OnCustomerPay?.Invoke(customer, amount);
    }

    public static void FirstQuestIsReady()
    {
        OnFirstQuestIsReady?.Invoke();
    }

    public static void NewSeatAvailable(GameObject chair)
    {
        OnNewSeatAvailable?.Invoke(chair);
    }

    public static void SeatDirty(Seat seat)
    {
        OnSeatDirty?.Invoke(seat);
    }

    public static void SeatCleaned(Seat seat)
    {
        OnSeatCleaned?.Invoke(seat);
    }

    public static void ArrowAction(int arrowId)
    {
        //Debug.Log("arrowId" + arrowId);
        OnArrowAction?.Invoke(arrowId);
    }
}
