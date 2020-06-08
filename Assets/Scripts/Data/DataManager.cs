using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class SimpleDataSource<T>
{
    [SerializeField] protected TextAsset typesJson = null;
    [HideInInspector] public List<T> types;

    public virtual void LoadData()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset( typesJson.GetDataPathRelative() );
#endif
        types = JsonHelper.FromJson<T>( typesJson.text ).ToList();
    }
}

[Serializable]
public abstract class DataSource<T> where T : BaseDataType, new()
{
    [SerializeField] protected TextAsset typesJson = null;
    public T debugType = new T();
    [HideInInspector] public List<T> types;

    public int FindIndex()
    {
        if( types == null )
            LoadData();

        var searchName = debugType.id.ToLower().Replace( " ", "" );
        return types.FindIndex( obj => obj.id.ToLower().Replace( " ", "" ).Contains( searchName ) );
    }

#if UNITY_EDITOR
    public void ImportValuesFromJSON()
    {
        LoadData();
        var index = FindIndex();

        if( index == -1 )
        {
            UnityEditor.EditorUtility.DisplayDialog( "Import Error", "Failed to import values from \"" + debugType.id + "\"", "OK" );
            return;
        }

        debugType = types[index].DeepCopy();
    }

    public void ExportValuesToJSON()
    {
        var index = FindIndex();

        if( index == -1 )
        {
            if( !UnityEditor.EditorUtility.DisplayDialog( "Confirm Export", "Are you sure you want to create a new entry called \"" + debugType.id + "\"?", "Yes", "No" ) )
                return;

            types.Add( debugType );
            SaveData();
            return;
        }

        if( !UnityEditor.EditorUtility.DisplayDialog( "Confirm Export", "Are you sure you want to override the values for \"" + debugType.id + "\"?", "Yes", "No" ) )
            return;

        types[index] = debugType.DeepCopy();
        SaveData();
        return;
    }

    public void DrawImportExportButtons( string name )
    {
        GUILayout.BeginHorizontal();

        if( GUILayout.Button( String.Format( "Export {0} to JSON", name ), GUILayout.MinWidth( 250.0f ) ) )
            ExportValuesToJSON();

        if( GUILayout.Button( String.Format( "Import {0} from JSON", name ), GUILayout.MinWidth( 250.0f ) ) )
            ImportValuesFromJSON();

        GUILayout.EndHorizontal();
    }

    public virtual void SaveData()
    {
        System.IO.File.WriteAllText( typesJson.GetDataPathAbsolute(), JsonHelper.ToJson( types.ToArray(), true ) );
    }
#endif

    public virtual void LoadData()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset( typesJson.GetDataPathRelative() );
#endif
        types = JsonHelper.FromJson<T>( typesJson.text ).ToList();
    }
}

[Serializable]
public abstract class DataSourceWithCustomJSON<T, TJSON> : DataSource< T >
    where T : BaseDataType, new() 
    where TJSON : BaseDataType, new()
{
    List<TJSON> typesIntermediate = new List<TJSON>();

#if UNITY_EDITOR
    public override void SaveData()
    {
        typesIntermediate.Clear();
        foreach( var x in types )
        {
            var newTJSON = new TJSON();
            var type = typeof( T );
            foreach( var fieldInf in type.GetFields() )
            {
                if( newTJSON.GetType().GetField( fieldInf.Name ) != null )
                    fieldInf.SetValue( newTJSON, fieldInf.GetValue( x ) );
                else
                    if( !( bool )newTJSON.GetType().GetMethod( "CopyValueFrom" ).Invoke( newTJSON, new object[] { fieldInf.Name, fieldInf.GetValue( x ) } ) )
                        Debug.LogException( new System.Exception( "SaveData failed to handle CopyValueFrom, field: " + fieldInf.Name + ", value: " + fieldInf.GetValue( x ).ToString() ) );
            }

            typesIntermediate.Add( newTJSON );
        }

        System.IO.File.WriteAllText( typesJson.GetDataPathAbsolute(), JsonHelper.ToJson( typesIntermediate.ToArray(), true ) );
    }
#endif

    public override void LoadData()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset( typesJson.GetDataPathRelative() );
#endif
        typesIntermediate = JsonHelper.FromJson<TJSON>( typesJson.text ).ToList();

        types.Clear();
        foreach( var x in typesIntermediate )
        {
            var newT = new T();
            var type = typeof( TJSON );
            foreach( var fieldInf in type.GetFields() )
            {
                if( newT.GetType().GetField( fieldInf.Name ) != null )
                    fieldInf.SetValue( newT, fieldInf.GetValue( x ) );
                else
                    if( !( bool )newT.GetType().GetMethod( "CopyValueFrom" ).Invoke( newT, new object[] { fieldInf.Name, fieldInf.GetValue( x ) } ) )
                        Debug.LogException( new System.Exception( "LoadData failed to load type from string value in JSON, field: " + fieldInf.Name + ", value: " + fieldInf.GetValue( x ).ToString() ) );
            }
                    
            types.Add( newT );
        }
    }
}

