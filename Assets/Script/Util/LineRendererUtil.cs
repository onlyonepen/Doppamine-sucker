using System;
using UnityEngine;
using VInspector;

[ExecuteAlways]
public class LineRendererUtil : MonoBehaviour
{
    [SerializeField] private Transform[] positions;
    [Button]
    public void AllignLr()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = positions.Length;
        for (int i = 0; i < positions.Length; i++)
        {
            lr.SetPosition(i, positions[i].position);
        }
    }
    

    private void Update()
    {
        AllignLr();
    }
}
