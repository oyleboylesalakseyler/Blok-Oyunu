using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Ayarları")]
    public int currentSize = 5;
    public float cellSize = 1.1f;
    public GameObject cellPrefab;

    [Header("Referanslar")]
    public ScoreManager scoreManager;

    private Cell[,] gridArray;
    private GameObject[,] cellObjects; // Genişleme sırasında hücreleri koruyabilmek için 2D referans

    void Awake()
    {
        // Awake() kullanıyoruz çünkü Unity, SAHNEDEKİ TÜM objelerin Awake()'ini
        // bitirmeden hiçbirinin Start()'ına geçmez. Bu sayede ShapeSpawner'ın
        // Start()'ı çalıştığında grid'in kesinlikle hazır olduğu garanti edilir.
        // (Start() kullansaydık, hangi objenin Start()'ının önce çalışacağı
        // garanti değildi ve bu "boş board'da bile sığmıyor" hatasına yol açabilirdi.)
        CreateGrid();
    }

    // 1. Izgarayı İLK KEZ oluşturma (oyun başında bir kez çağrılır)
    public void CreateGrid()
    {
        // Varsa eski hücreleri temizle
        if (cellObjects != null)
        {
            foreach (GameObject cell in cellObjects)
            {
                if (cell != null) Destroy(cell);
            }
        }

        gridArray = new Cell[currentSize, currentSize];
        cellObjects = new GameObject[currentSize, currentSize];

        for (int x = 0; x < currentSize; x++)
        {
            for (int y = 0; y < currentSize; y++)
            {
                SpawnCell(x, y);
            }
        }
        CenterCamera();
    }

    void SpawnCell(int x, int y)
    {
        Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);
        GameObject newCellObj = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
        newCellObj.name = $"Cell_{x}_{y}";

        Cell cellScript = newCellObj.GetComponent<Cell>();
        gridArray[x, y] = cellScript;
        cellObjects[x, y] = newCellObj;
    }

    // Izgarayı büyütürken MEVCUT hücreleri ve doluluk durumlarını KORUR,
    // sadece eksik olan yeni satır/sütunları ekler. (Eski CreateGrid() her şeyi siliyordu!)
    void ExpandGrid(int newSize)
    {
        Cell[,] oldGrid = gridArray;
        GameObject[,] oldObjects = cellObjects;
        int oldSize = currentSize;

        gridArray = new Cell[newSize, newSize];
        cellObjects = new GameObject[newSize, newSize];

        for (int x = 0; x < newSize; x++)
        {
            for (int y = 0; y < newSize; y++)
            {
                if (x < oldSize && y < oldSize)
                {
                    // Eski hücreyi (ve dolu/boş durumunu) aynen koru
                    gridArray[x, y] = oldGrid[x, y];
                    cellObjects[x, y] = oldObjects[x, y];
                }
                else
                {
                    // Yeni açılan alan için hücre oluştur
                    SpawnCell(x, y);
                }
            }
        }

        currentSize = newSize;
        CenterCamera();
    }

    // 2. Kamerayı Grid'e Göre Hizalama
    void CenterCamera()
    {
        float centerPos = (currentSize * cellSize) / 2f - (cellSize / 2f);
        Camera.main.transform.position = new Vector3(centerPos, centerPos, -10);

        // Grid büyüdükçe kamerayı uzaklaştır
        Camera.main.orthographicSize = currentSize + 2;
    }

    // 3. Yerleştirme Kontrolü (Shape script'i burayı çağıracak)
    public bool TryPlaceShape(List<Vector3> blockPositions, Color color)
    {
        List<Cell> targetCells = new List<Cell>();

        foreach (Vector3 worldPos in blockPositions)
        {
            Cell closestCell = GetClosestCell(worldPos);

            // Grid dışındaysa, doluysa veya (nadir bir snapping hatasıyla) aynı hücre iki kez seçildiyse yerleşemez
            if (closestCell == null || closestCell.isOccupied || targetCells.Contains(closestCell))
                return false;

            targetCells.Add(closestCell);
        }

        // Tüm parçalar uygun, yerleştir
        foreach (Cell cell in targetCells)
        {
            cell.SetOccupied(color);
        }

        CheckBoard(); // Tek ve tek kontrol noktası burası - Shape.cs'de TEKRAR çağırmayın!
        return true;
    }

    // Dünya pozisyonuna en yakın hücreyi bulur (Snapping için)
    Cell GetClosestCell(Vector3 worldPos)
    {
        Cell closest = null;
        float minDistance = 0.5f;

        foreach (Cell cell in gridArray)
        {
            float dist = Vector3.Distance(worldPos, cell.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = cell;
            }
        }
        return closest;
    }

    // 4. Satır ve Sütun Silme Kontrolü
    public void CheckBoard()
    {
        List<int> rowsToDelete = new List<int>();
        List<int> colsToDelete = new List<int>();

        for (int y = 0; y < currentSize; y++)
        {
            bool isFull = true;
            for (int x = 0; x < currentSize; x++)
            {
                if (!gridArray[x, y].isOccupied) { isFull = false; break; }
            }
            if (isFull) rowsToDelete.Add(y);
        }

        for (int x = 0; x < currentSize; x++)
        {
            bool isFull = true;
            for (int y = 0; y < currentSize; y++)
            {
                if (!gridArray[x, y].isOccupied) { isFull = false; break; }
            }
            if (isFull) colsToDelete.Add(x);
        }

        // Silinecek bir şey yoksa ScoreManager'ı hiç tetikleme (streak boşuna sıfırlanmasın)
        if (rowsToDelete.Count > 0 || colsToDelete.Count > 0)
        {
            ClearLines(rowsToDelete, colsToDelete);
        }
    }

    void ClearLines(List<int> rows, List<int> cols)
    {
        foreach (int y in rows)
        {
            for (int x = 0; x < currentSize; x++) gridArray[x, y].Clear();
        }

        foreach (int x in cols)
        {
            for (int y = 0; y < currentSize; y++) gridArray[x, y].Clear();
        }

        int totalCleared = rows.Count + cols.Count;
        scoreManager.AddScore(totalCleared);

        CheckForExpansion();
    }

    void CheckForExpansion()
    {
        // Skor eşiği aşıldığında ve max 10x10 olana kadar büyüt
        if (scoreManager.totalScore >= (currentSize - 4) * 1000 && currentSize < 10)
        {
            int newSize = currentSize + 1;
            ExpandGrid(newSize); // Artık mevcut bloklar SİLİNMİYOR
            Debug.Log("Harita Genişledi! Yeni Boyut: " + currentSize);
        }
    }

    // 5. Oyun Bitti mi? Verilen şekillerden HİÇBİRİ tahtaya sığmıyorsa true döner.
    public bool HasAnyValidMove(List<ShapeData> shapes)
    {
        // Grid henüz oluşturulmadıysa (teorik olarak artık imkansız ama garanti olsun),
        // hatalı "oyun bitti" tetiklenmesin diye true dönüyoruz.
        if (gridArray == null) return true;

        foreach (ShapeData shape in shapes)
        {
            if (shape == null) continue;
            if (CanShapeFitAnywhere(shape)) return true;
        }
        return false;
    }

    bool CanShapeFitAnywhere(ShapeData shape)
    {
        for (int startX = 0; startX < currentSize; startX++)
        {
            for (int startY = 0; startY < currentSize; startY++)
            {
                bool fits = true;
                foreach (Vector2Int cellOffset in shape.cells)
                {
                    int gx = startX + cellOffset.x;
                    int gy = startY + cellOffset.y;

                    if (gx < 0 || gx >= currentSize || gy < 0 || gy >= currentSize || gridArray[gx, gy].isOccupied)
                    {
                        fits = false;
                        break;
                    }
                }
                if (fits) return true;
            }
        }
        return false;
    }
}