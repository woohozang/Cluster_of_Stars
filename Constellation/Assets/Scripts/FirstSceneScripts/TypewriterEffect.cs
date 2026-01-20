using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class TypewriterEffect : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text targetText;
    public GameObject Panel;
    public GameObject Jojak;
    public GameObject Lever;

    [Header("Settings")]
    public float typingSpeed = 0.1f;    // 글자 출력 속도 (0.1초)
    public float soundInterval = 1.0f;  // 타자 소리 간격 (1.0초)
    public float delayBetweenLines = 2.0f; // 문장 간 대기 시간 (2.0초)

    [Header("Content")]
    [TextArea(3, 10)]
    public string[] scenarioLines;

    [Header("Events")]
    public UnityEvent onSequenceFinished;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typingSound;      // 타자 칠 때 나는 소리 (탁... 탁...)
    public AudioClip lineChangeSound;  // [추가] 문장이 끝나고 대기할 때 나는 소리 (띠링! 또는 철컥!)

    void Start()
    {
        if (Jojak != null) Jojak.SetActive(false);
        if (Lever != null) Lever.SetActive(false);
        if (Panel != null) Panel.SetActive(true);

        if (targetText != null && scenarioLines.Length > 0)
        {
            StartCoroutine(ProcessLines());
        }
    }

    IEnumerator ProcessLines()
    {
        if (audioSource != null)
        {
            audioSource.pitch = 1.0f;
            audioSource.loop = false;
        }

        foreach (string line in scenarioLines)
        {
            targetText.text = "";
            float timeSinceLastSound = soundInterval;

            yield return null;

            // --- 1. 텍스트 타이핑 시작 ---
            foreach (char c in line)
            {
                targetText.text += c;

                // 타자 소리 (설정된 간격마다 재생)
                if (timeSinceLastSound >= soundInterval)
                {
                    if (audioSource != null && typingSound != null)
                    {
                        audioSource.PlayOneShot(typingSound);
                    }
                    timeSinceLastSound = 0f;
                }

                timeSinceLastSound += typingSpeed;
                yield return new WaitForSeconds(typingSpeed);
            }

            // --- 2. 문장 출력 끝! 대기 시간 시작 ---

            // [추가된 기능] 2초 대기하기 직전에 '줄바꿈 소리' 재생
            if (audioSource != null && lineChangeSound != null)
            {
                audioSource.PlayOneShot(lineChangeSound);
            }

            // 설정된 2.0초만큼 대기 (이때 위에서 재생한 소리가 들림)
            yield return new WaitForSeconds(delayBetweenLines);
        }

        Debug.Log("모든 텍스트 출력 완료!");
        targetText.text = "";

        if (Panel != null) Panel.SetActive(false);
        if (Jojak != null) Jojak.SetActive(true);
        if (Lever != null) Lever.SetActive(true);

        if (onSequenceFinished != null) onSequenceFinished.Invoke();
    }
}