/************************************************************************************

Filename    :   OVRPlayerControllerEditor.cs
Content     :   Player controller interface. 
				This script adds editor functionality to the OVRPlayerController
Created     :   January 17, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(OVRPlayerController))]

//-------------------------------------------------------------------------------------
// ***** OVRPlayerControllerEditor
//
// OVRPlayerControllerEditor adds extra functionality in the inspector for the currently
// selected OVRPlayerController.
//
public class OVRPlayerControllerEditor : Editor
{
    // target component
    private OVRPlayerController m_Component;

    // OnEnable
    void OnEnable()
    {
        m_Component = (OVRPlayerController)target;
    }

    // OnDestroy
    void OnDestroy()
    {
    }

    // OnInspectorGUI
    public override void OnInspectorGUI()
    {
        GUI.color = Color.white;

        //Undo.SetSnapshotTarget(m_Component, "OVRPlayerController");
        Undo.RecordObject(m_Component, "OVRPlayerController");      // ~RJB

        {
            m_Component.CameraController_OVR = EditorGUILayout.ObjectField("CameraController_OVR", m_Component.CameraController_OVR, typeof(OVRCameraController)) as OVRCameraController;
            m_Component.CameraController_GFL = EditorGUILayout.ObjectField("CameraController_GFL", m_Component.CameraController_GFL, typeof(GFLCamera_Zelda)) as GFLCamera_Zelda;
            m_Component.weaponContainerLeft  = EditorGUILayout.ObjectField("Weapon Container Left", m_Component.weaponContainerLeft, typeof(Transform)) as Transform;
            m_Component.weaponContainerRight = EditorGUILayout.ObjectField("Weapon Container Right", m_Component.weaponContainerRight, typeof(Transform)) as Transform;

            m_Component.Acceleration = EditorGUILayout.Slider("Acceleration", m_Component.Acceleration, 0, 1);
            m_Component.RunMultiplier = EditorGUILayout.Slider("Run Multiplier", m_Component.RunMultiplier, 1, 10);
            m_Component.Damping = EditorGUILayout.Slider("Damping", m_Component.Damping, 0, 1);
            m_Component.BackAndSideDampen = EditorGUILayout.Slider("Back and Side Dampen", m_Component.BackAndSideDampen, 0, 1);
            m_Component.JumpForce = EditorGUILayout.Slider("Jump Force", m_Component.JumpForce, 0, 10);
            m_Component.RotationAmount = EditorGUILayout.Slider("Rotation Amount", m_Component.RotationAmount, 0, 5);

            OVREditorGUIUtility.Separator();

            m_Component.GravityModifier = EditorGUILayout.Slider("Gravity Modifier", m_Component.GravityModifier, 0, 1);

            OVREditorGUIUtility.Separator();
        }

        if (GUI.changed)
        {
            Undo.CreateSnapshot();
            Undo.RegisterSnapshot();
            EditorUtility.SetDirty(m_Component);
        }

        Undo.ClearSnapshotTarget();
    }
}

