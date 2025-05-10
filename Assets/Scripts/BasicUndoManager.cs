using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class BasicUndoManager : MonoBehaviour
{
    public GameObject drawing;

    private InputDevice rightController;
    private bool isUndoButtonDown;
    private bool firstTimeUndoButtonDown = true;


    void Start()
    {
        // rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        // print(rightController);
    }

    void Update()
    {
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out isUndoButtonDown);

        if (isUndoButtonDown)
        {
            print("firstTimeUndoButtonDown=" + firstTimeUndoButtonDown);
            if (firstTimeUndoButtonDown)
            {
                Undo();
                firstTimeUndoButtonDown = false;
            }
        }
        // Release the undo button
        else   
        {
            print("on button release firstTimeUndoButtonDown=" + firstTimeUndoButtonDown);
            firstTimeUndoButtonDown = true;
        }
    }

    void Undo()
    {
        if (drawing.transform.childCount > 0)
        {
            int indexOfLastBruhsStrokeInDrawing = drawing.transform.childCount - 1;
            Transform lastBruhsStrokeInDrawing = drawing.transform.GetChild(indexOfLastBruhsStrokeInDrawing);
            Destroy(lastBruhsStrokeInDrawing.gameObject);
        }
    }
}
