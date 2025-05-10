using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PenHandSwitcher : MonoBehaviour
{
    // Member variables
    [SerializeField] private Pen pen;
    [SerializeField] private GameObject rightController;
    [SerializeField] private GameObject leftController;

    public enum ActiveDrawingHand
    {
        LeftHand,
        RightHand
    }

    public ActiveDrawingHand CurrentActiveDrawingHand = ActiveDrawingHand.LeftHand;

    private bool isLeftSwitchButtonDown;
    private bool isRightSwitchButtonDown;


    void AttachPenToHand(GameObject controller)
    {
        // left
        // pos = 0.0129, 0.0389, -0.0313
        // rot = 0.97, 1.2, -32.167

        // right
        // pos = -0.013, 0.03, -0.03
        // rot = -4.3, 11.88, 46.787

        pen.transform.SetParent(controller.transform, false);

        if (controller == leftController)
        {
            pen.transform.localPosition = new Vector3(0.013f, 0.03f, -0.03f);
            pen.transform.localRotation = Quaternion.Euler(0.97f, 1.2f, -32.167f);
            CurrentActiveDrawingHand = ActiveDrawingHand.LeftHand;
        }
        else if (controller == rightController)
        {
            pen.transform.localPosition = new Vector3(-0.013f, 0.03f, -0.03f);
            pen.transform.localRotation = Quaternion.Euler(-4.3f, 11.88f, 46.787f);
            CurrentActiveDrawingHand = ActiveDrawingHand.RightHand;
        }
    }

    void GetControllerButtons()
    {  
        UnityEngine.XR.InputDevice rightControllerXR = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        rightControllerXR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out isRightSwitchButtonDown);
        UnityEngine.XR.InputDevice leftControllerXR = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        leftControllerXR.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out isLeftSwitchButtonDown);
    }


    void Update()
    {
        GetControllerButtons();

        if (isLeftSwitchButtonDown)
        {
            AttachPenToHand(leftController);
        }

        else if (isRightSwitchButtonDown)
        {
            AttachPenToHand(rightController);
        }
    }
}
