using System;
using UnityEngine;
using UniverseEngine;

public class BhvApplicationQuit : MonoBehaviour
{
	private void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}
	private void OnApplicationQuit()
	{

	}
}
