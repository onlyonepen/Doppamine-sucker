using System;
using UnityEngine;
using VInspector;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject enemy;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) SummonEnemy();
    }

    [Button]
    public void SummonEnemy()
    {
        Instantiate(enemy);
    }
}
