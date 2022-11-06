using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VisionCone))]
public class FOVEditor : Editor
{
    private void OnSceneGUI()
    {
        VisionCone visionCone = (VisionCone)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(visionCone.transform.position, Vector3.up, Vector3.forward, 360, visionCone.ViewRadius);
        Vector3 viewAngleA = visionCone.DirFromAngle(-visionCone.ViewAngle / 2, false);
        Vector3 viewAngleB = visionCone.DirFromAngle(visionCone.ViewAngle / 2, false);

        Handles.DrawLine(visionCone.transform.position, visionCone.transform.position + viewAngleA * visionCone.ViewRadius);
        Handles.DrawLine(visionCone.transform.position, visionCone.transform.position + viewAngleB * visionCone.ViewRadius);

        foreach (Transform visibleTarget in visionCone._visibleTargets)
        {
            Handles.color = Color.red;
            Handles.DrawLine(visionCone.transform.position, visibleTarget.position);
        }
    }
}
