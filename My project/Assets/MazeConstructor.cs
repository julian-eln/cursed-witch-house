/*
 * written by Joseph Hocking 2017
 * released under MIT license
 * text of license https://opensource.org/licenses/MIT
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The coordinator for building a maze. It doesn't decide the layout or build
// geometry itself; it asks MazeDataGenerator for the layout, asks
// MazeMeshGenerator for the geometry, then puts everything into the scene.
//
// Lives on the same GameObject as GameController (GameController's
// [RequireComponent] guarantees that).
public class MazeConstructor : MonoBehaviour
{
    // Tick this in the Inspector to draw the maze grid as text on screen.
    public bool showDebug;

    // Materials, assigned in the Inspector. [SerializeField] shows a private
    // field in the Inspector without letting other scripts change it.
    [SerializeField] private Material mazeMat1;     // floors and ceilings
    [SerializeField] private Material mazeMat2;     // walls
    [SerializeField] private Material startMat;     // start trigger cube
    [SerializeField] private Material treasureMat;  // goal/treasure cube

    // ---- Prefab mode (optional) ----
    // Tick usePrefabs to build the maze from prefabs placed in each grid cell
    // instead of one generated mesh. Leave it off to use the original mesh.
    [Header("Prefab mode")]
    [SerializeField] private bool usePrefabs = false;
    [SerializeField] private GameObject wallPrefab;     // fills one wall cell (needs a collider)
    [SerializeField] private GameObject floorPrefab;    // one floor tile per open cell (needs a collider)
    [SerializeField] private GameObject ceilingPrefab;  // optional; leave empty for no ceiling
    [SerializeField] private float cellSize = 3.75f;    // width of one cell in prefab mode
    [SerializeField] private float cellHeight = 3.5f;   // wall height in prefab mode

    // On: stretch each piece to fit the cell (good for Unity's 1x1x1 cube).
    // Off: use each prefab at its own size; author it to match cellSize/cellHeight.
    [SerializeField] private bool scalePrefabsToCell = true;

    // The current maze grid: data[row, col], 0 = floor, 1 = wall.
    // "get; private set;" means any script can READ it, but only this class
    // can replace it. GameController can inspect the maze but not overwrite it.
    public int[,] data
    {
        get; private set;
    }

    // Size of one grid cell in world units, copied from the mesh generator.
    // Other scripts use these to convert grid coordinates to world positions:
    //     worldX = col * hallWidth, worldZ = row * hallWidth
    public float hallWidth
    {
        get; private set;
    }
    public float hallHeight
    {
        get; private set;
    }

    // Grid coordinates of the start cell (useful for placing the player).
    public int startRow
    {
        get; private set;
    }
    public int startCol
    {
        get; private set;
    }

    // Grid coordinates of the goal (treasure) cell.
    public int goalRow
    {
        get; private set;
    }
    public int goalCol
    {
        get; private set;
    }

    // The two helpers that do the actual work. They're plain C# classes, so
    // they're created with "new" instead of being added as components.
    private MazeDataGenerator dataGenerator;
    private MazeMeshGenerator meshGenerator;

    // Awake runs before any Start(), so the helpers exist by the time
    // GameController.Start() asks for a maze.
    void Awake()
    {
        dataGenerator = new MazeDataGenerator();
        meshGenerator = new MazeMeshGenerator();

        // default to walls surrounding a single empty cell
        // A tiny placeholder so "data" is never null. Without it, OnGUI would
        // crash if showDebug were on before a real maze was generated.
        data = new int[,]
        {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        };
    }

    // The main entry point: throws away any old maze and builds a new one.
    // The two callbacks are optional (default null). They're the methods to
    // run when the player touches the start cube or the treasure cube.
    public void GenerateNewMaze(int sizeRows, int sizeCols,
        TriggerEventHandler startCallback = null, TriggerEventHandler goalCallback = null)
    {
        // Warn about even sizes, which give a lopsided border. Note this only
        // fires when BOTH are even; "||" would also catch one even dimension.
        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
        {
            Debug.LogError("Odd numbers work better for dungeon size.");
        }

        // 1. Remove the previous maze and its trigger cubes.
        DisposeOldMaze();

        // 2. Generate the layout (a grid of 0s and 1s).
        data = dataGenerator.FromDimensions(sizeRows, sizeCols);

        // 3. Choose start and goal cells from the open spaces.
        FindStartPosition();
        FindGoalPosition();

        // store values used to generate this mesh
        // 4. Remember the cell size so other scripts can convert grid
        //    coordinates to world positions.
        //    In prefab mode the cell size comes from the Inspector instead.
        hallWidth = usePrefabs ? cellSize : meshGenerator.width;
        hallHeight = usePrefabs ? cellHeight : meshGenerator.height;

        // 5. Build the visible maze: prefabs or the generated mesh.
        if (usePrefabs)
        {
            DisplayMazeWithPrefabs();
        }
        else
        {
            DisplayMaze();
        }

        // 6. Place the start and treasure cubes and hook up their callbacks.
        PlaceStartTrigger(startCallback);
        PlaceGoalTrigger(goalCallback);
    }

    // Creates a new GameObject that shows the maze mesh and collides with it.
    private void DisplayMaze()
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;   // grid cell (0,0) sits at the world origin
        go.name = "Procedural Maze";
        go.tag = "Generated";   // the "Generated" tag must exist in Tags & Layers

        // MeshFilter holds the shape.
        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = meshGenerator.FromData(data);

        // MeshCollider uses the same shape for physics, so the player can
        // stand on the floor and bump into the walls.
        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        // MeshRenderer draws it. The mesh has two submeshes, and each
        // material is matched by position: submesh 0 (floors/ceilings) gets
        // mazeMat1, submesh 1 (walls) gets mazeMat2.
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] { mazeMat1, mazeMat2 };
    }

    // Prefab alternative to DisplayMaze: one wall block per wall cell, and a
    // floor tile (plus optional ceiling tile) per open cell.
    private void DisplayMazeWithPrefabs()
    {
        // One parent holds every piece, which keeps the Hierarchy tidy. Only
        // the parent needs the "Generated" tag: when DisposeOldMaze destroys
        // it, all of its children are destroyed with it.
        GameObject root = new GameObject("Procedural Maze");
        root.transform.position = Vector3.zero;
        root.tag = "Generated";
        Transform parent = root.transform;

        int rMax = data.GetUpperBound(0);
        int cMax = data.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                // Same grid-to-world rule as the mesh version: col -> x, row -> z.
                Vector3 cellCenter = new Vector3(j * hallWidth, 0, i * hallWidth);

                if (data[i, j] == 1)
                {
                    // Wall block, raised by half its height so its bottom sits
                    // on the floor. This assumes the pivot is at the block's
                    // center, like Unity's built-in cube.
                    PlacePiece(wallPrefab, parent,
                        cellCenter + Vector3.up * (hallHeight * .5f), true);
                }
                else
                {
                    // Floor tile at ground level.
                    PlacePiece(floorPrefab, parent, cellCenter, false);

                    // Ceiling tile at the top of the walls, if one is assigned.
                    if (ceilingPrefab != null)
                    {
                        PlacePiece(ceilingPrefab, parent,
                            cellCenter + Vector3.up * hallHeight, false);
                    }
                }
            }
        }

        // Hundreds of separate objects each cost a draw call. This merges
        // them into a few large batches at runtime so they render much faster.
        // (Imported models may need Read/Write enabled in import settings.)
        StaticBatchingUtility.Combine(root);
    }

    // Creates one copy of a prefab as a child of the maze root.
    // scaleHeight: true for walls (stretch to full wall height); false for
    // floor and ceiling tiles (keep the prefab's own thickness).
    private void PlacePiece(GameObject prefab, Transform parent, Vector3 position, bool scaleHeight)
    {
        if (prefab == null)
        {
            return;   // nothing assigned in the Inspector for this piece
        }

        GameObject piece = Instantiate(prefab, position, prefab.transform.rotation, parent);

        if (scalePrefabsToCell)
        {
            Vector3 s = prefab.transform.localScale;
            piece.transform.localScale = new Vector3(
                hallWidth,
                scaleHeight ? hallHeight : s.y,
                hallWidth);
        }
    }

    // Destroys everything this script created. Every object made here is
    // tagged "Generated", so one tag search finds the maze and both cubes.
    // Public so GameController can clear the maze (e.g. when restarting).
    public void DisposeOldMaze()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Generated");
        foreach (GameObject go in objects)
        {
            Destroy(go);   // actual removal happens at the end of the frame
        }
    }

    // Start = the first open cell, scanning from row 0, column 0 upward.
    // That puts it near the grid's (0,0) corner.
    private void FindStartPosition()
    {
        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (maze[i, j] == 0)
                {
                    startRow = i;
                    startCol = j;
                    return;   // stop at the first one found
                }
            }
        }
    }

    // Goal = the first open cell scanning backward from the last row and
    // column. That puts it near the opposite corner from the start.
    // Note: nothing checks that a path exists between start and goal.
    private void FindGoalPosition()
    {
        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        // loop top to bottom, right to left
        for (int i = rMax; i >= 0; i--)
        {
            for (int j = cMax; j >= 0; j--)
            {
                if (maze[i, j] == 0)
                {
                    goalRow = i;
                    goalCol = j;
                    return;
                }
            }
        }
    }

    // Creates a cube at the start cell that reports when something enters it.
    private void PlaceStartTrigger(TriggerEventHandler callback)
    {
        // A default 1x1x1 cube with a BoxCollider and MeshRenderer.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Convert grid (row, col) to world (x, z). y = .5 puts the bottom of
        // a 1-unit-tall cube on the floor at y = 0.
        go.transform.position = new Vector3(startCol * hallWidth, .5f, startRow * hallWidth);
        go.name = "Start Trigger";
        go.tag = "Generated";   // so DisposeOldMaze cleans it up

        // isTrigger: things pass through it, but Unity still sends
        // OnTriggerEnter when they do.
        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = startMat;

        // Attach the router and give it the method to call on contact.
        TriggerEventRouter tc = go.AddComponent<TriggerEventRouter>();
        tc.callback = callback;
    }

    // Same as PlaceStartTrigger, but at the goal cell with the treasure material.
    private void PlaceGoalTrigger(TriggerEventHandler callback)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = new Vector3(goalCol * hallWidth, .5f, goalRow * hallWidth);
        go.name = "Treasure";
        go.tag = "Generated";

        go.GetComponent<BoxCollider>().isTrigger = true;
        go.GetComponent<MeshRenderer>().sharedMaterial = treasureMat;

        TriggerEventRouter tc = go.AddComponent<TriggerEventRouter>();
        tc.callback = callback;
    }

    // top-down debug display
    // OnGUI is Unity's immediate-mode UI callback; it runs every frame (often
    // more than once). Fine for debugging, too slow for real UI.
    void OnGUI()
    {
        if (!showDebug)
        {
            return;
        }

        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        string msg = "";

        // loop top to bottom, left to right
        // Rows print from last to first, so row 0 ends up at the BOTTOM of
        // the text, matching a top-down view where z increases upward.
        for (int i = rMax; i >= 0; i--)
        {
            for (int j = 0; j <= cMax; j++)
            {
                if (maze[i, j] == 0)
                {
                    msg += "....";   // floor (4 dots ≈ width of 2 "=" signs)
                }
                else
                {
                    msg += "==";     // wall
                }
            }
            msg += "\n";
        }

        // Draw the text in a 500x500 pixel box near the top-left corner.
        GUI.Label(new Rect(20, 20, 500, 500), msg);
    }
}