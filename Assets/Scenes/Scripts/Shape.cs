using UnityEngine;
using System.Collections.Generic;

public class Shape : MonoBehaviour
{
    public ShapeData shapeData;
    public GameObject squarePrefab;
    private List<GameObject> children = new List<GameObject>();

    private Vector3 originalPosition;
    private Vector3 dragOffset;
    private GridManager gridManager;
    private ShapeSpawner spawner;
    private BoxCollider2D boxCollider;

    void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        spawner = FindFirstObjectByType<ShapeSpawner>();
        boxCollider = GetComponent<BoxCollider2D>();
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

            if (square.GetComponent<SpriteRenderer>() != null)
                square.GetComponent<SpriteRenderer>().color = shapeData.shapeColor;

            children.Add(square);
        }

        ResizeColliderToFitShape();
    }

    // Parent üzerindeki Box Collider 2D varsayılan olarak 1x1'dir ve sadece (0,0) hücresini kapsar.
    // Bu yüzden L/T gibi çok hücreli şekillerin çoğu bölgesine tıklayınca OnMouseDown hiç tetiklenmiyordu.
    // Burada collider'ı, şeklin tüm hücrelerini kaplayacak şekilde otomatik hesaplıyoruz.
    void ResizeColliderToFitShape()
    {
        if (boxCollider == null || shapeData == null || shapeData.cells.Length == 0)
            return;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var pos in shapeData.cells)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        float width = (maxX - minX) + 1;
        float height = (maxY - minY) + 1;
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        boxCollider.size = new Vector2(width, height);
        boxCollider.offset = new Vector2(centerX, centerY);
    }

    private void OnMouseDown()
    {
        // Tıklayınca bloğu biraz büyüt (UX efekti)
        transform.localScale = Vector3.one * 1.1f;

        // Fareyle şeklin merkezi arasındaki farkı kaydet, ki sürüklerken şekil
        // aniden fare imlecinin tam ortasına "zıplamasın"
        Vector3 mouseWorld = GetMouseWorldPosition();
        dragOffset = transform.position - mouseWorld;
    }

    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPosition() + dragOffset;
    }

    // Project Settings > Active Input Handling ayarının "Input Manager (Old)" veya "Both"
    // olması gerekiyor; aksi halde Input.mousePosition InvalidOperationException fırlatır.
    Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;
        return worldPos;
    }

    private void OnMouseUp()
    {
        // Bırakınca ölçeği düzelt
        transform.localScale = Vector3.one;

        if (TryPlace())
        {
            // NOT: GridManager.TryPlaceShape() tahtayı zaten CheckBoard() ile kontrol ediyor.
            // Burada tekrar çağırmak, satırlar zaten silindiği için AddScore(0) tetikliyor
            // ve bu da streak'i (kombo sayacını) her seferinde sıfırlıyordu. Kaldırıldı.

            if (spawner != null)
                spawner.NotifyShapePlaced(gameObject); // Destroy() gerçekleşmeden diziden hemen çıkar

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

        return gridManager.TryPlaceShape(blockPositions, shapeData.shapeColor);
    }
}
