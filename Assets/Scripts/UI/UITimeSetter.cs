using System;
using UnityEngine;
using UnityEngine.UI;

public class UITimeSetter : MonoBehaviour
{
    [SerializeField] Button[] buttons = new Button[6];
    [SerializeField] Text text = null;
    public event Action<UITimeSetter, TimeSpan, TimeSpan> OnTimeChangedEvent;
    private TimeSpan time;
    public TimeSpan Time
    {
        get { return time; }
        set { SetTime( value ); }
    }

    private void Start()
    {
        SetTime( DateTime.Now.TimeOfDay );

        buttons[0].onClick.AddListener( () => { SetTime( TimeSpan.FromHours( time.TotalHours - time.Hours % 12 + ( ( time.Hours + 1 ) % 12 ) ) ); } );
        buttons[1].onClick.AddListener( () => { SetTime( TimeSpan.FromHours( time.TotalHours - time.Hours % 12 + ( ( time.Hours + 23 ) % 12 ) ) ); } );
        buttons[2].onClick.AddListener( () => { SetTime( TimeSpan.FromMinutes( time.TotalMinutes - time.Minutes + ( ( time.Minutes + 1 ) % 60 ) ) ); } );
        buttons[3].onClick.AddListener( () => { SetTime( TimeSpan.FromMinutes( time.TotalMinutes - time.Minutes + ( ( time.Minutes + 59 ) % 60 ) ) ); } );
        buttons[4].onClick.AddListener( () => { SetTime( TimeSpan.FromHours( time.TotalHours - time.Hours + ( ( time.Hours + 12 ) % 24 ) ) ); } );
        buttons[5].onClick.AddListener( () => { SetTime( TimeSpan.FromHours( time.TotalHours - time.Hours + ( ( time.Hours + 12 ) % 24 ) ) ); } );
    }

    public void SetTime( TimeSpan t )
    {
        var oldTime = time;
        time = t;
        //text.text = time.ToString( "hh:mm" );
        text.text = new DateTime().Add( time ).ToString( "hh:mm tt", System.Globalization.CultureInfo.CreateSpecificCulture( "en-US" ) );
        OnTimeChangedEvent?.Invoke( this, oldTime, time );

        //var amAlpha = time.TotalHours < 12 ? 0.0f : 1.0f;
        //var spriteRendererAm = buttons[4].GetComponent<Image>();
        //spriteRendererAm.color = spriteRendererAm.color.SetA( amAlpha );
        //
        //var spriteRendererPm = buttons[5].GetComponent<Image>();
        //spriteRendererPm.color = spriteRendererPm.color.SetA( 1.0f - amAlpha );
    }
}
