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
    [SerializeField] private bool BrushSizeWorking = true;
    // [Range(0.001f, 0.1f)] [SerializeField] private float SizeScale = 0.01f;

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

    // Remember: LEFT hand is Giselle's drawing hand!!! XD :P ^_^ T^T :v 
    public GameObject CreateStroke(Transform drawing, Material drawingMaterial, Transform tip, float morphTriggerValue, float sizeTriggerValue)
    {
        s_Num++;
        GameObject go = new GameObject("Tube Stroke " + s_Num, typeof(TubeGeometry));
        go.transform.SetParent(drawing.transform, false);

        TubeGeometry tube = go.GetComponent<TubeGeometry>();
        tube.SetMaterial(drawingMaterial);
        tube.SetNumFaces(m_NFaces);
        tube.SetWrapTwice(m_WrapTwice);
        tube.SetTexMode(m_TexMode);
        tube.BrushSizeWorking = BrushSizeWorking;
        // tube.SizeScale = SizeScale;
        float penScale = sizeTriggerValue/10f;
        tube.Init();
        return go;
    }

    public void AddSampleToStroke(GameObject strokeObject, Material drawingMaterial, Transform tip, float morphTriggerValue, float sizeTriggerValue)
    {
        TubeGeometry tube = strokeObject.GetComponent<TubeGeometry>();
        Debug.Assert(tube != null);
        float penScale = sizeTriggerValue/10f;
    
        tube.AddSample(tip.position, tip.rotation, penScale, drawingMaterial.color, morphTriggerValue, sizeTriggerValue);
    }

    public void CompleteStroke(GameObject strokeObject, Material drawingMaterial, Transform tip, float morphTriggerValue, float sizeTriggerValue)
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

        //float penScale = sizeTriggerValue/10f;
        // float widthRoom = Brush.SizeToMeters(brushState.size) * brushState.aspect.x * brushState.pressure;
        // float heightRoom = Brush.SizeToMeters(brushState.size) * brushState.aspect.y * brushState.pressure;
        // heightRoom /= m_AspectRatio; // take tube's inherent aspect ratio into account as well
        tube.Complete();
    }
}