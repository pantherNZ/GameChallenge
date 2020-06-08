using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Language
{
    English,
}

[Serializable]
public class BaseDataType
{
    public string id;

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public override bool Equals( object obj )
    {
        var other = obj as BaseDataType;
        if( other == null )
            return false;
        return id == other.id;
    }
}

[Serializable]
public class GameStringJSON
{
    public string Key;
    public GameString Value;
}

[Serializable]
public class GameString
{
    public string Get( Language lang )
    {
        switch( lang )
        {
            case Language.English: return english;
            default: throw new Exception( "Language not implemented" );
        }
    }

    [SerializeField]
    string english = null;
}