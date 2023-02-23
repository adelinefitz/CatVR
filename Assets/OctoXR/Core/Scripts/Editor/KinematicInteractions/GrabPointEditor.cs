using OctoXR.KinematicInteractions;
using UnityEditor;
using UnityEngine;

namespace OctoXR.Editor.KinematicInteractions
{
    [CustomEditor(typeof(GrabPoint))]
    [CanEditMultipleObjects]
    public class GrabPointEditor : UnityEditor.Editor
    {
        [HideInInspector] [SerializeField] private GameObject[] allObjects;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var monoBeh = (MonoBehaviour) target;
            var previewButton = false;
            var closeButton = false;
            var editButton = false;

            var grabPoint = monoBeh.GetComponent<GrabPoint>();
            
            if (!grabPoint.InstantiatedPreviewHand)
            {
                previewButton = GUILayout.Button("Preview Pose");
            }
            else
            {
                editButton = GUILayout.Button("Edit pose");
                closeButton = GUILayout.Button("Hide Pose");
            }

            if (previewButton)
            {
                grabPoint.InstantiateHandPose();

                InspectorLock.LockEditor();
                ActiveEditorTracker.sharedTracker.ForceRebuild();
            }
            
            if (editButton)
            {
                InspectorLock.UnlockEditor();
                
                Selection.SetActiveObjectWithContext(grabPoint.InstantiatedPreviewHand, null);
                SceneView.FrameLastActiveSceneView();

                Tools.current = Tool.None;

                InspectorLock.LockEditor();
            }
            
            if (closeButton)
            {
                DeleteObject(grabPoint.InstantiatedPreviewHand);
                Tools.current = Tool.Move;

                InspectorLock.UnlockEditor();
            }
        }

        public static void DeleteObject(GameObject gameObject)
        {
            DestroyImmediate(gameObject);
        }
    }
}
