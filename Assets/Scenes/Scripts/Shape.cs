using UnityEngine;
using System.Collections.Generic;

public class Shape : MonoBehaviour
{
    public ShapeData shapeData;
    public GameObject squarePrefab; 
    private List<GameObject> children = new List<GameObject>();
    
    private Vector3 originalPosition;
    private GridManager gridManager;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        originalPosition = transform.position;
        
        // Eğer shapeData atanmışsa şekli oluştur
        if (shapeData != null) 
            CreateShape();
    }

    void CreateShape()
    {
        // Eski parçalar varsa temizle
        foreach (var child in children) Destroy(child);
        children.Clear();

        foreach (var pos in shapeData.cells)
        {
            GameObject square = Instantiate(squarePrefab, transform);
            square.transform.localPosition = new Vector3(pos.x, pos.y, 0);
            
            // Hücrenin rengini ayarla
            if (square.GetComponent<SpriteRenderer>() != null)
                square.GetComponent<SpriteRenderer>().color = shapeData.shapeColor;
                
            children.Add(square);
        }
    }

    private void OnMouseDown() 
    {
        // Tıklayınca bloğu biraz büyüt (UX efekti)
        transform.localScale = Vector3.one * 1.1f;
    }

    private void OnMouseDrag()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        transform.position = mousePos;
    }

    private void OnMouseUp()
    {
        // Bırakınca ölçeği düzelt
        transform.localScale = Vector3.one;

        if (TryPlace())
        {
            gridManager.CheckBoard();
            
            // Spawner'ı bul ve kontrol etmesini söyle
            ShapeSpawner spawner = FindFirstObjectByType<ShapeSpawner>();
            if (spawner != null) 
                spawner.CheckAndRespawn(); 
                
            Destroy(gameObject); // Yerleştiyse bu objeyi yok et
        }
        else
        {
            transform.position = originalPosition; // Yerleşemediyse geri dön
        }
    }

    bool TryPlace()
    {
        List<Vector3> blockPositions = new List<Vector3>();
        foreach (var child in children)
        {
            blockPositions.Add(child.transform.position);
        }

        // GridManager'a yerleştirme isteği gönder
        return gridManager.TryPlaceShape(blockPositions, shapeData.shapeColor);
    }
}