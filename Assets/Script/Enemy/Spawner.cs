using System;
using UnityEngine;
using VInspector;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject LightDrone;
    [SerializeField] private GameObject HeavyDrone;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) SummonLightDrone();
        if (Input.GetKeyDown(KeyCode.H)) SummonHeavyDrone();
    }

    public void SummonLightDrone()
    {
        Instantiate(LightDrone);
    }
    public void SummonHeavyDrone()
    {
        Instantiate(HeavyDrone);
    }
}