public class DataManager : MonoBehaviour
{
    [SerializeField] TextAsset stringsJson = null;
    Dictionary<string, GameString> strings = new Dictionary<string, GameString>();
    public Language language = Language.English;

    public string GetGameString( string key )
    {
        if( !strings.ContainsKey( key ) )
        {
            Debug.LogWarning( "Game string for stat not found, stat: " + key );
            return "";
        }

        return strings[key].Get( language );
    }

    public string GetGameStringFormatted( string key, int arg )
    {
        if( !strings.ContainsKey( key ) )
        {
            Debug.LogWarning( "Game string for stat not found, stat: " + key );
            return "";
        }

        return string.Format( new GameStringFormatter(), strings[key].Get( language ), arg );
    }

    public void LoadData()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset( stringsJson.GetDataPathRelative() );
#endif
        strings = JsonHelper.FromJson<GameStringJSON>( stringsJson.text ).ToDictionary( x => x.Key, x => x.Value );
    }

    // Singleton
    [HideInInspector] static DataManager dataManager;
    [HideInInspector] public static DataManager Instance { get { return dataManager; } }

    void Awake()
    {
        if( dataManager != null && dataManager != this )
        {
            Destroy( gameObject );
            return;
        }

        dataManager = this;
        DontDestroyOnLoad( gameObject );
        LoadData();
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>( string json )
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>( json );
        return wrapper.Items;
    }

    public static string ToJson<T>( T[] array )
    {
        Wrapper<T> wrapper = new Wrapper<T> { Items = array };
        return JsonUtility.ToJson( wrapper );
    }

    public static string ToJson<T>( T[] array, bool prettyPrint )
    {
        Wrapper<T> wrapper = new Wrapper<T> { Items = array };
        return JsonUtility.ToJson( wrapper, prettyPrint );
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

public class GameStringFormatter : IFormatProvider, ICustomFormatter
{
    public object GetFormat( Type formatType )
    {
        if( formatType == typeof( ICustomFormatter ) )
            return this;
        return null;
    }

    public string Format( string format, object arg, IFormatProvider formatProvider )
    {
        if( !Equals( formatProvider ) )
            return null;

        if( String.IsNullOrEmpty( format ) )
            return arg.ToString();

        switch( format.ToLower()[0] )
        {
            case '/':       return ( ( int )arg / int.Parse( format.Substring( 1 ) ) ).ToString();
            case '*':       return ( ( int )arg * int.Parse( format.Substring( 1 ) ) ).ToString();
            case '+':       return ( ( int )arg + int.Parse( format.Substring( 1 ) ) ).ToString();
            case '-':       return ( ( int )arg - int.Parse( format.Substring( 1 ) ) ).ToString();
            default:        throw new FormatException( String.Format( "The '{0}' format specifier is not supported.", format ) );
        }
    }
}