namespace KFrameCompliant
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(Layout))]
    public class LayoutEditor : Editor
    {

        private string jsonString;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Layout myTarget = (Layout)target;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("JSON String to load");
            jsonString = EditorGUILayout.TextField(jsonString);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUILayout.Button("Save Json"))
            {
                myTarget.SaveJSON();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Load Json"))
            {
                myTarget.LoadJSON(jsonString);
            }

        }

    }
}
