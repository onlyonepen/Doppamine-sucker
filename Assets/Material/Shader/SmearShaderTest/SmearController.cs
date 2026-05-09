using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SmearController : MonoBehaviour
{
    [SerializeField] private Material mat;
    [SerializeField] private int frameDelay = 3;

    private Queue<Vector3> positionHistory = new Queue<Vector3>();

    private void Awake()
    {
        mat = new Material(mat);
        GetComponent<Renderer>().material = mat;
    }

    void Update()
    {
        positionHistory.Enqueue(transform.position);

        if (positionHistory.Count > frameDelay)
        {
            Vector3 prevPosition = positionHistory.Dequeue();
            mat.SetVector("_PreviousPos", prevPosition);
        }
    }
}

