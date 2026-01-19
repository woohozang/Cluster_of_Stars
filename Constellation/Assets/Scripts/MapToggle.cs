using UnityEngine;

public class MapToggle : MonoBehaviour
{
    [Header("Map Object Settings")]
    public GameObject mapObject; // 껐다 켰다 할 맵 오브젝트 (자식으로 넣은 것)

    void Update()
    {
        // 오큘러스 컨트롤러의 A버튼 (또는 X버튼) 입력을 감지
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ToggleMap();
        }
    }

    void ToggleMap()
    {
        if (mapObject != null)
        {
            // 현재 켜져있으면 끄고, 꺼져있으면 켬 (Toggle)
            bool isActive = mapObject.activeSelf;
            mapObject.SetActive(!isActive);
        }
    }
}