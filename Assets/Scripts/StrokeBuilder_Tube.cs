using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// tube connects w/ prev tube  ?? maybe related to the caps?
/// xyz on tube or brushmodel are off -- cursors may help to debug
/// brush resizing doesn't work
/// palette is too big
/// 
/// </summary>
public class StrokeBuilder_Tube : MonoBehaviour
{
    static int s_Num = 0;

    [Tooltip("Does the texture repeat as a series of stamps or stretch all the way along the stroke")]
    [SerializeField] private TubeGeometry.TextureMode m_TexMode;

    [Tooltip("Aspect ratio: tube width / tube height.")]
    [SerializeField] private float m_AspectRatio;

    [Tooltip("Number of faces around the tube.")]
    [SerializeField] private int m_NFaces;

    [Tooltip("If true, texture wraps twice around the tube, otherwise wraps once.")]
    [SerializeField] private bool m_WrapTwice;

    public TubeGeometry.TextureMode TexMode 
    { 
        get => m_TexMode;
        set => m_TexMode = value;
    }


    void Reset()
    {
        m_NFaces = 6;
        m_AspectRatio = 1.0f;
        m_WrapTwice = false;
        m_TexMode = TubeGeometry.TextureMode.RepeatTexture;
    }

    public GameObject CreateStroke(Transform drawing, Material drawingMaterial, Transform tip, float triggerValue)
    {
        s_Num++;
        GameObject go = new GameObject("Tube Stroke " + s_Num, typeof(TubeGeometry));
        go.transform.SetParent(drawing.transform, false);

        TubeGeometry tube = go.GetComponent<TubeGeometry>();
        tube.SetMaterial(drawingMaterial);
        tube.SetNumFaces(m_NFaces);
        tube.SetWrapTwice(m_WrapTwice);
        tube.SetTexMode(m_TexMode);
        float penScale = triggerValue/10f;
        tube.Init(tip.position, tip.rotation, penScale, penScale, drawingMaterial.color);
        return go;
    }

    public void AddSampleToStroke(GameObject strokeObject, Material drawingMaterial, Transform tip, float triggerValue)
    {
        TubeGeometry tube = strokeObject.GetComponent<TubeGeometry>();
        Debug.Assert(tube != null);
        float penScale = triggerValue/10f;
        tube.AddSample(tip.position, tip.rotation, penScale, penScale, drawingMaterial.color);
    }

    public void CompleteStroke(GameObject strokeObject, Material drawingMaterial, Transform tip, float triggerValue)
    {
        TubeGeometry tube = strokeObject.GetComponent<TubeGeometry>();
        
        // -----
        Debug.Assert(tube != null);
        // -----
        // if (tube == null)
        // {
        //     Debug.Log("tube is null");
        // }
        // -----

        float penScale = triggerValue/10f;
        // float widthRoom = Brush.SizeToMeters(brushState.size) * brushState.aspect.x * brushState.pressure;
        // float heightRoom = Brush.SizeToMeters(brushState.size) * brushState.aspect.y * brushState.pressure;
        // heightRoom /= m_AspectRatio; // take tube's inherent aspect ratio into account as well
        tube.Complete(tip.position, tip.rotation, penScale, penScale, drawingMaterial.color);
    }
}