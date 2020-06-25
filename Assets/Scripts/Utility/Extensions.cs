﻿using System;
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

    public static void RemoveBySwap<T>( this List<T> list, int index )
    {
        list[index] = list[list.Count - 1];
        list.RemoveAt( list.Count - 1 );
    }

    public static bool RemoveBySwap<T>( this List<T> list, Func< T, bool > predicate )
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
        list.RemoveAt(0);
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

        return list[ ( int )( ( list.Count - 1 ) * UnityEngine.Random.value )];
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

    public static Vector3 SetX( this Vector3 vec, float x )
    {
        vec.x = x;
        return vec;
    }

    public static Vector3 SetY( this Vector3 vec, float y )
    {
        vec.y = y;
        return vec;
    }

    public static Vector3 SetZ( this Vector3 vec, float z )
    {
        vec.z = z;
        return vec;
    }

    public static Vector2 ToVector2( this Vector3 vec )
    {
        return new Vector2( vec.x, vec.y );
    }

    public static Vector3 ToVector3( this Vector2 vec, float z = 0.0f )
    {
        return new Vector3( vec.x, vec.y, z );
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
}