using UnityEngine;
using System.Collections;

public class VRStartRotator : MonoBehaviour
{
    [Header("Settings")]
    public Transform cockpitForward; // 정면 기준점 (HUD나 전면 유리창)
    public Transform ovrCameraRig;   // OVRCameraRig 최상위 부모
    public Transform centerEyeAnchor; // 실제 눈(카메라)

    [Header("Debug")]
    public KeyCode recenterKey = KeyCode.R; // PC에서 테스트할 때 R키로 리셋

    IEnumerator Start()
    {
        // VR 기기가 초기화되고 안정을 찾을 때까지 약간 대기 (매우 중요!)
        yield return new WaitForSeconds(0.5f);

        Recenter();

        // 혹시 모르니 1초 뒤에 한 번 더 확실하게 교정
        yield return new WaitForSeconds(1.0f);
        Recenter();
    }

    void Update()
    {
        // 개발 중에 키보드로 강제 정렬 테스트
        if (Input.GetKeyDown(recenterKey))
        {
            Recenter();
        }

        // 오큘러스 오른쪽 컨트롤러 A버튼을 길게 눌러도 정렬되게 하려면:
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            // Recenter(); // 필요하면 주석 해제
        }
    }

    public void Recenter()
    {
        if (cockpitForward == null || ovrCameraRig == null || centerEyeAnchor == null) return;

        // 1. 현재 카메라(눈)가 월드에서 보고 있는 Y축 각도
        float currentHeadY = centerEyeAnchor.eulerAngles.y;

        // 2. 콕핏(목표)이 보고 있는 Y축 각도
        float targetY = cockpitForward.eulerAngles.y;

        // 3. 그 차이만큼(오차) 계산
        float difference = targetY - currentHeadY;

        // 4. OVRCameraRig 자체를 그 차이만큼 돌려서 보정
        // (현재 회전값 + 오차)
        ovrCameraRig.Rotate(0, difference, 0);

        Debug.Log($"시점 강제 정렬 완료! (보정각도: {difference})");
    }
}