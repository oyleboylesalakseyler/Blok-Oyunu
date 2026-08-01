using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool isOccupied = false; // Hücre dolu mu?
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    // Bloğu yerleştirdiğimizde çağrılacak
    public void SetOccupied(Color color)
    {
        isOccupied = true;
        spriteRenderer.color = color;
    }

    // Satır silindiğinde çağrılacak
    public void Clear()
    {
        isOccupied = false;
        spriteRenderer.color = originalColor;
    }
}