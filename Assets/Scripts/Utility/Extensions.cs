using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static partial class Extensions
{
    public static void Destroy( this GameObject gameObject )
    {
        UnityEngine.Object.Destroy( gameObject );
    }

    public static void DestroyAll( this List<GameObject> objects )
    {
        foreach( var x in objects )
            x?.Destroy();
        objects.Clear();
    }

    public static void DestroyObject( this MonoBehaviour component )
    {
        UnityEngine.Object.Destroy( component.gameObject );
    }

    public static void DestroyComponent( this MonoBehaviour component )
    {
        UnityEngine.Object.Destroy( component );
    }

    public static void Resize<T>( this List<T> list, int size, T value = default )
    {
        int cur = list.Count;
        if( size < cur )
            list.RemoveRange( size, cur - size );
        else if( size > cur )
            list.AddRange( Enumerable.Repeat( value, size - cur ) );
    }

    public static Pair<U, V> FindPairFirst<U, V>( this List<Pair<U, V>> list, U item )
    {
        return list.Find( x => x.First.Equals( item ) );
    }

    public static Pair<U, V> FindPairSecond<U, V>( this List<Pair<U, V>> list, U item )
    {
        return list.Find( x => x.Second.Equals( item ) );
    }

    public static void RemoveBySwap<T>( this List<T> list, int index )
    {
        list[index] = list[list.Count - 1];
        list.RemoveAt( list.Count - 1 );
    }

    public static void RemovePairFirst<U, V>( this List<Pair<U, V>> list, U item )
    {
        RemoveBySwap( list, x => x.First.Equals( item ) );
    }

    public static void RemovePairSecond<U, V>( this List<Pair<U, V>> list, U item )
    {
        RemoveBySwap( list, x => x.Second.Equals( item ) );
    }

    public static bool RemoveBySwap<T>( this List<T> list, Func<T, bool> predicate )
    {
        if( list.IsEmpty() )
            return false;

        var end = list.Count;

        for( int i = 0; i < end; ++i )
        {
            if( predicate( list[i] ) )
            {
                if( i != end - 1 )
                    list[i] = list[end - 1];

                if( end > 0 )
                    end--;
            }
        }

        bool removed = end < list.Count;
        list.Resize( end );
        return removed;
    }

    public static bool IsVisible( this CanvasGroup group )
    {
        return group.alpha != 0.0f;
    }

    public static void ToggleVisibility( this CanvasGroup group )
    {
        group.SetVisibility( !group.IsVisible() );
    }

    public static void SetVisibility( this CanvasGroup group, bool visible )
    {
        group.alpha = visible ? 1.0f : 0.0f;
        group.blocksRaycasts = visible;
        group.interactable = visible;
    }

    // Deep clone
    public static T DeepCopy<T>( this T a )
    {
        using( MemoryStream stream = new MemoryStream() )
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize( stream, a );
            stream.Position = 0;
            return ( T )formatter.Deserialize( stream );
        }
    }

    public static List<T> Rotate<T>( this List<T> list, int offset )
    {
        if( offset == 0 )
            return list;
        offset = list.Count - Utility.Mod( offset, list.Count );
        offset = ( offset < 0 ? list.Count + offset : offset );
        return list.Skip( offset ).Concat( list.Take( offset ) ).ToList();
    }

    public static T PopFront<T>( this List<T> list )
    {
        if( list.IsEmpty() )
            throw new System.ArgumentException( "You cannot use PopFront on an empty list!" );

        var last = list[0];
        list.RemoveAt( 0 );
        return last;
    }

    public static T PopBack<T>( this List<T> list )
    {
        if( list.IsEmpty() )
            throw new System.ArgumentException( "You cannot use PopBack on an empty list!" );

        var last = list[list.Count - 1];
        list.RemoveAt( list.Count - 1 );
        return last;
    }

    public static T Front<T>( this List<T> list )
    {
        if( list.IsEmpty() )
            throw new System.ArgumentException( "You cannot use Front on an empty list!" );

        var first = list[0];
        return first;
    }

    public static T Back<T>( this List<T> list )
    {
        if( list.IsEmpty() )
            throw new System.ArgumentException( "You cannot use Back on an empty list!" );

        var last = list[list.Count - 1];
        return last;
    }

    public static bool IsEmpty<T>( this List<T> list )
    {
        return list.Count == 0;
    }

    public static bool IsEmpty<T>( this HashSet<T> list )
    {
        return list.Count == 0;
    }

    public static T RandomItem<T>( this List<T> list, T defaultValue = default )
    {
        if( list.IsEmpty() )
            return defaultValue;
        return list[UnityEngine.Random.Range( 0, list.Count )];
    }

    public static List<T> RandomShuffle<T>( this List<T> list )
    {
        for( int i = 0; i < list.Count; i++ )
        {
            T temp = list[i];
            int randomIndex = UnityEngine.Random.Range( i, list.Count );
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        return list;
    }

#if UNITY_EDITOR
    public static string GetDataPathAbsolute( this TextAsset textAsset )
    {
        return Application.dataPath.Substring( 0, Application.dataPath.Length - 6 ) + UnityEditor.AssetDatabase.GetAssetPath( textAsset );
    }

    public static string GetDataPathRelative( this TextAsset textAsset )
    {
        return UnityEditor.AssetDatabase.GetAssetPath( textAsset );
    }
#endif

    public static Vector2 SetX( this Vector2 vec, float x ) { vec.x = x; return vec; }
    public static Vector2 SetY( this Vector2 vec, float y ) { vec.y = y; return vec; }
    public static Vector3 SetX( this Vector3 vec, float x ) { vec.x = x; return vec; }
    public static Vector3 SetY( this Vector3 vec, float y ) { vec.y = y; return vec; }
    public static Vector3 SetZ( this Vector3 vec, float z ) { vec.z = z; return vec; }
    public static Vector4 SetX( this Vector4 vec, float x ) { vec.x = x; return vec; }
    public static Vector4 SetY( this Vector4 vec, float y ) { vec.y = y; return vec; }
    public static Vector4 SetZ( this Vector4 vec, float z ) { vec.z = z; return vec; }
    public static Vector4 SetW( this Vector4 vec, float w ) { vec.w = w; return vec; }
    public static Color SetR( this Color col, float r ) { col.r = r; return col; }
    public static Color SetG( this Color col, float g ) { col.g = g; return col; }
    public static Color SetB( this Color col, float b ) { col.b = b; return col; }
    public static Color SetA( this Color col, float a ) { col.a = a; return col; }

    public static Vector2 ToVector2( this Vector3 vec ) { return new Vector2( vec.x, vec.y ); }
    public static Vector2 ToVector2( this Vector4 vec ) { return new Vector2( vec.x, vec.y ); }
    public static Vector3 ToVector3( this Vector4 vec ) { return new Vector3( vec.x, vec.y, vec.z ); }
    public static Vector3 ToVector3( this Vector2 vec, float z = 0.0f ) { return new Vector3( vec.x, vec.y, z ); }
    public static Vector2 ToVector2( this Vector2Int vec ) { return new Vector2( vec.x, vec.y ); }

    public static Vector2Int ToVector2Int( this Vector2 vec, bool round = false )
    {
        if( round )
            return new Vector2Int( Mathf.RoundToInt( vec.x ), Mathf.RoundToInt( vec.y ) );
        return new Vector2Int( Mathf.FloorToInt( vec.x ), Mathf.FloorToInt( vec.y ) );
    }

    public static float Angle( this Vector2 vec )
    {
        if( vec.x < 0 )
        {
            return 360.0f - ( Mathf.Atan2( vec.x, vec.y ) * Mathf.Rad2Deg * -1.0f );
        }
        else
        {
            return Mathf.Atan2( vec.x, vec.y ) * Mathf.Rad2Deg;
        }
    }

    public static Vector2 Rotate( this Vector2 vec, float angleDegrees )
    {
        return Quaternion.AngleAxis( angleDegrees, Vector3.forward ) * vec;
    }

    public static Vector3 RotateX( this Vector3 vec, float angleDegrees )
    {
        return Quaternion.AngleAxis( angleDegrees, Vector3.right ) * vec;
    }

    public static Vector3 RotateY( this Vector3 vec, float angleDegrees )
    {
        return Quaternion.AngleAxis( angleDegrees, Vector3.up ) * vec;
    }

    public static Vector3 RotateZ( this Vector3 vec, float angleDegrees )
    {
        return Quaternion.AngleAxis( angleDegrees, Vector3.forward ) * vec;
    }

    public static Vector2 RandomPosition( this Rect rect )
    {
        return new Vector2(
            rect.x + UnityEngine.Random.value * rect.width,
            rect.y + UnityEngine.Random.value * rect.height );
    }

    public static bool Overlaps( this RectTransform rectTrans1, RectTransform rectTrans2 )
    {
        Rect rect1 = new Rect( rectTrans1.localPosition.x, rectTrans1.localPosition.y, rectTrans1.rect.width * rectTrans1.localScale.x, rectTrans1.rect.height * rectTrans1.localScale.y );
        Rect rect2 = new Rect( rectTrans2.localPosition.x, rectTrans2.localPosition.y, rectTrans2.rect.width * rectTrans1.localScale.x, rectTrans2.rect.height * rectTrans1.localScale.y );

        return rect1.Overlaps( rect2 );
    }

    public static IEnumerable<Tuple<int, T>> Enumerate<T>( this IEnumerable<T> collection )
    {
        int counter = 0;
        foreach( var item in collection )
        {
            yield return new Tuple<int, T>( counter, item );
            counter++;
        }
    }

    static public Rect GetWorldRect( this RectTransform rt )
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners( corners );
        Vector2 scaledSize = new Vector2( rt.lossyScale.x * rt.rect.size.x, rt.lossyScale.y * rt.rect.size.y );
        return new Rect( ( corners[1] + corners[3] ) / 2.0f, scaledSize );
    }

    static public Vector2 TopLeft( this Rect rect )
    {
        return new Vector2( rect.xMin, rect.yMax );
    }

    static public Vector2 TopRight( this Rect rect )
    {
        return new Vector2( rect.xMax, rect.yMax );
    }

    static public Vector2 BottomLeft( this Rect rect )
    {
        return new Vector2( rect.xMin, rect.yMin );
    }

    static public Vector2 BottomRight( this Rect rect )
    {
        return new Vector2( rect.xMax, rect.yMin );
    }

    static public Rect ToRect( this Bounds bound )
    {
        return new Rect( bound.center - bound.extents, bound.size );
    }

    static public bool Contains( this Rect rect, Rect other )
    {
        return rect.Contains( other.TopLeft() )
             && rect.Contains( other.TopRight() )
             && rect.Contains( other.BottomLeft() )
             && rect.Contains( other.BottomRight() );
    }

    static public bool Contains( this Rect rect, Bounds other )
    {
        return rect.Contains( other.ToRect() );
    }
}