#if UNITY_EDITOR

using UnityEditor;
using UnityEngine.UI;

[UnityEditor.CustomEditor( typeof( UtilityComponent ) )]
class UtilityComponentEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var dataManager = target as UtilityComponent;

        var text = dataManager.GetComponent<Text>();
        var colour = text.color;
        colour = EditorGUILayout.ColorField( "New Color", colour );
        text.color = colour;
    }
}

#endif