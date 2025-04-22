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


    [Header("XR Interaction")]
    public XRGrabInteractable grabInteractable;
    public XRDirectInteractor leftHandInteractor;   // Assign Left Hand XR Direct Interactor
    public XRDirectInteractor rightHandInteractor;  // Assign Right Hand XR Direct Interactor

    private LineRenderer currentDrawing;
    private int index;
    private int currentColorIndex;
    private bool isGrabbed = false;
    private UnityEngine.XR.InputDevice leftController;
    private UnityEngine.XR.InputDevice rightController;

    private float triggerValue;


    private void Start()
    {
        StartCoroutine(InitCouroutine());
    }


    IEnumerator InitCouroutine()
    {
        yield return new WaitForSeconds(1f);

        currentColorIndex = 0;
        tipMaterial.color = penColors[currentColorIndex];

        // Subscribe to grab events
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }

                // Get input devices for left and right controllers
        leftController = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    private void Update()
    {
        bool isRightHandDrawing = isGrabbed && IsTriggerPressed(rightController);
        bool isLeftHandDrawing = isGrabbed && IsTriggerPressed(leftController);

        if (isRightHandDrawing || isLeftHandDrawing)
        {
            Draw();
        }
        else if (currentDrawing != null)
        {
            currentDrawing = null;
        }

        // Switch color if button is pressed
        if (IsPrimaryButtonPressed(rightController) || IsPrimaryButtonPressed(leftController))
        {
            SwitchColor();
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    private void Draw()
    { /*
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

        GameObject drawnCube = Instantiate(cubeToDraw);
        
        float minScale = 0.010f;
        float maxScale = 0.050f;
        float scaleValue = Mathf.Lerp(minScale, maxScale, triggerValue);

        drawnCube.transform.localScale = Vector3.one * scaleValue;
        drawnCube.transform.rotation = tip.rotation;
        drawnCube.transform.position = tip.position;

        // float minScale = 0.001f;
        // float maxScale = 1.010f;
        // float uniformScale =Mathf.Lerp(minScale, maxScale, triggerValue);

        

        GameObject drawnSphere = Instantiate(sphereToDraw);
        //drawnSphere.transform.localScale = Vector3.one * scaleValue;
        drawnSphere.transform.position = tip.position;
        drawnSphere.transform.rotation = tip.rotation;

    }

    private void SwitchColor()
    {
        currentColorIndex = (currentColorIndex + 1) % penColors.Length;
        tipMaterial.color = penColors[currentColorIndex];
    }

    private bool IsTriggerPressed(UnityEngine.XR.InputDevice controller)
    {
        // return controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue) && triggerValue > 0.1f;
        bool triggerPressed =  controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue) && triggerValue > 0.001f;
        // print("triggerPressed = " + triggerPressed);
        print("triggerValue = " + triggerValue);
        return triggerPressed;
    }

    private bool IsPrimaryButtonPressed(UnityEngine.XR.InputDevice controller)
    {
        return controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool isPressed) && isPressed;
    }
}
