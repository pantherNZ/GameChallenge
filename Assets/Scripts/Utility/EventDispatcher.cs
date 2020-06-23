using System;
using UnityEngine;

public class EventDispatcher : MonoBehaviour
{
    public Action<Collider> OnTriggerEnterEvent;
    public Action<Collider2D> OnTriggerEnter2DEvent;
    public Action<Collider> OnTriggerExitEvent;
    public Action<Collider2D> OnTriggerExit2DEvent;

    private void OnTriggerEnter2D( Collider2D collision )
    {
        OnTriggerEnter2DEvent?.Invoke( collision );
    }

    private void OnTriggerExit2D( Collider2D collision )
    {
        OnTriggerExit2DEvent?.Invoke( collision );
    }

    private void OnTriggerEnter( Collider collision )
    {
        OnTriggerEnterEvent?.Invoke( collision );
    }

    private void OnTriggerExit( Collider collision )
    {
        OnTriggerExitEvent?.Invoke( collision );
    }
}
