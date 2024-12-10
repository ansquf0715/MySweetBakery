using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyStick : MonoBehaviour//, IBeginDragHandler, IDragHandler, IPointerUpHandler
{
    //public delegate void DirectionChangedHandler(Vector2 direction);
    //public event DirectionChangedHandler onDirectionChanged;

    Vector2 direction;
    bool isActivate = false;
    Player player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    isActivate = true;
    //    transform.position = eventData.position;
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    if(isActivate)
    //    {
    //        Vector2 touchPos = eventData.position;
    //        Vector2 joystickCenter = new Vector2(transform.position.x, transform.position.y);
    //        direction = (touchPos - joystickCenter).normalized;
    //    }

    //    if(player != null)
    //    {
    //        player.MoveInDirection(direction);
    //    }
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    isActivate = false;
    //    if(player != null)
    //    {
    //        player.StopMovement();
    //    }
    //}

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    Debug.Log("On Begin drag");
    //    isActivate = true;
    //    transform.position = eventData.position;
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    Debug.Log("On Drag");
    //    if(isActivate)
    //    {
    //        Vector2 touchPos = eventData.position;
    //        Vector2 joystickCenter = new Vector2(transform.position.x, transform.position.y);
    //        direction = (touchPos - joystickCenter).normalized;

    //        // 플레이어에게 방향 전달
    //        if (player != null)
    //        {
    //            player.UpdateRotation(direction);
    //        }
    //    }
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    isActivate = false;
    //    if (player != null)
    //    {
    //        player.StopMovement();
    //    }
    //}
}
