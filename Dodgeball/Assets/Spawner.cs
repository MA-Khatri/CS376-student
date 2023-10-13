using UnityEngine;

/// <summary>
/// Periodically spawns the specified prefab in a random location.
/// </summary>
public class Spawner : MonoBehaviour
{
    /// <summary>
    /// Object to spawn
    /// </summary>
    public GameObject Prefab;

    /// <summary>
    /// Seconds between spawn operations
    /// </summary>
    public float SpawnInterval = 20;

    /// <summary>
    /// Tracks the time when the next enemy should spawn
    /// </summary>
    private float timeForNextSpawn = 0;

    /// <summary>
    /// How many units of free space to try to find around the spawned object
    /// </summary>
    public float FreeRadius = 10;

    /// <summary>
    /// Check if we need to spawn and if so, do so.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    void Update()
    {
        if (Time.time > timeForNextSpawn)
        {
            Instantiate(Prefab, SpawnUtilities.RandomFreePoint(FreeRadius), Quaternion.identity);
            timeForNextSpawn = Time.time + SpawnInterval;
        }
    }
}
