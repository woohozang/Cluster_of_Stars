using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class FinalEndingEvent : MonoBehaviour
{
    [Header("0. OVR 화면 페이드 설정")]
    // [변경] Image 대신 OVRScreenFade를 사용합니다.
    public OVRScreenFade screenFade;

    [Header("1. 별자리 이미지 & 부모 캔버스")]
    public CanvasGroup starCanvasGroup;
    public Image[] constellationImages;

    [Header("2. 중앙 3D 별")]
    public Transform targetStar;
    public SkinnedMeshRenderer starMesh;

    [Header("3. 엔딩 UI")]
    public TextMeshProUGUI endText;
    public Image endImage;

    [Header("4. 이펙트 설정")]
    public GameObject lightProjectilePrefab;
    public float lightSpeed = 8.0f;

    [Header("오디오")]
    public AudioSource audioSource;
    public AudioClip appearSound;
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip explosionSound;

    private bool isEndingStarted = false;

    private void Start()
    {
        // 1. 시작할 때 화면이 투명해지도록 (혹시 몰라 초기화)
        if (screenFade != null) screenFade.FadeIn();

        // 2. 부모 캔버스 켜기
        if (starCanvasGroup != null)
        {
            starCanvasGroup.alpha = 1f;
            starCanvasGroup.gameObject.SetActive(true);
        }

        // 3. 별자리 이미지 투명 초기화
        foreach (var img in constellationImages)
        {
            if (img != null)
            {
                SetImageAlpha(img, 0f);
                img.gameObject.SetActive(true);
            }
        }

        // 4. 엔딩 텍스트 초기화
        if (endText != null)
        {
            SetTextAlpha(endText, 0f);
            endText.gameObject.SetActive(true);
        }
        if (endImage != null)
        {
            SetImageAlpha(endImage, 0f);
            endImage.gameObject.SetActive(true);
        }

        if (starMesh) starMesh.SetBlendShapeWeight(0, 100f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEndingStarted) return;

        if (other.CompareTag("Player") || other.name.Contains("Player") || other.name.Contains("Camera"))
        {
            isEndingStarted = true;
            Debug.Log("🚀 엔딩 시퀀스 시작!");
            StartCoroutine(ProcessEndingSequence());
        }
    }

    IEnumerator ProcessEndingSequence()
    {
        // === [1단계] 별자리 이미지 순차 페이드 인 ===
        for (int i = 0; i < constellationImages.Length; i++)
        {
            if (constellationImages[i] != null)
            {
                if (audioSource && appearSound) audioSource.PlayOneShot(appearSound);
                yield return StartCoroutine(FadeImage(constellationImages[i], 0f, 1f, 1.0f));
            }
            yield return new WaitForSeconds(0.2f);

            if (constellationImages[i] != null)
            {
                yield return StartCoroutine(ShootLight(constellationImages[i].transform.position));
            }

            float targetWeight = 100f - ((i + 1) * 25.0f);
            yield return StartCoroutine(ChangeShapeKey(targetWeight));

            if (audioSource && hitSound) audioSource.PlayOneShot(hitSound);

            yield return new WaitForSeconds(0.3f);
        }

        // === [2단계] 피날레 화이트 아웃 (OVRScreenFade 사용) ===
        yield return new WaitForSeconds(1.0f);
        if (audioSource && explosionSound) audioSource.PlayOneShot(explosionSound);

        // ★ FadeOut()이 화면을 "색깔로 채우는" 함수입니다. (투명 -> 흰색)
        if (screenFade != null) screenFade.FadeOut();
        yield return new WaitForSeconds(2.0f); // 완전히 하얘질 때까지 대기

        // === [3단계] 교체 작업 (하얀 화면 뒤에서) ===
        if (targetStar) targetStar.gameObject.SetActive(false);

        foreach (var img in constellationImages)
        {
            if (img != null)
            {
                SetImageAlpha(img, 0f);
                img.gameObject.SetActive(false);
            }
        }

        // === [4단계] 화이트 인 (화면 밝아짐) ===
        // ★ FadeIn()이 화면을 "투명하게 만드는" 함수입니다. (흰색 -> 투명)
        if (screenFade != null) screenFade.FadeIn();
        yield return new WaitForSeconds(2.0f); // 밝아지는 시간 대기

        // === [5단계] 엔딩 텍스트 등장 ===
        StartCoroutine(FadeText(endText, 0f, 1f, 2.0f));
        if (endImage != null) StartCoroutine(FadeImage(endImage, 0f, 1f, 2.0f));

        yield return new WaitForSeconds(2.0f);
    }

    // --- Helper 함수들은 기존과 동일 ---
    IEnumerator FadeImage(Image target, float start, float end, float duration)
    {
        if (target == null) yield break;
        target.gameObject.SetActive(true);
        float timer = 0f;
        Color c = target.color;
        c.a = start;
        target.color = c;
        while (timer < 1f)
        {
            timer += Time.deltaTime / duration;
            c.a = Mathf.Lerp(start, end, timer);
            target.color = c;
            yield return null;
        }
        c.a = end;
        target.color = c;
    }

    IEnumerator FadeText(TextMeshProUGUI target, float start, float end, float duration)
    {
        if (target == null) yield break;
        target.gameObject.SetActive(true);
        float timer = 0f;
        Color c = target.color;
        c.a = start;
        target.color = c;
        while (timer < 1f)
        {
            timer += Time.deltaTime / duration;
            c.a = Mathf.Lerp(start, end, timer);
            target.color = c;
            yield return null;
        }
        c.a = end;
        target.color = c;
    }

    void SetImageAlpha(Image target, float alpha)
    {
        if (target == null) return;
        Color c = target.color;
        c.a = alpha;
        target.color = c;
    }

    void SetTextAlpha(TextMeshProUGUI target, float alpha)
    {
        if (target == null) return;
        Color c = target.color;
        c.a = alpha;
        target.color = c;
    }

    IEnumerator ShootLight(Vector3 startPos)
    {
        if (audioSource && shootSound) audioSource.PlayOneShot(shootSound);
        GameObject projectile = Instantiate(lightProjectilePrefab, startPos, Quaternion.identity);
        Vector3 dest = targetStar != null ? targetStar.position : transform.position;
        while (Vector3.Distance(projectile.transform.position, dest) > 0.1f)
        {
            projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, dest, Time.deltaTime * lightSpeed);
            yield return null;
        }
        Destroy(projectile);
    }

    IEnumerator ChangeShapeKey(float targetVal)
    {
        if (starMesh == null) yield break;
        float currentVal = starMesh.GetBlendShapeWeight(0);
        float timer = 0f;
        while (timer < 1f) { timer += Time.deltaTime * 2.0f; starMesh.SetBlendShapeWeight(0, Mathf.Lerp(currentVal, targetVal, timer)); yield return null; }
        starMesh.SetBlendShapeWeight(0, targetVal);
    }
}