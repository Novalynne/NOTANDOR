using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridRuntime : MonoBehaviour
{
    [System.Serializable]
    public class TileEntry
    {
        public string name;
        public GameObject prefab;
    }
    //tipi di tiles creabili, con riferimento al prefab da istanziare
    public TileEntry[] tileTypes;
    // dimensioni e spaziatura della griglia
    [Header("Grid Settings")]
    public int halfWidth = 5;
    public int halfHeight = 5;
    public float tileDistance = 1f;

    // Mappa tile (indice)
    public int[,] tileMap;

    private GameObject gridParent;
    // flag per mostrare la griglia in runtime
    private bool gridVisible = false;
    private bool togglable = true;
    // inizializza la griglia e se esiste già un oggetto "GridVisual" si ferma altrimenti crea la griglia
    void OnEnable()
    {
        if (tileMap == null || tileMap.Length == 0)
            tileMap = new int[halfWidth * 2 + 1, halfHeight * 2 + 1];

        Transform existing = transform.Find("GridVisual");

        if (existing != null)
        {
            gridParent = existing.gameObject;
        }
        else
        {
            GenerateGrid();
        }

        if (gridParent != null)
            gridParent.SetActive(true);
    }

    void Start()
    {
        // Inizia con la griglia nascosta in runtime
        if (gridParent != null)
            gridParent.SetActive(false);
    }
    public void ToggleGrid()
    {
        gridVisible = !gridVisible;
        if (gridParent != null)
            gridParent.SetActive(gridVisible);
    }
    //setta s ela griglia è togglable in runtime
    public void SetGridTogglable(bool canToggle)
    {
        togglable = canToggle;
    }
    //setta la visibilità della griglia in runtime, usato da altri script per mostrare o nascondere la griglia in base a determinate condizioni
    public void SetGridVisible(bool visible)
    {
        gridVisible = visible;

        if (gridParent != null)
            gridParent.SetActive(gridVisible);
    }
    void Update()
    {
        // Mostra la griglia se gridVisible o se il giocatore preme V
        if (Input.GetKeyDown(KeyCode.V) && togglable)
            ToggleGrid();
    }

    // flag per generare la griglia in editor
    [ContextMenu("Generate Grid")]
    void GenerateGrid()
    {
        // evita duplicati
        Transform existing = transform.Find("GridVisual");

        if (existing != null)
        {
            gridParent = existing.gameObject;
            return;
        }
        // crea un nuovo oggetto per contenere la griglia
        gridParent = new GameObject("GridVisual");
        gridParent.transform.SetParent(transform);
        // registra l'undo per la creazione della griglia in editor
        //permette di annullare la creazione della griglia se è stata generata per errore o se si vuole rigenerare
        //permette di salvare la scena dopo la creazione della griglia, in modo che le modifiche siano permanenti
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(gridParent, "Create Grid");
        EditorUtility.SetDirty(gridParent);
#endif
        // itera sulla mappa delle tile e istanzia i prefab corrispondenti
        for (int x = -halfWidth; x <= halfWidth; x++)
        {
            for (int z = -halfHeight; z <= halfHeight; z++)
            {
                int ix = x + halfWidth;
                int iz = z + halfHeight;

                int index = tileMap[ix, iz];

                if (index < 0 || index >= tileTypes.Length)
                    continue;

                GameObject prefab = tileTypes[index].prefab;

                Vector3 pos = new Vector3(x * tileDistance, 0.01f, z * tileDistance);

#if UNITY_EDITOR
                // Usa PrefabUtility per mantenere i legami con i prefab e supportare l'undo in editor
                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                tile.transform.SetParent(gridParent.transform);
                tile.transform.position = pos;
                tile.transform.rotation = Quaternion.identity;
                // Registra l'undo per la creazione della tile in editor
                Undo.RegisterCreatedObjectUndo(tile, "Create Tile");
                // Segna la scena come dirty per assicurarsi che venga salvata nella scena
                EditorUtility.SetDirty(tile);
#else
                // In runtime, usa Instantiate normale per evitare dipendenze dall'editor
                //scollegati dai prefab
                Instantiate(prefab, pos, Quaternion.identity, gridParent.transform);
#endif
            }
        }
    }
}