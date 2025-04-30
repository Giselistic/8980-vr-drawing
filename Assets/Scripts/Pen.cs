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
    [Range(0.01f, 0.1f)]
    public float penWidth = 0.01f;
    public Color[] penColors;
    public GameObject cubeToDraw;
    public GameObject sphereToDraw;

    [Header("Drawing")]
    public StrokeBuilder_Tube giselistiStrokeBuilder;
    public Transform drawingTransform;


    [Header("XR Interaction")]
    public XRGrabInteractable grabInteractable;
    public XRDirectInteractor leftHandInteractor;   // Assign Left Hand XR Direct Interactor
    public XRDirectInteractor rightHandInteractor;  // Assign Right Hand XR Direct Interactor

    private LineRenderer currentDrawing;
    private int index;
    private int currentColorIndex;
    private bool isLeftGrabbed = false;
    private bool isRightGrabbed = false;
    private UnityEngine.XR.InputDevice leftController;
    private UnityEngine.XR.InputDevice rightController;

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
        }

                // Get input devices for left and right controllers
        leftController = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        bool isRightHandDrawing = isRightGrabbed && IsRightTriggerPressed(rightController);
        bool isLeftHandDrawing = isLeftGrabbed && IsLeftTriggerPressed(leftController);

        if (isRightHandDrawing || isLeftHandDrawing)
        {
            if (firstTimePressingLeftTrigger)
            {
                currentStrokeGameObject = giselistiStrokeBuilder.CreateStroke(drawingTransform, drawingMaterial, tip, LeftTriggerValue);
                firstTimePressingLeftTrigger = false;
            }
            else  // If trigger is still held down
            {
                giselistiStrokeBuilder.AddSampleToStroke(currentStrokeGameObject,drawingMaterial, tip, LeftTriggerValue);
            }
            // Draw();
        }
        else if (currentDrawing != null)
        {
            currentDrawing = null;
        }
        else  // If trigger is released
        {
            if (currentStrokeGameObject != null)
            {
                giselistiStrokeBuilder.CompleteStroke(currentStrokeGameObject, drawingMaterial, tip, LeftTriggerValue);
                firstTimePressingLeftTrigger = true;
            }
        }

        // Switch color if button is pressed
        // if (IsPrimaryButtonPressed(rightController) || IsPrimaryButtonPressed(leftController))
        // {
        //     SwitchColor();
        // }
    }

    private void OnLeftGrab(SelectEnterEventArgs args)
    {
        // Check if left or right hand is grabbing with args
        isLeftGrabbed = true;
    }

    private void OnRightGrab(SelectEnterEventArgs args)
    {
        isRightGrabbed = true;
    }

    private void OnLeftRelease(SelectExitEventArgs args)
    {
        isLeftGrabbed = false;
    }

    private void OnRightRelease(SelectExitEventArgs args)
    {
        isRightGrabbed = false;
    }

    private void Draw()
    { 
        //  Line Renderer Transparency concept
        /*
        if (currentDrawing == null)
        {
            index = 0;
            currentDrawing = new GameObject("DrawingLine").AddComponent<LineRenderer>();
            currentDrawing.useWorldSpace = false;
            currentDrawing.material = drawingMaterial;
            currentDrawing.startColor = currentDrawing.endColor = penColors[currentColorIndex];
            currentDrawing.startWidth = currentDrawing.endWidth = penWidth;
            currentDrawing.positionCount = 1;
            currentDrawing.SetPosition(0, tip.position);
        }
        else
        {
            var currentPos = currentDrawing.GetPosition(index);
            if (Vector3.Distance(currentPos, tip.position) > 0.01f)
            {
                index++;
                currentDrawing.positionCount = index + 1;
                currentDrawing.SetPosition(index, tip.position);
                currentDrawing.startWidth = currentDrawing.endWidth = penWidth;

                // // 11 / 3 = 3 -- remainder = 2 
                // // % means modulus
                // // 11 % 3 = 2
                // // 10 % 3 = 1

                // index = index % penColors.Length;

            }
        }

        penWidth = triggerValue / 100f;
        */

        // Instantiated 3d gameobjects--------------------------------------------------------------------

        // GameObject drawnCube = Instantiate(cubeToDraw);
        
        // float minScale = 0.010f;
        // float maxScale = 0.050f;
        // float scaleValue = Mathf.Lerp(minScale, maxScale, triggerValue);

        // drawnCube.transform.localScale = Vector3.one * scaleValue;
        // drawnCube.transform.rotation = tip.rotation;
        // drawnCube.transform.position = tip.position;

        // // float minScale = 0.001f;
        // // float maxScale = 1.010f;
        // // float uniformScale =Mathf.Lerp(minScale, maxScale, triggerValue);

        

        // GameObject drawnSphere = Instantiate(sphereToDraw);
        // //drawnSphere.transform.localScale = Vector3.one * scaleValue;
        // drawnSphere.transform.position = tip.position;
        // drawnSphere.transform.rotation = tip.rotation;
        //---------------------------------------------------------------------------------------------------

        // GameObject strokeGameObject = giselistiStrokeBuilder.CreateStroke(drawingTransform, drawingMaterial, tip, LeftTriggerValue);
        // giselistiStrokeBuilder.AddSampleToStroke(strokeGameObject,drawingMaterial, tip, LeftTriggerValue);
        // giselistiStrokeBuilder.CompleteStroke(strokeGameObject, drawingMaterial, tip, LeftTriggerValue);
    }

    // private void SwitchColor()
    // {
    //     currentColorIndex = (currentColorIndex + 1) % penColors.Length;
    //     tipMaterial.color = penColors[currentColorIndex];
    // }

    private bool IsLeftTriggerPressed(UnityEngine.XR.InputDevice leftController)
    {
        // return controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue) && triggerValue > 0.1f;
        bool leftTriggerPressed =  leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out LeftTriggerValue) && LeftTriggerValue > 0.0001f;
        // print("triggerPressed = " + triggerPressed);
        print("triggerValue = " + LeftTriggerValue);
        return leftTriggerPressed;
    }

        private bool IsRightTriggerPressed(UnityEngine.XR.InputDevice rightController)
    {
        // return controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue) && triggerValue > 0.1f;
        bool rightTriggerPressed =  rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out RightTriggerValue) && RightTriggerValue > 0.0001f;
        // print("triggerPressed = " + triggerPressed);
        print("triggerValue = " + RightTriggerValue);
        return rightTriggerPressed;
    }



    private bool IsPrimaryButtonPressed(UnityEngine.XR.InputDevice controller)
    {
        return controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool isPressed) && isPressed;
    }
}
