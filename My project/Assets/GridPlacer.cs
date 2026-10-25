using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    [System.Serializable]
    public class SpawnOption
    {
        public GameObject prefab;
        public float weight = 1f;                     // higher = more common
        public Vector2Int footprint = Vector2Int.one; // how many cells it covers
    }
    [Header("Blocked Areas")]
    [Tooltip("Rectangles of cells where nothing spawns. X/Y = first cell, W/H = size in cells")]
    public Transform[] blockedAreas;
    [Tooltip("Colliders on these layers block any cell they touch")]
    public LayerMask blockerMask;
    [Header("Grid")]
    public int width = 20;
    public int depth = 20;
    public float cellSize = 2f;

    [Header("Randomness")]
    public int seed = 123;
    [Range(0f, 1f)] public float fillChance = 0.3f;

    [Header("Noise")]
    public bool useNoise = true;
    [Tooltip("Smaller = bigger, smoother patches")]
    public float noiseScale = 0.1f;
    [Range(0f, 1f)] public float noiseThreshold = 0.45f;

    [Header("Variation")]
    [Tooltip("How far from the cell center an object can move, as a fraction of cell size")]
    [Range(0f, 0.5f)] public float jitter = 0.3f;
    public bool randomRotation = true;
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f);   // x = min, y = max

    [Header("Ground")]                                                  // STEP 8
    public bool snapToGround = true;                                    // STEP 8
    public LayerMask groundMask;            // set to "Ground" in the Inspector!  STEP 8
    public float raycastHeight = 100f;                                  // STEP 8
    [Range(0f, 90f)] public float maxSlope = 35f;                       // STEP 8
    public bool alignToSlope = false;                                   // STEP 8
    [Tooltip("Lift each object so the bottom of its mesh sits on the ground (fixes center-pivot prefabs like Unity's Cube)")]
    public bool liftToSurface = true;                                   // STEP 8

    [Header("Objects")]
    public SpawnOption[] options;

    System.Random rng;
    bool[,] occupied;
    Vector2 noiseOffset;

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        if (options == null || options.Length == 0)
        {
            Debug.LogWarning("GridPlacer: add at least one option in the Inspector.");
            return;
        }

        if (snapToGround && groundMask == 0)                            // STEP 8
            Debug.LogWarning("GridPlacer: Ground Mask is set to Nothing, so nothing will spawn.");

        rng = new System.Random(seed);
        occupied = new bool[width, depth];
        UpdateNoiseOffset();
        // Mark manually blocked cells as taken before anything spawns
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                if (IsManuallyBlocked(x, z)) occupied[x, z] = true;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (occupied[x, z]) continue;                             // cell already taken
                if (rng.NextDouble() > fillChance) continue;              // random skip
                if (useNoise && NoiseAt(x, z) < noiseThreshold) continue; // outside a noise patch

                SpawnOption option = PickOption();
                if (!Fits(x, z, option.footprint)) continue;              // would overlap or go off the grid

                Vector3 pos = CellToWorld(x, z, option.footprint);

                // Jitter: move up to (jitter × cellSize) in any direction
                float j = jitter * cellSize;
                pos += new Vector3(RandomRange(-j, j), 0f, RandomRange(-j, j));

                // STEP 8: find the ground under this spot
                Vector3 groundNormal = Vector3.up;
                if (snapToGround)
                {
                    Vector3 rayStart = pos + Vector3.up * raycastHeight;
                    if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask))
                        continue;   // no ground below, skip this cell

                    if (Vector3.Angle(hit.normal, Vector3.up) > maxSlope)
                        continue;   // too steep, skip

                    pos = hit.point;
                    groundNormal = hit.normal;
                }

                // Rotation: random spin around Y (up)
                Quaternion rot = randomRotation
                    ? Quaternion.Euler(0f, RandomRange(0f, 360f), 0f)
                    : Quaternion.identity;

                if (alignToSlope)                                                   // STEP 8
                    rot = Quaternion.FromToRotation(Vector3.up, groundNormal) * rot; // tilt to match the hill

                GameObject go = Instantiate(option.prefab, pos, rot, transform);

                // Scale: multiply the prefab's own scale by a random amount
                go.transform.localScale *= RandomRange(scaleRange.x, scaleRange.y);

                // STEP 8: lift so the bottom of the mesh touches the ground
                if (liftToSurface)
                {
                    Renderer r = go.GetComponentInChildren<Renderer>();
                    if (r != null)
                        go.transform.position += Vector3.up * (pos.y - r.bounds.min.y);
                }

                Mark(x, z, option.footprint);
            }
        }
    }

    float RandomRange(float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }

    SpawnOption PickOption()
    {
        float total = 0f;
        foreach (var o in options) total += o.weight;

        float roll = (float)rng.NextDouble() * total;
        foreach (var o in options)
        {
            roll -= o.weight;
            if (roll <= 0f) return o;
        }
        return options[options.Length - 1];
    }

    Vector3 CellToWorld(int x, int z, Vector2Int size)
    {
        return transform.position + new Vector3((x + size.x * 0.5f) * cellSize, 0f, (z + size.y * 0.5f) * cellSize);
    }

    bool Fits(int x, int z, Vector2Int size)
    {
        if (x + size.x > width || z + size.y > depth) return false; // would stick off the grid
        for (int i = x; i < x + size.x; i++)
            for (int j = z; j < z + size.y; j++)
                if (occupied[i, j]) return false;
        return true;
    }

    void Mark(int x, int z, Vector2Int size)
    {
        for (int i = x; i < x + size.x; i++)
            for (int j = z; j < z + size.y; j++)
                occupied[i, j] = true;
    }

    void UpdateNoiseOffset()
    {
        // Each seed reads from a different spot in the noise.
        var offsetRng = new System.Random(seed);
        noiseOffset = new Vector2(offsetRng.Next(0, 10000), offsetRng.Next(0, 10000));
    }

    float NoiseAt(int x, int z)
    {
        return Mathf.PerlinNoise((x + noiseOffset.x) * noiseScale,
                                 (z + noiseOffset.y) * noiseScale);
    }

    void OnDrawGizmosSelected()
    {
        UpdateNoiseOffset();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (IsManuallyBlocked(x, z))
                    Gizmos.color = new Color(1f, 0f, 0f, 0.7f);    // your blocks: strong red
                else if (!useNoise)
                    continue;                                       // nothing to show for this cell
                else if (NoiseAt(x, z) >= noiseThreshold)
                    Gizmos.color = new Color(0f, 1f, 0f, 0.4f);    // noise allows it: green
                else
                    Gizmos.color = new Color(1f, 0f, 0f, 0.15f);   // noise blocks it: faint red

                Gizmos.DrawCube(CellToWorld(x, z, Vector2Int.one),
                                new Vector3(cellSize * 0.9f, 0.05f, cellSize * 0.9f));
            }
        }
    }
    bool IsManuallyBlocked(int x, int z)
    {
        // 1. Is this cell inside one of the rectangles?
        Vector3 cellCenter = CellToWorld(x, z, Vector2Int.one);
        foreach (Transform t in blockedAreas)
        {
            if (t == null) continue;   // empty slot in the list
            Vector3 half = t.lossyScale * 0.5f;   // a Unity Cube is 1 unit, so scale = size
            if (Mathf.Abs(cellCenter.x - t.position.x) <= half.x &&
                Mathf.Abs(cellCenter.z - t.position.z) <= half.z)
                return true;
        }


        // 2. Is there a blocker object touching this cell?
        if (blockerMask != 0)
        {
            Vector3 center = CellToWorld(x, z, Vector2Int.one);
            // A tall, thin box over the cell. Slightly smaller than the cell
            // so a blocker that only touches the edge doesn't block the neighbors too.
            Vector3 halfSize = new Vector3(cellSize * 0.45f, raycastHeight, cellSize * 0.45f);
            if (Physics.CheckBox(center, halfSize, Quaternion.identity, blockerMask, QueryTriggerInteraction.Collide))
                return true;
        }

        return false;
    }
}