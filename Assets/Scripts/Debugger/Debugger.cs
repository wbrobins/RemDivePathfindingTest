using System.Collections;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    [SerializeField] private GameObject enemyToSpawn;
    [SerializeField] private GameObject player;
    [SerializeField] private float spawnRange = 10f;
    private PlayerController playerController;
    private bool spawning = false;

    void Start()
    {
        player = GameObject.Find("Player");
        playerController = player.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) && !spawning)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    IEnumerator SpawnRoutine()
    {
        spawning = true;

        Transform cameraTransform = playerController.GetCamera().transform;
        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;

        Debug.DrawRay(origin, direction * spawnRange, Color.blue, 1f);

        Vector3 spawnPosition;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, spawnRange))
        {
            spawnPosition = hit.point;
        }
        else
        {
            spawnPosition = origin + direction * spawnRange;
        }

        GameObject enemy = Instantiate(enemyToSpawn);
        enemy.transform.position = spawnPosition;

        yield return new WaitForSeconds(1);

        spawning = false;
    }
}