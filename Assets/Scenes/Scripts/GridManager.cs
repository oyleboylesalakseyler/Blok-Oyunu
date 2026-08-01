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
    private List<GameObject> spawnedCells = new List<GameObject>();

    void Start()
    {
        CreateGrid();
    }

    // 1. Izgarayı Oluşturma ve Genişletme
    public void CreateGrid()
    {
        // Eski hücreleri temizle (Harita büyürken eskileri siler)
        foreach (GameObject cell in spawnedCells)
        {
            Destroy(cell);
        }
        spawnedCells.Clear();

        gridArray = new Cell[currentSize, currentSize];

        for (int x = 0; x < currentSize; x++)
        {
            for (int y = 0; y < currentSize; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);
                GameObject newCellObj = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                newCellObj.name = $"Cell_{x}_{y}";
                
                Cell cellScript = newCellObj.GetComponent<Cell>();
                gridArray[x, y] = cellScript;
                spawnedCells.Add(newCellObj);
            }
        }
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
            
            // Eğer blok grid dışındaysa veya hücre doluysa yerleşemez
            if (closestCell == null || closestCell.isOccupied)
                return false;

            targetCells.Add(closestCell);
        }

        // Eğer buraya geldiyse tüm parçalar uygun demektir, yerleştir!
        foreach (Cell cell in targetCells)
        {
            cell.SetOccupied(color);
        }

        CheckBoard(); // Her yerleştirmeden sonra tahtayı kontrol et
        return true;
    }

    // Dünya pozisyonuna en yakın hücreyi bulur (Snapping için)
    Cell GetClosestCell(Vector3 worldPos)
    {
        Cell closest = null;
        float minDistance = 0.5f; // Hücreye ne kadar yakın olmalı?

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

        // Satırları kontrol et
        for (int y = 0; y < currentSize; y++)
        {
            bool isFull = true;
            for (int x = 0; x < currentSize; x++)
            {
                if (!gridArray[x, y].isOccupied) { isFull = false; break; }
            }
            if (isFull) rowsToDelete.Add(y);
        }

        // Sütunları kontrol et
        for (int x = 0; x < currentSize; x++)
        {
            bool isFull = true;
            for (int y = 0; y < currentSize; y++)
            {
                if (!gridArray[x, y].isOccupied) { isFull = false; break; }
            }
            if (isFull) colsToDelete.Add(x);
        }

        // Temizleme işlemini başlat
        ClearLines(rowsToDelete, colsToDelete);
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

        // Skor ve Streak hesapla
        int totalCleared = rows.Count + cols.Count;
        scoreManager.AddScore(totalCleared);

        // Harita Büyütme Kontrolü (Her 1000 puanda bir büyüme örn: 5x5 -> 6x6)
        CheckForExpansion();
    }

    void CheckForExpansion()
    {
        // Skor 1000'in katlarına ulaştığında ve max 10x10 olana kadar büyüt
        if (scoreManager.totalScore >= (currentSize - 4) * 1000 && currentSize < 10)
        {
            currentSize++;
            CreateGrid();
            Debug.Log("Harita Genişledi! Yeni Boyut: " + currentSize);
        }
    }
}