using System.Collections;
using System.Linq;
using UnityEngine;

public static partial class Utility
{
    public static IEnumerator FadeToBlack( CanvasGroup group, float fadeDurationSec )
    {
        while( group != null && group.alpha > 0.0f )
        {
            group.alpha = Mathf.Max( 0.0f, group.alpha - Time.deltaTime * ( 1.0f / fadeDurationSec ) );
            yield return null;
        }
    }

    public static void FadeToBlack( this MonoBehaviour mono, float fadeDurationSec )
    {
        mono.StartCoroutine( FadeToBlack( mono.GetComponent<CanvasGroup>(), fadeDurationSec ) );
    }

    public static IEnumerator FadeFromBlack( CanvasGroup group, float fadeDurationSec )
    {
        while( group != null && group.alpha < 1.0f )
        {
            group.alpha = Mathf.Min( 1.0f, group.alpha + Time.deltaTime * ( 1.0f / fadeDurationSec ) );
            yield return null;
        }
    }

    public static void FadeFromBlack( this MonoBehaviour mono, float fadeDurationSec )
    {
        mono.StartCoroutine( FadeFromBlack( mono.GetComponent<CanvasGroup>(), fadeDurationSec ) );
    }

    public static IEnumerator InterpolateScale( Transform transform, Vector3 targetScale, float durationSec )
    {
        var interp = targetScale - transform.localScale;

        while( transform != null && ( targetScale - transform.localScale ).sqrMagnitude > 0.01f )
        {
            var diff = targetScale - transform.localScale;
            var delta = Time.deltaTime * ( 1.0f / durationSec );
            transform.localScale = new Vector3( 
                transform.localScale.x + Mathf.Min( Mathf.Abs( diff.x ), Mathf.Abs( interp.x ) * delta ) * Mathf.Sign( diff.x ),
                transform.localScale.y + Mathf.Min( Mathf.Abs( diff.y ), Mathf.Abs( interp.y ) * delta ) * Mathf.Sign( diff.y ),
                transform.localScale.z + Mathf.Min( Mathf.Abs( diff.z ), Mathf.Abs( interp.z ) * delta ) * Mathf.Sign( diff.z ) );
            yield return null;
        }
    }

    public static void InterpolateScale( this MonoBehaviour mono, Vector3 targetScale, float durationSec )
    {
        mono.StartCoroutine( InterpolateScale( mono.transform, targetScale, durationSec ) );
    }

    public static IEnumerator InterpolatePosition( Transform transform, Vector3 targetPosition, float durationSec )
    {
        var interp = targetPosition - transform.position;

        while( transform != null && ( targetPosition - transform.position ).sqrMagnitude > 0.01f )
        {
            var diff = targetPosition - transform.position;
            var delta = Time.deltaTime * ( 1.0f / durationSec );
            transform.position = new Vector3(
                transform.position.x + Mathf.Min( Mathf.Abs( diff.x ), Mathf.Abs( interp.x ) * delta ) * Mathf.Sign( diff.x ),
                transform.position.y + Mathf.Min( Mathf.Abs( diff.y ), Mathf.Abs( interp.y ) * delta ) * Mathf.Sign( diff.y ),
                transform.position.z + Mathf.Min( Mathf.Abs( diff.z ), Mathf.Abs( interp.z ) * delta ) * Mathf.Sign( diff.z ) );
            yield return null;
        }
    }

    public static void InterpolatePosition( this MonoBehaviour mono, Vector3 targetPosition, float durationSec )
    {
        mono.StartCoroutine( InterpolatePosition( mono.transform, targetPosition, durationSec ) );
    }
}