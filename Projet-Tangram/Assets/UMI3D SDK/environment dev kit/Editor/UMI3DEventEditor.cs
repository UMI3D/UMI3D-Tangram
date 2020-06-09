using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using umi3d.edk;

[CustomEditor(typeof(UMI3DEvent))]
public class UMI3DEventEditor : Editor
{

    SerializedProperty icon2D;
    SerializedProperty icon3D;
    SerializedProperty interactionName;
    SerializedProperty tool;
    SerializedProperty hold;

    SerializedProperty onHold;
    SerializedProperty onRelease;
    SerializedProperty onTrigger;


    protected virtual void Awake()
    {
        icon2D = serializedObject.FindProperty("Icon2D");
        icon3D = serializedObject.FindProperty("Icon3D");
        interactionName = serializedObject.FindProperty("InteractionName");
        tool = serializedObject.FindProperty("tool");
        hold = serializedObject.FindProperty("Hold");

        onHold = serializedObject.FindProperty("onHold");
        onRelease = serializedObject.FindProperty("onRelease");
        onTrigger = serializedObject.FindProperty("onTrigger");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(icon2D);
        EditorGUILayout.PropertyField(icon3D);
        EditorGUILayout.PropertyField(interactionName);
        EditorGUILayout.PropertyField(tool);
        EditorGUILayout.PropertyField(hold);

        if ((target as UMI3DEvent).Hold)
        {
            EditorGUILayout.PropertyField(onHold);
            EditorGUILayout.PropertyField(onRelease);
        }
        else
            EditorGUILayout.PropertyField(onTrigger);

        serializedObject.ApplyModifiedProperties();
    }

}
