using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FinalEndingEvent : MonoBehaviour
{
    [Header("1. 별자리 오브젝트 (표시할 4개)")]
    // 엔딩 때 EndTrigger 주변에 띄울 별자리 이미지들 (미리 배치하고 꺼두세요)
    public GameObject[] constellationObjs;

    [Header("2. 중앙 3D 별")]
    public Transform targetStar;          // 빛이 도착할 ReflectStar_1
    public SkinnedMeshRenderer starMesh;  // 쉐이프키 조절용 (없으면 비워두세요)

    [Header("3. 엔딩 UI")]
    public GameObject finalCanvas;        // "The End" 캔버스
    public Image whiteFadePanel;          // 화이트아웃용 패널

    [Header("4. 이펙트 설정")]
    public GameObject lightProjectilePrefab; // 날아갈 빛 프리팹
    public float lightSpeed = 8.0f;          // 빛 속도

    [Header("오디오")]
    public AudioSource audioSource;
    public AudioClip appearSound;
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip explosionSound;

    private bool isEndingStarted = false;

    // 플레이어가 EndTrigger에 닿으면 시작
    private void OnTriggerEnter(Collider other)
    {
        if (isEndingStarted) return; // 이미 시작했으면 중복 실행 방지

        // 플레이어 태그 확인 (필요시 "Player" 등으로 수정)
        if (other.CompareTag("Player") || other.name.Contains("Player") || other.name.Contains("Camera"))
        {
            isEndingStarted = true;
            StartCoroutine(ProcessEndingSequence());
        }
    }

    IEnumerator ProcessEndingSequence()
    {
        // 0. 쉐이프키 초기화
        if (starMesh) starMesh.SetBlendShapeWeight(0, 0);

        // 1. 4개의 별자리 순차 실행
        for (int i = 0; i < constellationObjs.Length; i++)
        {
            // (1) 별자리 오브젝트 켜기 (나타남)
            if (constellationObjs[i] != null)
            {
                constellationObjs[i].SetActive(true);
                if (audioSource && appearSound) audioSource.PlayOneShot(appearSound);
            }

            yield return new WaitForSeconds(0.5f); // 잠시 대기

            // (2) 빛 발사 (별자리 위치 -> 중앙 별)
            if (constellationObjs[i] != null)
            {
                yield return StartCoroutine(ShootLight(constellationObjs[i].transform.position));
            }

            // (3) 3D 별 쉐이프키 채우기 (25%씩)
            float targetWeight = (i + 1) * 25.0f;
            yield return StartCoroutine(FillStarShapeKey(targetWeight));

            if (audioSource && hitSound) audioSource.PlayOneShot(hitSound);

            yield return new WaitForSeconds(0.5f);
        }

        // 2. 피날레 대기
        yield return new WaitForSeconds(1.0f);

        // 3. 화이트 아웃 (눈부심)
        if (audioSource && explosionSound) audioSource.PlayOneShot(explosionSound);
        yield return StartCoroutine(FadeScreen(0, 1, 2.0f)); // 투명 -> 하양

        // 4. 오브젝트 정리 (별자리, 3D별 숨기기)
        if (targetStar) targetStar.gameObject.SetActive(false);
        foreach (var obj in constellationObjs) if (obj) obj.SetActive(false);

        // 5. 엔딩 크레딧 켜기
        if (finalCanvas) finalCanvas.SetActive(true);

        // 6. 화이트 인 (하얀색 걷히기)
        yield return StartCoroutine(FadeScreen(1, 0, 2.0f)); // 하양 -> 투명
    }

    // 빛 날리기
    IEnumerator ShootLight(Vector3 startPos)
    {
        if (audioSource && shootSound) audioSource.PlayOneShot(shootSound);

        GameObject projectile = Instantiate(lightProjectilePrefab, startPos, Quaternion.identity);

        // 타겟이 없으면 자기 자신(EndTrigger)으로 날아감
        Vector3 dest = targetStar != null ? targetStar.position : transform.position;

        while (Vector3.Distance(projectile.transform.position, dest) > 0.1f)
        {
            projectile.transform.position = Vector3.MoveTowards(
                projectile.transform.position,
                dest,
                Time.deltaTime * lightSpeed
            );
            yield return null;
        }
        Destroy(projectile);
    }

    // 쉐이프키 애니메이션
    IEnumerator FillStarShapeKey(float targetVal)
    {
        if (starMesh == null) yield break;

        float currentVal = starMesh.GetBlendShapeWeight(0);
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * 2.0f; // 속도
            starMesh.SetBlendShapeWeight(0, Mathf.Lerp(currentVal, targetVal, timer));
            yield return null;
        }
        starMesh.SetBlendShapeWeight(0, targetVal);
    }

    // 화면 페이드
    IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (whiteFadePanel == null) yield break;
        whiteFadePanel.gameObject.SetActive(true);

        float timer = 0f;
        Color c = whiteFadePanel.color;

        while (timer < 1f)
        {
            timer += Time.deltaTime / duration;
            c.a = Mathf.Lerp(startAlpha, endAlpha, timer);
            whiteFadePanel.color = c;
            yield return null;
        }
        if (endAlpha == 0) whiteFadePanel.gameObject.SetActive(false);
    }
}