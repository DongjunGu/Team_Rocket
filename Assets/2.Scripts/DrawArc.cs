using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawArc : MonoBehaviour
{
    [Header("총구")]
    public Transform firePoint;

    [Header("부채꼴 inner 설정")]
    public float radius = 3f;
    public float angle = 15f;
    public int segments = 20;
    public Color color;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        meshRenderer.material = mat;

        mesh = new Mesh();
        mesh.name = "ArcMesh";
        meshFilter.mesh = mesh;

        CreateArc();
    }

    void Update()
    {
        if (firePoint == null) return;

        transform.position = firePoint.position;
        transform.rotation = firePoint.rotation;
        CreateArc();
    }

    /// <summary>
    /// 부채꼴 채우기
    /// </summary>
    void CreateArc()
    {
        mesh.Clear();

        float halfAngle = angle / 2f;
        float step = angle / segments;

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -halfAngle + step * i;
            float rad = Mathf.Deg2Rad * currentAngle;
            vertices[i + 1] = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
