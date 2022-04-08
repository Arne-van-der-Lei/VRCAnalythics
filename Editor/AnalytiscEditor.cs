using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using VRC.SDKBase;

public class AnalytiscEditor : EditorWindow
{
    /// <summary>
    /// The textAsset for the analythics
    /// </summary>
    private TextAsset analyticsFile;
    /// <summary>
    /// The offset for the Visualization 
    /// </summary>
    private Vector3 offset;
    /// <summary>
    /// The scaling the visualization needs to use
    /// </summary>
    private Vector3 scale = Vector3.one;
    /// <summary>
    /// The lower treshold bounds in %
    /// </summary>
    private float lowerTreshold = 0.01f;

    /// <summary>
    /// The list of processed elements 
    /// </summary>
    private List<Data> elements = new List<Data>();
    /// <summary>
    /// The highes value the processed elements go to
    /// </summary>
    private int maxAmount;

    /// <summary>
    /// object we render to
    /// </summary>
    private GameObject hiddenRenderObject;
    /// <summary>
    /// object we render to
    /// </summary>
    private MeshFilter hiddenRenderMeshFilter;

    /// <summary>
    /// internal data refrence for after processing
    /// </summary>
    public struct Data
    {
        /// <summary>
        /// position of the information
        /// </summary>
        public Vector3Int pos;
        /// <summary>
        /// amount of events in this position
        /// </summary>
        public int amount;
    }

    /// <summary>
    /// Initilization of the analythics
    /// </summary>
    [MenuItem("Analytics/Analytics")]
    static void Init()
    {
        AnalytiscEditor window = (AnalytiscEditor)EditorWindow.GetWindow(typeof(AnalytiscEditor));
        window.Show();
    }

    /// <summary>
    /// Main event for showing data 
    /// </summary>
    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        analyticsFile = (TextAsset)EditorGUILayout.ObjectField("Analytics file",analyticsFile,typeof(TextAsset),false);
        offset = EditorGUILayout.Vector3Field("Offset", offset);
        scale = EditorGUILayout.Vector3Field("Scale", scale);
        lowerTreshold = EditorGUILayout.FloatField("treshold", lowerTreshold);


        if (analyticsFile == null) return;

        if (GUILayout.Button("Read data"))
        {
            AnalyticsElement[] AnaElements = JsonConvert.DeserializeObject<AnalyticsElement[]>(analyticsFile.text);
            elements.Clear();
            maxAmount = 0;

            Mesh cubemesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            List<CombineInstance> instances = new List<CombineInstance>();
            Mesh combineMesh = hiddenRenderMeshFilter.sharedMesh;

            if(combineMesh == null)
            {
                combineMesh = new Mesh();
            }
            combineMesh.Clear();

            

            for (int i = 0; i < AnaElements.Length; i++)
            {
                Vector3Int internalPos = Vector3Int.RoundToInt(Vector3.Scale(AnaElements[i].position ,new Vector3(1/scale.x, 1 / scale.y, 1 / scale.z)) - offset);
                int index = elements.FindIndex((o) => o.pos == internalPos);
                if (index == -1)
                {
                    elements.Add(new Data { amount = 1, pos = internalPos });
                }
                else
                {
                    Data data = elements[index];
                    data.amount++;

                    if (maxAmount < data.amount)
                        maxAmount = data.amount;

                    elements[index] = data;
                }
            }

            for(int i = 0; i < elements.Count; i++)
            {
                float amount = (float)elements[i].amount / maxAmount;
                if (amount <= lowerTreshold) continue;
                Mesh mesh = new Mesh();

                Vector2[] uvs = cubemesh.uv;

                for(int j = 0; j < uvs.Length; j++)
                {
                    uvs[j] = new Vector2(amount, 0);
                }
                mesh.vertices = cubemesh.vertices;
                mesh.triangles = cubemesh.triangles;
                mesh.uv = uvs;

                instances.Add(new CombineInstance
                {
                    mesh = mesh,
                    transform = Matrix4x4.Translate(Vector3.Scale(elements[i].pos, scale) + offset + Vector3.up * scale.y * 0.5f * amount) * Matrix4x4.Scale(new Vector3(0.5f * scale.x, amount * scale.y, 0.5f * scale.z)),
                    subMeshIndex = 0
                });
            }

            combineMesh.CombineMeshes(instances.ToArray());
            hiddenRenderMeshFilter.sharedMesh = combineMesh;

            hiddenRenderObject.SetActive(true);
        }
    }

    /// <summary>
    /// event when editor gets enabled
    /// </summary>
    void OnEnable()
    {
        if (!Utilities.IsValid(hiddenRenderObject))
        {
            hiddenRenderObject = GameObject.Find("AnalyticsObject");

            if (!Utilities.IsValid(hiddenRenderObject))
            {
                hiddenRenderObject = new GameObject();
                hiddenRenderObject.name = "AnalyticsObject";
                hiddenRenderObject.hideFlags = HideFlags.HideAndDontSave;
                hiddenRenderMeshFilter = hiddenRenderObject.AddComponent<MeshFilter>();
                MeshRenderer r = hiddenRenderObject.AddComponent<MeshRenderer>();
                r.sharedMaterial = new Material(Shader.Find("Unlit/Anlytics"));
            }
            hiddenRenderObject.SetActive(false);
        }
    }

    /// <summary>
    /// event when editor gets disabled
    /// </summary>
    void OnDisable()
    {
        hiddenRenderObject.SetActive(false);
    }

}
