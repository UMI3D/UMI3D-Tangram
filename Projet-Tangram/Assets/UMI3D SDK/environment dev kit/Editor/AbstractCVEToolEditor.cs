using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using umi3d.edk;


[CustomEditor(typeof(AbstractCVETool), true)]
public class AbstractCVEToolEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Interactions :");

        List<AbstractInteraction> interactions = (target as AbstractCVETool).Interactions;
        List<AbstractInteraction> interactionsCopy = new List<AbstractInteraction>(interactions);

        for(int i = 0; i < interactionsCopy.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("#" + i + ": " + interactionsCopy[i].InteractionName);
            
            if (GUILayout.Button("up"))
            {
                if (i > 0)
                {
                    AbstractInteraction buffer = interactions[i];
                    interactions[i] = interactions[i - 1];
                    interactions[i - 1] = buffer;
                }
            }

            if (GUILayout.Button("down"))
            {
                if (i < interactionsCopy.Count - 1)
                {
                    AbstractInteraction buffer = interactions[i];
                    interactions[i] = interactions[i + 1];
                    interactions[i + 1] = buffer;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

    }

}
