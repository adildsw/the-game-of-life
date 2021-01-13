using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBehaviourScript : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile inactiveTile;
    public Tile activeTile;
    [Range(1, 20)] public int playSpeed = 10;

    private bool isPlaying;

    private List<Vector3Int> activatedTilesList;
    private List<Vector3Int> previouslyActiveTilesList;

    private List<Vector3Int> toBeActiveTilesList;
    private List<Vector3Int> toBeInactiveTilesList;

    private Vector3Int[] neighbourReference;

    private float timer = 0.0f;
    private float playInterval;
    
    void Start() {
        isPlaying = false;
        activatedTilesList = new List<Vector3Int>();

        toBeActiveTilesList = new List<Vector3Int>();
        toBeInactiveTilesList = new List<Vector3Int>();

        // All possible neighbouring tile position references
        neighbourReference = new [] {
            new Vector3Int(+1, +0, +0),
            new Vector3Int(-1, +0, +0),
            new Vector3Int(+0, +1, +0),
            new Vector3Int(+0, -1, +0),
            new Vector3Int(+1, +1, +0),
            new Vector3Int(-1, +1, +0),
            new Vector3Int(+1, -1, +0),
            new Vector3Int(-1, -1, +0)
        };
    }

    void Update() {
        // Pausing simulation on 'space' key press
        if (Input.GetKeyDown("space")) {
            isPlaying = !isPlaying;
        }

        // Clearing simulation screen on 'c' key press
        if (Input.GetKeyDown("c")) {
            ClearScreen();
        }

        // Activating tiles on left mouse click
        if (Input.GetMouseButton(0) && isPlaying == false) {
            ActivateTileOnClick();
        }

        // Deactivating tiles on right mouse click
        if (Input.GetMouseButton(1) && isPlaying == false) {
            DeactivateTileOnClick();
        }
    }

    void FixedUpdate() {
        playInterval = 1.0f/playSpeed;
        if (isPlaying == true) {
            timer += Time.deltaTime;
            if (timer > playInterval) {
                timer -= playInterval; // Resetting iteration timer
                Play(); // Running the next simulation iteration
            }
        }
    }

    void Play() {
            List<Vector3Int> unvisitedTilesList = GetUnvisitedTilesList();
            previouslyActiveTilesList = new List<Vector3Int>(activatedTilesList);

            ClearBuffers();

            foreach (Vector3Int pos in unvisitedTilesList) {
                int activeNeighbourCount = GetActiveNeighbouringTileCount(pos);

                if (GetTileType(pos) == "ActiveTile") {
                    if (activeNeighbourCount < 2 || activeNeighbourCount > 3) {
                        toBeInactiveTilesList.Add(pos);
                    }
                    else {
                        toBeActiveTilesList.Add(pos);
                    }
                }
                else if (GetTileType(pos) == "InactiveTile") {
                    if (activeNeighbourCount == 3) {
                        toBeActiveTilesList.Add(pos);
                    }
                    else {
                        toBeInactiveTilesList.Add(pos);
                    }
                }
            }

            UpdateTiles();

            if (isSimulationComplete()) {
                isPlaying = false;
                Debug.Log("Simulation Complete.");
            }
    }

    bool isSimulationComplete() {
        if (activatedTilesList.Count == previouslyActiveTilesList.Count) {
            foreach (Vector3Int item in activatedTilesList) {
                if (!previouslyActiveTilesList.Contains(item)) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    List<Vector3Int> GetUnvisitedTilesList() {
        List<Vector3Int> unvisitedTilesList = new List<Vector3Int>();

        foreach (Vector3Int pos in activatedTilesList) {
            unvisitedTilesList.Add(pos);
            foreach (Vector3Int posRef in neighbourReference) {
                if (GetTileType(pos + posRef) == "InactiveTile") {
                    if (!unvisitedTilesList.Contains(pos + posRef)) {
                        unvisitedTilesList.Add(pos + posRef);
                    }
                }
            }
        }

        return unvisitedTilesList;
    }

    int GetActiveNeighbouringTileCount(Vector3Int pos) {
        int count = 0;
        foreach (Vector3Int posRef in neighbourReference) {
            if (GetTileType(pos + posRef) == "ActiveTile") {
                count++;
            }
        }
        return count;
    }

    void ClearBuffers() {
        activatedTilesList.Clear();
        toBeActiveTilesList.Clear();
        toBeInactiveTilesList.Clear();
    }

    void UpdateTiles() {
        foreach (Vector3Int pos in toBeActiveTilesList) {
            tilemap.SetTile(pos, activeTile);
            activatedTilesList.Add(pos);
        }

        foreach (Vector3Int pos in toBeInactiveTilesList) {
            tilemap.SetTile(pos, inactiveTile);
        }
    }

    void ActivateTileOnClick() {
            Vector3Int tilemapPos = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (!activatedTilesList.Contains(tilemapPos)) {
                activatedTilesList.Add(tilemapPos);
                tilemap.SetTile(tilemapPos, activeTile);
            }
    }

    void DeactivateTileOnClick() {
        Vector3Int tilemapPos = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (activatedTilesList.Contains(tilemapPos)) {
            activatedTilesList.Remove(tilemapPos);
            tilemap.SetTile(tilemapPos, inactiveTile);
        }
    }

    void ClearScreen() {
        foreach (Vector3Int pos in activatedTilesList) {
            tilemap.SetTile(pos, inactiveTile);
        }
        activatedTilesList.Clear();
    }

    string GetTileType(Vector3Int pos) {
        Tile tile = tilemap.GetTile<Tile>(pos);
        string tileType = "OutOfBoundsTile";

        if (tile == inactiveTile) {
            tileType = "InactiveTile";
        }
        else if (tile == activeTile) {
            tileType = "ActiveTile";
        }

        return tileType;
    }
}
