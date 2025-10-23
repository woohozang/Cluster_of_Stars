using UnityEngine;

public class LineFlow : MonoBehaviour
{
    public LineRenderer lr;
    public float scrollSpeed = 1.5f;

    private Material mat;
    private float offset;

    void Start()
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        mat = lr.material;
    }

    void Update()
    {
        if (mat == null) return;
        offset += Time.deltaTime * scrollSpeed;
        mat.SetTextureOffset("_BaseMap", new Vector2(offset, 0)); // URP ±‚¡ÿ
    }
}
