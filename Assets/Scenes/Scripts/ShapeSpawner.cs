using UnityEngine;
using System.Collections.Generic;

public class ShapeSpawner : MonoBehaviour
{
    public List<ShapeData> allShapes; // Oluşturduğun ShapeData'ları buraya sürükle
    public GameObject shapePrefab;    // ShapePrefab'ını buraya sürükle
    public Transform[] spawnPoints;   // Ekranın altındaki noktaların pozisyonu

    [Header("Referanslar")]
    public GridManager gridManager;   // Boş bırakılırsa otomatik bulunur

    private GameObject[] currentShapes;

    void Start()
    {
        currentShapes = new GameObject[spawnPoints.Length];

        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        SpawnAllShapes();
    }

    public void SpawnAllShapes()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            SpawnShape(i);
        }

        CheckGameOver();
    }

    void SpawnShape(int index)
    {
        int randomIndex = Random.Range(0, allShapes.Count);
        GameObject newShape = Instantiate(shapePrefab, spawnPoints[index].position, Quaternion.identity);
        Shape shapeScript = newShape.GetComponent<Shape>();
        shapeScript.shapeData = allShapes[randomIndex];

        currentShapes[index] = newShape;
    }

    // Bir şekil yerleştirildiğinde Shape.cs tarafından çağrılır.
    // Destroy() frame sonuna kadar objeyi gerçekten silmediği için (bu yüzden "s != null"
    // kontrolü o an hâlâ true dönüyordu), diziden elle ve ANINDA çıkarıyoruz.
    public void NotifyShapePlaced(GameObject placedShape)
    {
        for (int i = 0; i < currentShapes.Length; i++)
        {
            if (currentShapes[i] == placedShape)
            {
                currentShapes[i] = null;
                break;
            }
        }

        CheckAndRespawn();
    }

    // Bir blok yerleştirildiğinde çağrılacak
    public void CheckAndRespawn()
    {
        bool allEmpty = true;
        foreach (var s in currentShapes)
        {
            if (s != null) { allEmpty = false; break; }
        }

        if (allEmpty)
        {
            SpawnAllShapes();
        }
        else
        {
            // Tepside hala şekil var; bunlardan en az biri yerleşebiliyor mu kontrol et
            CheckGameOver();
        }
    }

    // Tepsideki mevcut şekillerden hiçbiri tahtaya sığmıyorsa oyun biter
    void CheckGameOver()
    {
        if (gridManager == null) return;

        List<ShapeData> activeShapeData = new List<ShapeData>();
        foreach (var s in currentShapes)
        {
            if (s == null) continue;
            Shape shapeScript = s.GetComponent<Shape>();
            if (shapeScript != null && shapeScript.shapeData != null)
                activeShapeData.Add(shapeScript.shapeData);
        }

        if (activeShapeData.Count > 0 && !gridManager.HasAnyValidMove(activeShapeData))
        {
            Debug.Log("Oyun Bitti! Elindeki hiçbir şekil tahtaya sığmıyor.");
            // TODO: Burada kendi Game Over UI panelini / event'ini tetikle
        }
    }
}
