using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using System.Collections;

public static partial class Utility
{
    public static IEnumerator FadeToBlack( this CanvasGroup group, float fadeDurationSec )
    {
        while( group.alpha > 0.0f )
        {
            group.alpha = Mathf.Max( 0.0f, group.alpha - Time.deltaTime * ( 1.0f / fadeDurationSec ) );
            yield return null;
        }
    }

    public static IEnumerator FadeFromBlack( this CanvasGroup group, float fadeDurationSec )
    {
        while( group.alpha < 1.0f )
        {
            group.alpha = Mathf.Min( 1.0f, group.alpha + Time.deltaTime * ( 1.0f / fadeDurationSec ) );
            yield return null;
        }
    }

    public class DestroySelf : MonoBehaviour
    {
        public void DestroyMe()
        {
            Destroy( gameObject );
        }
    }

    // Helper function to get Modulus (not remainder which is what % gives)
    public static int Mod( int a, int b )
    {
        return ( ( a %= b ) < 0 ) ? a + b : a;
    }

    public static Vector2 Vector2FromAngle( float angleDegrees )
    {
        return new Vector2( Mathf.Cos( angleDegrees * Mathf.Deg2Rad ), Mathf.Sin( angleDegrees * Mathf.Deg2Rad ) );
    }

    public static IEnumerable<T> GetEnumValues<T>()
    {
        return Enum.GetValues( typeof( T ) ).Cast<T>();
    }

    public static T ParseEnum<T>( string value ) where T : struct
    {
        return ( T )Enum.Parse( typeof( T ), value );
    }

    public static bool TryParseEnum<T>( string value, out T result ) where T : struct
    {
        return Enum.TryParse( value, out result );
    }

    // Parse a float, return default if failed
    public static float ParseFloat( string text, float defaultValue )
    {
        if( float.TryParse( text, out float f ) )
            return f;
        return defaultValue;
    }

    // Parse a int, return default if failed
    public static int ParseInt( string text, int defaultValue )
    {
        if( int.TryParse( text, out int i ) )
            return i;
        return defaultValue;
    }

    public static float Distance( GameObject a, GameObject b ) { return Distance( a.transform, b.transform ); }
    public static float Distance( GameObject a, Transform b ) { return Distance( a.transform, b ); }
    public static float Distance( Transform a, GameObject b ) { return Distance( a, b.transform ); }
    public static float Distance( Transform a, Transform b ) { return Mathf.Sqrt( DistanceSq( a, b ) ); }
    public static float Distance( Vector3 a, Vector3 b ) { return Mathf.Sqrt( DistanceSq( a, b ) ); }
    public static float DistanceSq( GameObject a, GameObject b ) { return DistanceSq( a.transform, b.transform ); }
    public static float DistanceSq( GameObject a, Transform b ) { return DistanceSq( a.transform, b ); }
    public static float DistanceSq( Transform a, GameObject b ) { return DistanceSq( a, b.transform ); }
    public static float DistanceSq( Transform a, Transform b ) { return DistanceSq( a.position, b.position ); }
    public static float DistanceSq( Vector3 a, Vector3 b ) { return ( a - b ).sqrMagnitude; }

    public static void DrawCircle( Vector3 position, float diameter, float lineWidth, Color? colour = null )
    {
        colour = colour ?? new Color( 1.0f, 1.0f, 1.0f, 1.0f );
        var newObj = new GameObject();
        newObj.transform.position = position;

        var segments = 20;
        var line = newObj.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = segments + 1;
        line.startColor = line.endColor = colour.Value;

        var pointCount = segments + 1;
        var points = new Vector3[pointCount];

        for( int i = 0; i < pointCount; i++ )
        {
            var rad = Mathf.Deg2Rad * ( i * 360.0f / segments );
            points[i] = new Vector3( Mathf.Sin( rad ) * diameter / 2.0f, Mathf.Cos( rad ) * diameter / 2.0f, -0.1f );
        }

        line.SetPositions( points );

        FunctionTimer.CreateTimer( 5.0f, () => newObj.Destroy() );
    }

