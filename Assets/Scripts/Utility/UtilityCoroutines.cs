using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    public static IEnumerator FadeToColour( Graphic image, Color colour, float fadeDurationSec )
    {
        return FadeToColour( image, colour, fadeDurationSec, Lerp );
    }

    public static IEnumerator FadeToColour( Graphic image, Color colour, float fadeDurationSec, Func<float, float, float, float> interpolator )
    {
        Debug.Assert( image != null );
        if( image == null )
            yield break;

        var startColour = image.color;

        for( float interp = 0.0f; interp < 1.0f; interp += Time.deltaTime * ( 1.0f / fadeDurationSec ) )
        {
            image.color = InterpolateColour( startColour, colour, interp, interpolator );
            yield return null;
        }

        image.color = colour;
    }

    public static void FadeToColour( this MonoBehaviour mono, Color colour, float fadeDurationSec )
    {
        mono.StartCoroutine( FadeToColour( mono.GetComponent<Graphic>(), colour, fadeDurationSec ) );
    }

    public static void FadeToColour( this MonoBehaviour mono, Color colour, float fadeDurationSec, Func<float, float, float, float> interpolator )
    {
        mono.StartCoroutine( FadeToColour( mono.GetComponent<Graphic>(), colour, fadeDurationSec, interpolator ) );
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

    public static IEnumerator InterpolateAlongPath( Transform transform, PathCreation.PathCreator path, float durationSec )
    {
        float interp = 0.0f;
        var startPos = transform.position;

        while( interp < 1.0f && transform != null )
        {
            interp += Time.deltaTime / durationSec;
            transform.position = startPos + path.path.GetPointAtTime( interp, PathCreation.EndOfPathInstruction.Stop ) - path.path.GetPoint( 0 );
            transform.rotation = Quaternion.LookRotation( Vector3.forward, path.path.GetDirection( interp, PathCreation.EndOfPathInstruction.Stop ) );
            yield return null;
        }
    }

    public static void InterpolateAlongPath( this MonoBehaviour mono, PathCreation.PathCreator path, float durationSec )
    {
        mono.StartCoroutine( InterpolateAlongPath( mono.transform, path, durationSec ) );
    }

    public static IEnumerator Shake( Transform transform, float duration, float amplitudeStart, float amplitudeEnd, float frequency, float yMultiplier )
    {
        var elapsed = 0.0f;
        var originalPos = transform.localPosition;

        while( elapsed < duration )
        {
            elapsed += Time.deltaTime;

            var dynamicAmplitude = Mathf.Lerp( amplitudeStart, amplitudeEnd, elapsed / duration );
            transform.localPosition = originalPos + new Vector3(
                Mathf.Sin( elapsed * frequency ) * dynamicAmplitude,
                Mathf.Sin( elapsed * frequency * yMultiplier ) * dynamicAmplitude / yMultiplier, 
                0.0f );

            yield return null;
        }

        //yield return InterpolatePosition( transform, originalPos, 0.1f );

        transform.localPosition = originalPos;
    }

    public static void Shake( this MonoBehaviour mono, float duration, float amplitudeStart, float amplitudeEnd, float frequency, float yMultiplier )
    {
        mono.StartCoroutine( Shake( mono.transform, duration, amplitudeStart, amplitudeEnd, frequency, yMultiplier ) );
    }

    public static void ShakeTarget( this MonoBehaviour mono, Transform target, float duration, float amplitudeStart, float amplitudeEnd, float frequency, float yMultiplier )
    {
        mono.StartCoroutine( Shake( target, duration, amplitudeStart, amplitudeEnd, frequency, yMultiplier ) );
    }
}