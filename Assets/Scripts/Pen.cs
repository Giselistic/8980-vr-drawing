using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Pen : MonoBehaviour
{
    [Header("Pen Properties")]
    public Transform tip;
    public Material drawingMaterial;
    public Material tipMaterial;
    [Range(0.01f, 0.5f)]
    // public float penWidth = 0.01f;
    // public Color[] penColors;
    // public GameObject cubeToDraw;
    // public GameObject sphereToDraw;

    [Header("Drawing")]
    public StrokeBuilder_Tube giselistiStrokeBuilder;
    public Transform drawingTransform;


    [Header("XR Interaction")]
    public XRGrabInteractable grabInteractable;
    public XRDirectInteractor leftHandInteractor;   // Assign Left Hand XR Direct Interactor
    public XRDirectInteractor rightHandInteractor;  // Assign Right Hand XR Direct Interactor

    // private LineRenderer currentDrawing;
    // private int index;
    private int currentColorIndex;
    private bool isLeftGrabbed = false;
    private bool isRightGrabbed = false;
    private UnityEngine.XR.InputDevice leftController;
    private UnityEngine.XR.InputDevice rightController;

    private XRNode activeDrawingHand;
    // private XRNode oppositeHand => activeDrawingHand == XRNode.LeftHand ? XRNode.RightHand : XRNode.LeftHand;



    private float LeftTriggerValue;
    private float RightTriggerValue;
    private bool firstTimePressingLeftTrigger = true;
    private bool firstTimePressingRightTrigger = true;
    private GameObject currentStrokeGameObject;


    private void Start()
    {
        StartCoroutine(InitCouroutine());
    }


    IEnumerator InitCouroutine()
    {
        yield return new WaitForSeconds(2f);

        currentColorIndex = 0;
        // tipMaterial.color = penColors[currentColorIndex];

        // Subscribe to grab events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnLeftGrab);
            grabInteractable.selectExited.AddListener(OnLeftRelease);
            grabInteractable.selectEntered.AddListener(OnRightGrab);
            grabInteractable.selectExited.AddListener(OnRightRelease);
        }


        // Get input devices for left and right controllers
        leftController = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void UpdateTriggerValues()
{
    leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out LeftTriggerValue);
    rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out RightTriggerValue);
}


// ===============================================================================================================================
    private void Update()
    {
        
        UpdateTriggerValues();

        float morphTriggerValue;
        float sizeTriggerValue;

        if (activeDrawingHand == XRNode.LeftHand)
        {
            print ("drawing with left hand");
            morphTriggerValue = RightTriggerValue;
            sizeTriggerValue = LeftTriggerValue;
            print ("left size trigger value: " + sizeTriggerValue);

        }

        else
        {
            print ("drawing with right hand");
            morphTriggerValue = LeftTriggerValue;
            sizeTriggerValue = RightTriggerValue;
            print ("right size trigger value: " + sizeTriggerValue);
        }


        bool isDrawing = sizeTriggerValue > 0.01f;

        // morphTriggerValue = morphTriggerValue;

        // Drawing state

        if (isDrawing)
        {
            // Stroke width interpolation
            sizeTriggerValue = Mathf.Lerp(0.06f, 4f, sizeTriggerValue); // Adjust min/max
            
            if (firstTimePressingLeftTrigger || firstTimePressingRightTrigger)
            {
                currentStrokeGameObject = giselistiStrokeBuilder.CreateStroke(drawingTransform, drawingMaterial, tip, morphTriggerValue, sizeTriggerValue);
                firstTimePressingLeftTrigger = firstTimePressingRightTrigger = false;
            }
            else
            {
                giselistiStrokeBuilder.AddSampleToStroke(currentStrokeGameObject, drawingMaterial, tip, morphTriggerValue, sizeTriggerValue);
            }
        }
        else
        {
            if (currentStrokeGameObject != null)
            {
                giselistiStrokeBuilder.CompleteStroke(currentStrokeGameObject, drawingMaterial, tip, morphTriggerValue, sizeTriggerValue);
                firstTimePressingLeftTrigger = firstTimePressingRightTrigger = true;
            }
        }

        


    }

//=================================================================================================================================
    // private void OnLeftGrab(SelectEnterEventArgs args)
    // {
    //     // Check if left or right hand is grabbing with args
    //     if (args.interactorObject.handedness == InteractorHandedness.Left)
    //     {
    //         isLeftGrabbed = true;

    //     }
    // }

    // private void OnRightGrab(SelectEnterEventArgs args)
    // {
    //     if (args.interactorObject.handedness == InteractorHandedness.Right)
    //     {
    //         isRightGrabbed = true;
    //     }
    // }

    // private void OnLeftRelease(SelectExitEventArgs args)
    // {
    //     if (args.interactorObject.handedness == InteractorHandedness.Left)
    //     {
    //         isLeftGrabbed = false;
    //     }
    // }

    // private void OnRightRelease(SelectExitEventArgs args)
    // {
    //     if (args.interactorObject.handedness == InteractorHandedness.Right)
    //     {
    //         isRightGrabbed = false;
    //     }
    // }
//========================================================================================================================================

    private void OnLeftGrab(SelectEnterEventArgs args)
    {
        if (args.interactorObject.handedness == InteractorHandedness.Left)
        {
            isLeftGrabbed = true;
            activeDrawingHand = XRNode.LeftHand;
        }
    }

    private void OnRightGrab(SelectEnterEventArgs args)
    {
        if (args.interactorObject.handedness == InteractorHandedness.Right)
        {
            isRightGrabbed = true;
            activeDrawingHand = XRNode.RightHand;
        }
    }

        private void OnLeftRelease(SelectExitEventArgs args)
    {
        if (args.interactorObject.handedness == InteractorHandedness.Left)
        {
            isLeftGrabbed = false;
        }
    }

    private void OnRightRelease(SelectExitEventArgs args)
    {
        if (args.interactorObject.handedness == InteractorHandedness.Right)
        {
            isRightGrabbed = false;
        }
    }


    private bool IsLeftTriggerPressed(UnityEngine.XR.InputDevice leftController)
    {
        // return controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue) && triggerValue > 0.1f;
        bool leftTriggerPressed = leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out LeftTriggerValue) && LeftTriggerValue > 0.0001f;
        // print("triggerPressed = " + triggerPressed);
        print("LeftTriggerValue = " + LeftTriggerValue);
        return leftTriggerPressed;
    }

    private bool IsRightTriggerPressed(UnityEngine.XR.InputDevice rightController)
    {
        // return controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue) && triggerValue > 0.1f;
        bool rightTriggerPressed = rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out RightTriggerValue) && RightTriggerValue > 0.0001f;
        // print("triggerPressed = " + triggerPressed);
        print("RightTriggerValue = " + RightTriggerValue);
        return rightTriggerPressed;
    }



    private bool IsPrimaryButtonPressed(UnityEngine.XR.InputDevice controller)
    {
        return controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool isPressed) && isPressed;
    }
}