    public static void DrawRect( Rect rect, Color? colour = null )
    {
        colour = colour ?? new Color( 1.0f, 1.0f, 1.0f, 1.0f );
        Debug.DrawLine( rect.TopLeft(), rect.TopRight(), colour.Value );
        Debug.DrawLine( rect.TopRight(), rect.BottomRight(), colour.Value );
        Debug.DrawLine( rect.BottomRight(), rect.BottomLeft(), colour.Value );
        Debug.DrawLine( rect.BottomLeft(), rect.TopLeft(), colour.Value );
    }

    public class FunctionComponent : MonoBehaviour
    {
        public static void Create( GameObject obj, Action action )
        {
            var cmp = obj.AddComponent<FunctionComponent>();
            cmp.SetFunction( action );
        }

        public static void Create( GameObject obj, Func<bool> action )
        {
            var cmp = obj.AddComponent<FunctionComponent>();
            cmp.SetFunction( action );
        }

        public void SetFunction( Action action )
        {
            this.action = action;
        }

        public void SetFunction( Func<bool> action )
        {
            actionWithResult = action;
        }

        Action action;
        Func<bool> actionWithResult;

        void Update()
        {
            action?.Invoke();
            if( actionWithResult?.Invoke() ?? false )
                Destroy( this );
        }
    }

    public static GameObject CreateSprite( string path, Vector3 pos, Vector2 scale, Quaternion? rotation = null, string layer = "Default", int order = 1 )
    {
        var sprite = new GameObject();
        sprite.transform.position = pos;
        sprite.transform.rotation = rotation ?? Quaternion.identity;
        var spriteRenderer = sprite.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Resources.Load<Sprite>( path );
        spriteRenderer.sortingOrder = order;
        sprite.transform.localScale = scale.ToVector3( 1.0f );
        sprite.layer = LayerMask.NameToLayer( layer );
        return sprite;
    }
}

public class WeightedSelector< T >
{
    public WeightedSelector( Func<int, int, int> randomGenerator )
    {
        randomGeneratorPred = randomGenerator;
    }

    public WeightedSelector()
    {
        randomGeneratorPred = ( int min, int max ) => { return UnityEngine.Random.Range( min, max ); };
    }

    public void AddItem( T item, int weight )
    {
        if( weight <= 0 )
            return;
        total += weight;

        if( randomGeneratorPred( 0, total - 1 ) < weight )
            current = item;
    }

    public T GetResult()
    {
        return current;
    }

    public bool HasResult()
    {
        return total != 0;
    }

    private T current;
    private int total = 0;
    private Func<int, int, int> randomGeneratorPred;
}

public class Pair<T, U>
{
    public Pair()
    {
    }

    public Pair( T first, U second )
    {
        First = first;
        Second = second;
    }

    public static bool operator ==( Pair<T, U> lhs, Pair<T, U> rhs )
    {
        if( System.Object.ReferenceEquals( lhs, null ) )
            return System.Object.ReferenceEquals( rhs, null );
        return lhs.Equals( rhs );
    }

    public static bool operator !=( Pair<T, U> lhs, Pair<T, U> rhs )
    {
        return !( lhs == rhs );
    }

    public override bool Equals( object obj )
    {
        if( System.Object.ReferenceEquals( obj, null ) )
            return false;

        var rhs = obj as Pair<T, U>;
        return !System.Object.ReferenceEquals( rhs, null ) && Equals( rhs );
    }

    public bool Equals( Pair<T, U> obj )
    {
        if( System.Object.ReferenceEquals( obj, null ) )
            return false;

        return First.Equals( obj.First ) && Second.Equals( obj.Second );
    }

    public override int GetHashCode()
    {
        return ( 23 * First.GetHashCode() ) ^ ( 397 * Second.GetHashCode() );
    }

    public T First;
    public U Second;
}

#if UNITY_EDITOR
public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer( typeof( ReadOnlyAttribute ) )]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return EditorGUI.GetPropertyHeight( property, label, true );
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        GUI.enabled = false;
        EditorGUI.PropertyField( position, property, label, true );
        GUI.enabled = true;
    }
}
#endif