using UnityEngine;

[CreateAssetMenu(fileName = "New Shape", menuName = "BlockBlast/Shape Data")]
public class ShapeData : ScriptableObject
{
    public Vector2Int[] cells; // Bloğun hangi hücrelerden oluştuğu (örn: 0,0 - 0,1 - 1,0)
    public Color shapeColor = Color.blue;
}