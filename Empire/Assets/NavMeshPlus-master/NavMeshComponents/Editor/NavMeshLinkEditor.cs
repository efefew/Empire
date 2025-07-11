using NavMeshPlus.Components;
using UnityEditor;
using UnityEditor.AI;
using UnityEngine;

namespace NavMeshPlus.Editors.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NavMeshLink))]
    internal class NavMeshLinkEditor : Editor
    {
        private static int s_SelectedID;
        private static int s_SelectedPoint = -1;

        private static Color s_HandleColor = new Color(255f, 167f, 39f, 210f) / 255;
        private static Color s_HandleColorDisabled = new Color(255f * 0.75f, 167f * 0.75f, 39f * 0.75f, 100f) / 255;
        private SerializedProperty m_AgentTypeID;
        private SerializedProperty m_Area;
        private SerializedProperty m_AutoUpdatePosition;
        private SerializedProperty m_Bidirectional;
        private SerializedProperty m_CostModifier;
        private SerializedProperty m_EndPoint;
        private SerializedProperty m_StartPoint;
        private SerializedProperty m_Width;

        private void OnEnable()
        {
            m_AgentTypeID = serializedObject.FindProperty("m_AgentTypeID");
            m_Area = serializedObject.FindProperty("m_Area");
            m_CostModifier = serializedObject.FindProperty("m_CostModifier");
            m_AutoUpdatePosition = serializedObject.FindProperty("m_AutoUpdatePosition");
            m_Bidirectional = serializedObject.FindProperty("m_Bidirectional");
            m_EndPoint = serializedObject.FindProperty("m_EndPoint");
            m_StartPoint = serializedObject.FindProperty("m_StartPoint");
            m_Width = serializedObject.FindProperty("m_Width");

            s_SelectedID = 0;
            s_SelectedPoint = -1;

            NavMeshVisualizationSettings.showNavigation++;
        }

        private void OnDisable()
        {
            NavMeshVisualizationSettings.showNavigation--;
        }

        public void OnSceneGUI()
        {
            NavMeshLink navLink = (NavMeshLink)target;
            if (!navLink.enabled)
                return;

            Matrix4x4 mat = UnscaledLocalToWorldMatrix(navLink.transform);

            Vector3 startPt = mat.MultiplyPoint(navLink.startPoint);
            Vector3 endPt = mat.MultiplyPoint(navLink.endPoint);
            Vector3 midPt = Vector3.Lerp(startPt, endPt, 0.35f);
            float startSize = HandleUtility.GetHandleSize(startPt);
            float endSize = HandleUtility.GetHandleSize(endPt);
            float midSize = HandleUtility.GetHandleSize(midPt);

            Quaternion zup = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
            Vector3 right = mat.MultiplyVector(CalcLinkRight(navLink));

            Color oldColor = Handles.color;
            Handles.color = s_HandleColor;

            Vector3 pos;

            if (navLink.GetInstanceID() == s_SelectedID && s_SelectedPoint == 0)
            {
                EditorGUI.BeginChangeCheck();
                Handles.CubeHandleCap(0, startPt, zup, 0.1f * startSize, Event.current.type);
                pos = Handles.PositionHandle(startPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navLink, "Move link point");
                    navLink.startPoint = mat.inverse.MultiplyPoint(pos);
                }
            }
            else
            {
                if (Handles.Button(startPt, zup, 0.1f * startSize, 0.1f * startSize, Handles.CubeHandleCap))
                {
                    s_SelectedPoint = 0;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }

            if (navLink.GetInstanceID() == s_SelectedID && s_SelectedPoint == 1)
            {
                EditorGUI.BeginChangeCheck();
                Handles.CubeHandleCap(0, endPt, zup, 0.1f * startSize, Event.current.type);
                pos = Handles.PositionHandle(endPt, navLink.transform.rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(navLink, "Move link point");
                    navLink.endPoint = mat.inverse.MultiplyPoint(pos);
                }
            }
            else
            {
                if (Handles.Button(endPt, zup, 0.1f * endSize, 0.1f * endSize, Handles.CubeHandleCap))
                {
                    s_SelectedPoint = 1;
                    s_SelectedID = navLink.GetInstanceID();
                }
            }

            EditorGUI.BeginChangeCheck();
            pos = Handles.Slider(midPt + right * navLink.width * 0.5f, right, midSize * 0.03f, Handles.DotHandleCap, 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(navLink, "Adjust link width");
                navLink.width = Mathf.Max(0.0f, 2.0f * Vector3.Dot(right, pos - midPt));
            }

            EditorGUI.BeginChangeCheck();
            pos = Handles.Slider(midPt - right * navLink.width * 0.5f, -right, midSize * 0.03f, Handles.DotHandleCap,
                0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(navLink, "Adjust link width");
                navLink.width = Mathf.Max(0.0f, 2.0f * Vector3.Dot(-right, pos - midPt));
            }

            Handles.color = oldColor;
        }

        private static Matrix4x4 UnscaledLocalToWorldMatrix(Transform t)
        {
            return Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
        }

        private void AlignTransformToEndPoints(NavMeshLink navLink)
        {
            Matrix4x4 mat = UnscaledLocalToWorldMatrix(navLink.transform);

            Vector3 worldStartPt = mat.MultiplyPoint(navLink.startPoint);
            Vector3 worldEndPt = mat.MultiplyPoint(navLink.endPoint);

            Vector3 forward = worldEndPt - worldStartPt;
            Vector3 up = navLink.transform.up;

            // Flatten
            forward -= Vector3.Dot(up, forward) * up;

            Transform transform = navLink.transform;
            transform.rotation = Quaternion.LookRotation(forward, up);
            transform.position = (worldEndPt + worldStartPt) * 0.5f;
            transform.localScale = Vector3.one;

            navLink.startPoint = transform.InverseTransformPoint(worldStartPt);
            navLink.endPoint = transform.InverseTransformPoint(worldEndPt);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", m_AgentTypeID);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_StartPoint);
            EditorGUILayout.PropertyField(m_EndPoint);

            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);
            if (GUILayout.Button("Swap"))
            {
                foreach (NavMeshLink navLink in targets)
                {
                    Vector3 tmp = navLink.startPoint;
                    navLink.startPoint = navLink.endPoint;
                    navLink.endPoint = tmp;
                }

                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Align Transform"))
            {
                foreach (NavMeshLink navLink in targets)
                {
                    Undo.RecordObject(navLink.transform, "Align Transform to End Points");
                    Undo.RecordObject(navLink, "Align Transform to End Points");
                    AlignTransformToEndPoints(navLink);
                }

                SceneView.RepaintAll();
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_Width);
            EditorGUILayout.PropertyField(m_CostModifier);
            EditorGUILayout.PropertyField(m_AutoUpdatePosition);
            EditorGUILayout.PropertyField(m_Bidirectional);

            NavMeshComponentsGUIUtility.AreaPopup("Area Type", m_Area);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
        }

        private static Vector3 CalcLinkRight(NavMeshLink navLink)
        {
            Vector3 dir = navLink.endPoint - navLink.startPoint;
            return new Vector3(-dir.z, 0.0f, dir.x).normalized;
        }

        private static void DrawLink(NavMeshLink navLink)
        {
            Vector3 right = CalcLinkRight(navLink);
            float rad = navLink.width * 0.5f;

            Gizmos.DrawLine(navLink.startPoint - right * rad, navLink.startPoint + right * rad);
            Gizmos.DrawLine(navLink.endPoint - right * rad, navLink.endPoint + right * rad);
            Gizmos.DrawLine(navLink.startPoint - right * rad, navLink.endPoint - right * rad);
            Gizmos.DrawLine(navLink.startPoint + right * rad, navLink.endPoint + right * rad);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active | GizmoType.Pickable)]
        private static void RenderBoxGizmo(NavMeshLink navLink, GizmoType gizmoType)
        {
            if (!EditorApplication.isPlaying)
                navLink.UpdateLink();

            Color color = s_HandleColor;
            if (!navLink.enabled)
                color = s_HandleColorDisabled;

            Color oldColor = Gizmos.color;
            Matrix4x4 oldMatrix = Gizmos.matrix;

            Gizmos.matrix = UnscaledLocalToWorldMatrix(navLink.transform);

            Gizmos.color = color;
            DrawLink(navLink);

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;

            Gizmos.DrawIcon(navLink.transform.position, "NavMeshLink Icon", true);
        }

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
        private static void RenderBoxGizmoNotSelected(NavMeshLink navLink, GizmoType gizmoType)
        {
            if (NavMeshVisualizationSettings.showNavigation > 0)
            {
                Color color = s_HandleColor;
                if (!navLink.enabled)
                    color = s_HandleColorDisabled;

                Color oldColor = Gizmos.color;
                Matrix4x4 oldMatrix = Gizmos.matrix;

                Gizmos.matrix = UnscaledLocalToWorldMatrix(navLink.transform);

                Gizmos.color = color;
                DrawLink(navLink);

                Gizmos.matrix = oldMatrix;
                Gizmos.color = oldColor;
            }

            Gizmos.DrawIcon(navLink.transform.position, "NavMeshLink Icon", true);
        }

        [MenuItem("GameObject/Navigation/NavMesh Link", false, 2002)]
        public static void CreateNavMeshLink(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            GameObject go = NavMeshComponentsGUIUtility.CreateAndSelectGameObject("NavMesh Link", parent);
            go.AddComponent<NavMeshLink>();
            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
                view.MoveToView(go.transform);
        }
    }
}