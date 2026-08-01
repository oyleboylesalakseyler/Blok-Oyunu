using UnityEngine;
using System.Collections.Generic;

public class ShapeSpawner : MonoBehaviour
{
    public List<ShapeData> allShapes; // Oluşturduğun ShapeData'ları buraya sürükle
    public GameObject shapePrefab;    // ShapePrefab'ını buraya sürükle
    public Transform[] spawnPoints;   // Ekranın altındaki 3 noktanın pozisyonu

    private GameObject[] currentShapes = new GameObject[3];

    void Start()
    {
        SpawnAllShapes();
    }

    public void SpawnAllShapes()
    {
        for (int i = 0; i < 3; i++)
        {
            SpawnShape(i);
        }
    }

    void SpawnShape(int index)
    {
        int randomIndex = Random.Range(0, allShapes.Count);
        GameObject newShape = Instantiate(shapePrefab, spawnPoints[index].position, Quaternion.identity);
        Shape shapeScript = newShape.GetComponent<Shape>();
        shapeScript.shapeData = allShapes[randomIndex];
        
        currentShapes[index] = newShape;
    }

    // Bir blok yerleştirildiğinde çağrılacak
    public void CheckAndRespawn()
    {
        bool allEmpty = true;
        foreach (var s in currentShapes)
        {
            if (s != null) allEmpty = false;
        }

        if (allEmpty)
        {
            SpawnAllShapes();
        }
    }
}