using UnityEditor;
using UnityEngine;
using OctoXR.Editor.KinematicInteractions;

namespace OctoXR.Scripts.Editor
{
    [InitializeOnLoad]
    public class OctoSetupWizard : EditorWindow
    {
        private static OctoSetupWizard window;
        private static OctoSettings octoSettings;
        private static readonly float spacing = 12f;

        static OctoSetupWizard()
        {
            EditorApplication.update += Start;
        }
    
        static void Start()
        {
            if (octoSettings == null)
            {
                octoSettings = Resources.Load<OctoSettings>("OctoSettings");
            }

            if (!octoSettings.SettingsApplied) OpenWindow();

            EditorApplication.update -= Start;
        }

        [MenuItem("Window/OctoXR/Setup")]
        public static void OpenWindow()
        {
            window = GetWindow<OctoSetupWizard>(true);
            window.minSize = window.maxSize = new Vector2(350, 430);
            window.titleContent = new GUIContent("OctoXR Setup");
        }
    
        private void OnGUI()
        {
            GUILayoutSetup();
        }
    
        private void GUILayoutSetup()
        {
            var headerLabelStyle = OctoGUIStyles.LabelStyle(TextAnchor.MiddleLeft, FontStyle.Bold, 15);
            var smallLabelStyle = OctoGUIStyles.LabelStyle(TextAnchor.MiddleLeft, FontStyle.Normal, 12);
            var titleLabelStyle = OctoGUIStyles.LabelStyle(TextAnchor.MiddleCenter, FontStyle.Normal, 25);

            var rect = EditorGUILayout.GetControlRect();
            rect.height *= 5;

            GUILayout.BeginHorizontal();
        
            GUI.Label(rect, (Texture2D)Resources.Load("oxr_banner", typeof(Texture2D)), OctoGUIStyles.LabelStyle(TextAnchor.MiddleCenter, FontStyle.Normal, 25));
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Space(spacing / 2);
            GUILayout.Label("OctoXR Setup", titleLabelStyle);
        
            GUILayout.Space(spacing * 3);

            GUILayout.Box("OctoXR has a few prerequisites that are necessary in order for the package to work, " +
                          "like adding some layers " +
                          "as well as a specific set of physics and time settings. " +
                          "Press the button below to set everything up.", OctoGUIStyles.BoxStyle(TextAnchor.MiddleCenter, FontStyle.Normal, 12));

            GUILayout.Label("Layers", headerLabelStyle);
            GUILayout.Label($"Layer {Constants.OctoPlayerLayer}: {Constants.OctoPlayer}", smallLabelStyle);
            GUILayout.Label($"Layer {Constants.GrabbableLayer}: {Constants.Grabbable}", smallLabelStyle);
            GUILayout.Label($"Layer {Constants.HandLayer} : {Constants.Hand}", smallLabelStyle);
        
            GUILayout.Space(spacing);
        
            GUILayout.Label("Physics settings", headerLabelStyle);
            GUILayout.Label($"Default solver iterations: {Constants.SolverIterations}", smallLabelStyle);
            GUILayout.Label($"Default solver velocity iterations: {Constants.SolverVelocityIterations}", smallLabelStyle);
        
            GUILayout.Space(spacing);
        
            GUILayout.Label("Time settings", headerLabelStyle);
            GUILayout.Label($"Fixed timestep: {Constants.FixedTimeStep}", smallLabelStyle);
        
            GUILayout.EndVertical();
        
            GUILayout.Space(spacing);
        
            if (GUILayout.Button("Apply Settings"))
            {
                ApplyRequiredLayers();
                ApplyPhysicsSettings();
                ApplyTimeSettings();

                WarningWindow.Open("Settings Confirmation", "Settings Applied!");
                octoSettings.SettingsApplied = true;
                EditorUtility.SetDirty(octoSettings);
            }
        }

        private static void ApplyRequiredLayers()
        {
            Object tagManager = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset");

            if (tagManager != null)
            {
                SerializedObject serializedObject = new SerializedObject(tagManager);
                SerializedProperty layers = serializedObject.FindProperty("layers");

                layers.DeleteArrayElementAtIndex(Constants.OctoPlayerLayer);
                layers.InsertArrayElementAtIndex(Constants.OctoPlayerLayer);
                layers.GetArrayElementAtIndex(Constants.OctoPlayerLayer).stringValue = Constants.OctoPlayer;

                layers.DeleteArrayElementAtIndex(Constants.GrabbableLayer);
                layers.InsertArrayElementAtIndex(Constants.GrabbableLayer);
                layers.GetArrayElementAtIndex(Constants.GrabbableLayer).stringValue = Constants.Grabbable;

                layers.DeleteArrayElementAtIndex(Constants.HandLayer);
                layers.InsertArrayElementAtIndex(Constants.HandLayer);
                layers.GetArrayElementAtIndex(Constants.HandLayer).stringValue = Constants.Hand;

                serializedObject.ApplyModifiedProperties();
            }
        }

        private static void ApplyPhysicsSettings()
        {
            UnityEngine.Physics.defaultSolverIterations = Constants.SolverIterations;
            UnityEngine.Physics.defaultSolverVelocityIterations = Constants.SolverVelocityIterations;
            UnityEngine.Physics.IgnoreLayerCollision(Constants.OctoPlayerLayer, Constants.OctoPlayerLayer);
            UnityEngine.Physics.IgnoreLayerCollision(Constants.OctoPlayerLayer, Constants.GrabbableLayer);
            UnityEngine.Physics.IgnoreLayerCollision(Constants.OctoPlayerLayer, Constants.HandLayer);
        }

        private static void ApplyTimeSettings()
        {
            Time.fixedDeltaTime = Constants.FixedTimeStep;
        }
    }
}
