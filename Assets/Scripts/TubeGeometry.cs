using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.MinVR3;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class TubeGeometry : MonoBehaviour
{
    [Tooltip("Used to render the stroke's meshes.")]
    [SerializeField] private Material m_Material;

    [Tooltip("Number of faces around the tube.")]
    [SerializeField] private int m_NFaces;

    [Tooltip("If true, texture wraps twice around the tube, otherwise wraps once.")]
    [SerializeField] private bool m_WrapTwice;

    public enum TextureMode
    {
        RepeatTexture,
        StretchTexture
    }
    [Tooltip("Does the texture repeat as a series of stamps or stretch all the way along the stroke")]
    [SerializeField] private TextureMode m_TexMode;

    // Created dynamically
    private MeshRenderer m_MeshRend;
    private Mesh m_Mesh;
    private List<Vector3> m_Vertices;
    private List<Vector3> m_Normals;
    private List<Color> m_Colors;
    private List<Vector2> m_TexCoords;
    private List<int> m_Indices;
    private float m_LastV;
    private List<float> m_ArcLengths;
    private Vector3 m_LastBrushPosInRoom;

    public bool BrushSizeWorking = true;
    public float SizeScale = 0.01f;


    private void Reset()
    {
#if UNITY_EDITOR
        m_Material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
#endif
        m_NFaces = 8;
        m_WrapTwice = false;
        m_TexMode = TextureMode.RepeatTexture;
    }

    public void Init(Vector3 brushPosRoom, Quaternion brushRotRoom, float penScale, Color brushColor, float pressure)
    {
        m_Vertices = new List<Vector3>();
        m_Normals = new List<Vector3>();
        m_Colors = new List<Color>();
        m_TexCoords = new List<Vector2>();
        m_Indices = new List<int>();
        m_ArcLengths = new List<float>();

        m_MeshRend = gameObject.GetComponent<MeshRenderer>();
        m_MeshRend.material = new Material(m_Material);
        m_Mesh = gameObject.GetComponent<MeshFilter>().mesh;
        m_Mesh.MarkDynamic();

        m_LastV = 0.0f;

        // Make the very first sample have a scale of zero to bring the tube to a point
        //AddSample(brushPosRoom, brushRotRoom, Vector3.zero, brushColor);
        // If the current scale is > 0, then add another sample at the same pos & rot
        //if (brushScaleRoom != Vector3.zero) {
        //    AddSample(brushPosRoom, brushRotRoom, brushScaleRoom, brushColor);
        //}
    }

    public void AddSample(Vector3 brushPosWorld, Quaternion brushRotWorld, float penScale, Color brushColor, float pressure)
    {
        // convert these into the local space of the stroke, which has already been added to the artwork parent
        Vector3 brushPosLocal = this.transform.WorldPointToLocalSpace(brushPosWorld);
        Quaternion brushRotLocal = this.transform.WorldRotationToLocalSpace(brushRotWorld);

        Vector3 brushRightLocal;
        Vector3 brushRightScaledLocal;
        Vector3 brushUpLocal;
        Vector3 brushUpScaledLocal;

        // brushWidthWorld and brushHeightWorld
        if (BrushSizeWorking)
        {
            brushRightLocal = brushRotLocal * Vector3.right;
            brushRightScaledLocal = brushRightLocal * transform.WorldLengthToLocalSpace(0.5f * penScale);
            brushUpLocal = brushRotLocal * Vector3.up;
            brushUpScaledLocal = brushUpLocal * transform.WorldLengthToLocalSpace(0.5f * penScale);
        }
        else
        {
            brushRightLocal = brushRotLocal * Vector3.right;
            brushRightScaledLocal = brushRightLocal * SizeScale;
            brushUpLocal = brushRotLocal * Vector3.up;
            brushUpScaledLocal = brushUpLocal * SizeScale;
        }

        // update arc length and calc v texcoord
        float deltaPos = 0.0f;
        if (m_ArcLengths.Count == 0)
        {
            m_ArcLengths.Add(deltaPos);
        }
        else
        {
            Vector3 delta = (brushPosWorld - m_LastBrushPosInRoom);
            if (delta.sqrMagnitude > 0.0f)
            {
                deltaPos = delta.magnitude;
            }
            m_ArcLengths.Add(m_ArcLengths[m_ArcLengths.Count - 1] + deltaPos);
        }
        m_LastBrushPosInRoom = brushPosWorld;
        float texWidthRoom = 2.0f * Mathf.PI * penScale;
        if (m_WrapTwice)
        {
            texWidthRoom *= 0.5f;
        }
        float v = m_LastV;
        if (texWidthRoom > 0.0f)
        {
            v += deltaPos / texWidthRoom;
        }
        m_LastV = v;


        // add a new ring of vertices and associated data
        float angleStep = Mathf.PI * 2f / m_NFaces;
        for (int j = 0; j <= m_NFaces; j++)
        {

            //=====================================================================================
            // store verts
            // Vector3 thisVert = brushPosLocal + brushRightScaledLocal * Mathf.Cos(a) + brushUpScaledLocal * Mathf.Sin(a);
            // m_Vertices.Add(thisVert);

            // // store normals
            // m_Normals.Add((brushRightLocal * Mathf.Cos(a) + brushUpLocal * Mathf.Sin(a)).normalized);
            //=======================================================================================================
            float a = j * angleStep;

            float x_circle = Mathf.Cos(a);
            float y_circle = Mathf.Sin(a);

            float x_square = Mathf.Sign(x_circle);
            float y_square = Mathf.Sign(y_circle);

            float morphX = Mathf.Lerp(x_square, x_circle, pressure);
            float morphY = Mathf.Lerp(y_square, y_circle, pressure);

            // float morphX = x_circle;
            // float morphY = y_circle;

            Vector3 thisVert = brushPosLocal + brushRightScaledLocal * morphX + brushUpScaledLocal * morphY;

            // if (BrushSizeWorking)
            // {
            //     thisVert = brushPosLocal + brushRightScaledLocal * morphX + brushUpScaledLocal * morphY;
            // }
            // else
            // {
            //     thisVert = brushPosLocal + Vector3.right * SizeScale * morphX + Vector3.up * SizeScale * morphY;
            // }

            m_Vertices.Add(thisVert);

            Vector3 normal = (brushRightLocal * morphX + brushUpLocal * morphY).normalized;
            m_Normals.Add(normal);

            // store colors
            m_Colors.Add(brushColor);

            // store tex coords
            float u;
            if (m_WrapTwice)
            {
                // To wrap twice around the tube
                int halfverts = Mathf.RoundToInt((float)m_NFaces / 2.0f);
                if (j < halfverts)
                    u = (float)j / (float)halfverts;
                else
                    u = 1.0f - (float)(j - halfverts) / (float)halfverts;
            }
            else
            {
                // to wrap once around the tube
                u = (float)j / (float)m_NFaces;
            }
            m_TexCoords.Add(new Vector2(u, v));

            // update angle
            a += 2.0f * Mathf.PI / (float)m_NFaces;
        }

        // add triangles connecting this ring to the previous
        if (m_ArcLengths.Count > 1)
        {
            int iStartOfLastRing = m_Vertices.Count - 2 * (m_NFaces + 1);
            int iStartOfThisRing = m_Vertices.Count - (m_NFaces + 1);

            for (int f = 0; f < m_NFaces; f++)
            {
                int lastV0 = iStartOfLastRing + f;
                int lastV1 = iStartOfLastRing + f + 1;

                int thisV0 = iStartOfThisRing + f;
                int thisV1 = iStartOfThisRing + f + 1;

                // tri #1
                m_Indices.Add(lastV0);
                m_Indices.Add(thisV1);
                m_Indices.Add(thisV0);

                // tri #1 backside
                m_Indices.Add(lastV0);
                m_Indices.Add(thisV0);
                m_Indices.Add(thisV1);

                // tri #2
                m_Indices.Add(lastV1);
                m_Indices.Add(thisV1);
                m_Indices.Add(lastV0);

                // tri #2 backside
                m_Indices.Add(lastV1);
                m_Indices.Add(lastV0);
                m_Indices.Add(thisV1);
            }
        }


        // if stretching the texture, the v tex coords across the whole length of the tube need to be updated
        if (m_TexMode == TextureMode.StretchTexture)
        {
            for (int n = 0; n < m_ArcLengths.Count; n++)
            {
                float vNew = m_ArcLengths[n] / m_ArcLengths[m_ArcLengths.Count - 1];
                int iStartOfRingN = n * (m_NFaces + 1);
                for (int f = 0; f <= m_NFaces; f++)
                {
                    int i = iStartOfRingN + f;
                    m_TexCoords[i] = new Vector2(m_TexCoords[i].x, vNew);
                }
            }
        }

        // update the mesh
        m_Mesh.Clear();
        m_Mesh.SetVertices(m_Vertices);
        m_Mesh.SetNormals(m_Normals);
        m_Mesh.SetColors(m_Colors);
        m_Mesh.SetUVs(0, m_TexCoords);
        m_Mesh.SetIndices(m_Indices, MeshTopology.Triangles, 0);
    }


    public void Complete(Vector3 brushPosRoom, Quaternion brushRotRoom, float brushWidthRoom, float brushHeightRoom, Color brushColor, float pressure)
    {
        // If the current scale is > 0, then add a sample
        //if (brushScaleRoom != Vector3.zero) {
        //    AddSample(brushPosRoom, brushRotRoom, brushScaleRoom, brushColor);
        //}
        // Make sure the very first last sample has a scale of zero to bring the tube to a point
        //AddSample(brushPosRoom, brushRotRoom, Vector3.zero, brushColor);


        // TODO: When the meshes are created we use the MarkDynamic() call to tell Unity they will change dynamically.
        // Not sure if there is a way to now tell Unity that we are done changing them dynamically.  If so, it
        // might provide some rendering performance benefits.  For now, leaving as is.
    }

    public void SetMaterial(Material mat)
    {
        m_Material = mat;
        if (m_MeshRend != null)
        {
            m_MeshRend.material = mat;
        }
    }

    public void SetNumFaces(int n)
    {
        m_NFaces = n;

        // TODO: We could save the data needed to regenerate the mesh if we end up needing that functionality, but for
        // now there is no way to regenerate the mesh if this parameter changes.  So, this function should only be
        // called before the mesh has been created.
        Debug.Assert(m_ArcLengths == null || m_ArcLengths.Count == 0);
    }

    public void SetWrapTwice(bool twice)
    {
        m_WrapTwice = twice;

        // TODO: We could save the data needed to regenerate the mesh if we end up needing that functionality, but for
        // now there is no way to regenerate the mesh if this parameter changes.  So, this function should only be
        // called before the mesh has been created.
        Debug.Assert(m_ArcLengths == null || m_ArcLengths.Count == 0);
    }

    public void SetTexMode(TextureMode texMode)
    {
        m_TexMode = texMode;

        // TODO: We could save the data needed to regenerate the mesh if we end up needing that functionality, but for
        // now there is no way to regenerate the mesh if this parameter changes.  So, this function should only be
        // called before the mesh has been created.
        Debug.Assert(m_ArcLengths == null || m_ArcLengths.Count == 0);
    }
}