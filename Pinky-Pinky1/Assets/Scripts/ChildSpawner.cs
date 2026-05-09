using System.Collections;
using UnityEngine;

public class ChildSpawner : MonoBehaviour
{
    [SerializeField] private GameObject normchild;
    [SerializeField] private float spawnInterval = 3f;

    [SerializeField] private Transform spawnPoint;   // assign in Inspector

    void Start()
    {
        StartCoroutine(SpawnChild());
    }

    IEnumerator SpawnChild()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            Instantiate(normchild, spawnPoint.position, Quaternion.identity);
        }
    }
}
