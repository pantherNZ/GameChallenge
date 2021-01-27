using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Game
{
    public static class SaveGameSystem
    {
        public static string folderName = "/SavedGames/";
        public static string fileExtension = ".dat";

        static string ConvertSaveNameToPath( string name )
        {
            return Application.persistentDataPath + folderName + name + fileExtension;
        }

        static string ConvertPathToSaveName( string path )
        {
            var index = path.LastIndexOf( '/' ) + 1;
            return path.Substring( index, path.Length - index - fileExtension.Length );
        }

        public static bool SaveExists( string name )
        {
            return GetSaveGames().Find( x => x == name ) != null;
        }

        public static List<string> GetSaveGames()
        {
            string folderPath = Application.persistentDataPath + folderName;

            if( !Directory.Exists( folderPath ) )
                return new List<string>();

            var files = Directory.GetFiles( folderPath, "*" + fileExtension );
            files = Array.ConvertAll( files, x => ConvertPathToSaveName( x ) );
            return files.ToList();
        }

        public static bool SaveGame( string name )
        {
            if( name.Length == 0 )
                return false;

            string folderPath = Application.persistentDataPath + folderName;
            string fullPath = ConvertSaveNameToPath( name );

            if( !Directory.Exists( folderPath ) )
                Directory.CreateDirectory( folderPath );

            try
            {
                using( var fileStream = File.Open( fullPath, FileMode.OpenOrCreate ) )
                {
                    using( var memoryStream = new MemoryStream() )
                    {
                        using( var writer = new BinaryWriter( memoryStream ) )
                        {
                            foreach( var subscriber in subscribers )
                                subscriber.Serialise( writer );
                        }

                        var content = memoryStream.ToArray();
                        fileStream.Write( content, 0, content.Length );
                    }
                }
            }
            catch( Exception e )
            {
                // TODO: Error handling
                Debug.LogError( "Failed to save game: " + e.ToString() );
                return false;
            }

            return true;
        }

        public static bool LoadGame( string name )
        {
            string fullPath = ConvertSaveNameToPath( name );

            if( !File.Exists( fullPath ) )
                return false;

            try
            {
                using( var fileStream = File.Open( fullPath, FileMode.Open ) )
                {
                    byte[] bytes = new byte[fileStream.Length];
                    fileStream.Read( bytes, 0, bytes.Length );

                    using( var memoryStream = new MemoryStream( bytes, writable: false ) )
                    {
                        using( var reader = new BinaryReader( memoryStream ) )
                        {
                            foreach( var subscriber in subscribers )
                                subscriber.Deserialise( reader );
                        }
                    }
                }
            }
            catch( Exception e )
            {
                // TODO: Error handling
                Debug.LogError( "Failed to load game: " + e.ToString() );
                return false;
            }

            return true;
        }

        public static bool DeleteSave( string name )
        {
            string fullPath = ConvertSaveNameToPath( name );

            if( !File.Exists( fullPath ) )
                return false;

            File.Delete( fullPath );
            return true;
        }

        public static void AddSaveableObject( ISavableObject obj )
        {
            subscribers.Add( obj );
        }

        static readonly List<ISavableObject> subscribers = new List<ISavableObject>();
    }

    // Base savable object interface
    public interface ISavableObject
    {
        void Serialise( BinaryWriter writer );
        void Deserialise( BinaryReader reader );
    }

    public static partial class Extensions
    {
        public static T Get<T>( this Dictionary<string, object> instance, string name )
        {
            return ( T )instance[name];
        }

        public static Texture2D ToTexture2D( this RenderTexture rTex )
        {
            Texture2D tex = new Texture2D( rTex.width, rTex.height, TextureFormat.ARGB32, false );
            var previous = RenderTexture.active;
            RenderTexture.active = rTex;
            tex.ReadPixels( new Rect( 0, 0, rTex.width, rTex.height ), 0, 0 );
            tex.Apply();
            RenderTexture.active = previous;
            return tex;
        }

        public static RenderTexture ToRenderTexture( this Texture2D tex )
        {
            RenderTexture rTex = new RenderTexture( tex.width, tex.height, 0, RenderTextureFormat.ARGB32 ) { filterMode = FilterMode.Bilinear };
            rTex.Create();
            Graphics.CopyTexture( tex, rTex );
            return rTex;
        }

        // Helper functions
        public static void Write( this BinaryWriter writer, Vector2 vec )
        {
            writer.Write( vec.x );
            writer.Write( vec.y );
        }

        public static Vector2 ReadVector2( this BinaryReader reader )
        {
            return new Vector2
            {
                x = reader.ReadSingle(),
                y = reader.ReadSingle()
            };
        }

        public static void Write( this BinaryWriter writer, Vector2Int vec )
        {
            writer.Write( vec.x );
            writer.Write( vec.y );
        }

        public static Vector2Int ReadVector2Int( this BinaryReader reader )
        {
            return new Vector2Int
            {
                x = reader.ReadInt32(),
                y = reader.ReadInt32()
            };
        }

        public static void Write( this BinaryWriter writer, Vector3 vec )
        {
            writer.Write( vec.x );
            writer.Write( vec.y );
            writer.Write( vec.z );
        }

        public static Vector3 ReadVector3( this BinaryReader reader )
        {
            return new Vector3
            {
                x = reader.ReadSingle(),
                y = reader.ReadSingle(),
                z = reader.ReadSingle()
            };
        }

        public static void Write( this BinaryWriter writer, Vector3Int vec )
        {
            writer.Write( vec.x );
            writer.Write( vec.y );
            writer.Write( vec.z );
        }

        public static Vector3Int ReadVector3Int( this BinaryReader reader )
        {
            return new Vector3Int
            {
                x = reader.ReadInt32(),
                y = reader.ReadInt32(),
                z = reader.ReadInt32()
            };
        }
    }
}