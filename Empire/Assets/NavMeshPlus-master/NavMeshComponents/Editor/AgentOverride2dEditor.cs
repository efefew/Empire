﻿using NavMeshPlus.Extensions;
using UnityEditor;

namespace NavMeshPlus.Editors.Extensions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AgentOverride2d))]
    internal class AgentOverride2dEditor : Editor
    {
        //SerializedProperty m_OverrideByGrid;
        //SerializedProperty m_UseMeshPrefab;
        //SerializedProperty m_CompressBounds;
        //SerializedProperty m_OverrideVector;
        private void OnEnable()
        {
            //m_OverrideByGrid = serializedObject.FindProperty("m_OverrideByGrid");
            //m_UseMeshPrefab = serializedObject.FindProperty("m_UseMeshPrefab");
            //m_CompressBounds = serializedObject.FindProperty("m_CompressBounds");
            //m_OverrideVector = serializedObject.FindProperty("m_OverrideVector");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            AgentOverride2d agent = target as AgentOverride2d;
            EditorGUILayout.LabelField("Agent Override", agent.agentOverride?.GetType().Name);
        }
    }
}