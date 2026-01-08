using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TapGame : MonoBehaviour
{
    private bool firstGlowDone = false;

    [Header("Panels")]
    [SerializeField] GameObject titlePanel;
    [SerializeField] GameObject gamePanel;

    [Header("Time Limit")]
    public float timeLimit = 30f;
    public TMP_Text timeText;

    private float currentTime;
    private bool isGameOver = false;

    [Header("Speed Settings")]
    public float waitTime = 4.5f;        // 出現までの初期時間
    public float glowTime = 1.2f;        // 光っている時間
    public float speedUpRate = 0.03f;    // 1回成功ごとに短くする量

    public float minWaitTime = 5.0f;     // 最小待ち時間
    public float minGlowTime = 2.0f;     // 最小光時間

    [Header("Button Move")]
    public RectTransform buttonRect;
    public Vector2 minPos;
    public Vector2 maxPos;

    [Header("UI References")]
    public Button tapButton;
    public Image glowImage;
    public TMP_Text scoreText;
    public TMP_Text resultText;

    private bool canTap = false;
    private int score = 0;

    [Header("Clear Condition")]
    public int clearScore = 40;

    void Awake()
    {
        Debug.Log("TapGame Awake : " + gameObject.name);
    }

    void Start()
    {
        // タイトル表示を先に
        titlePanel.SetActive(true);
        gamePanel.SetActive(false);

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        // ボタンのクリック登録
        tapButton.onClick.AddListener(OnTap);
    }

    public void StartGame()
    {
        score = 0;
        scoreText.text = "0";

        currentTime = timeLimit;
        isGameOver = false;
        canTap = false;
        firstGlowDone = false;   // ★追加

        tapButton.interactable = true;

        titlePanel.SetActive(false);
        gamePanel.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(GlowRoutine());
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        UpdateTimeText();

        if (currentTime <= 0f)
        {
            GameOver();
        }
    }

    public void OnTap()
    {
        Debug.Log($"OnTap 呼ばれた isGameOver={isGameOver}, canTap={canTap}");

        if (isGameOver) return;
        if (!canTap) return;   // 光っていない時は無視

        score++;
        scoreText.text = score.ToString();

        waitTime = Mathf.Max(minWaitTime, waitTime - speedUpRate);
        glowTime = Mathf.Max(minGlowTime, glowTime - speedUpRate);

        // canTap = false;  // 一度押したら、その点滅中はもう反応しないように
    }

    public void TestClick()
    {
        Debug.Log("BUTTON CLICKED");
    }

    IEnumerator GlowRoutine()
    {
        Debug.Log("GlowRoutine スタート");

        while (true)
        {
            // ★ 最初の1回だけは待たない
            if (firstGlowDone)
            {
                // 2回目以降は今まで通り待つ
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                firstGlowDone = true;
            }

            // ランダム位置に移動
            float x = Random.Range(minPos.x, maxPos.x);
            float y = Random.Range(minPos.y, maxPos.y);
            buttonRect.anchoredPosition = new Vector2(x, y);

            // 光る
            glowImage.color = new Color(1f, 1f, 0f, 1f);
            canTap = true;

            Debug.Log("光った！");

            yield return new WaitForSeconds(glowTime);

            // 消える
            glowImage.color = new Color(1f, 1f, 0f, 0f);
            canTap = false;
        }
    }

    void UpdateTimeText()
    {
        float t = Mathf.Max(0f, currentTime);
        timeText.text = "Time: " + t.ToString("F1");
    }

    void GameOver()
    {
        isGameOver = true;
        tapButton.interactable = false;
        canTap = false;

        StopAllCoroutines();
        glowImage.color = new Color(1f, 1f, 0f, 0f);

        if (score >= clearScore)
        {
            resultText.text = "CLEAR";
        }
        else
        {
            resultText.text = "GAME OVER";
        }

        resultText.gameObject.SetActive(true);
        StartCoroutine(FadeInAndScale(resultText));
    }

    IEnumerator FadeInAndScale(TMP_Text text)
    {
        float duration = 0.6f;
        float time = 0f;

        Color c = text.color;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        // 初期状態
        c.a = 0f;
        text.color = c;
        text.transform.localScale = startScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // フェード
            c.a = Mathf.Lerp(0f, 1f, t);
            text.color = c;

            // 拡大
            text.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        // 念のため最終値を固定
        c.a = 1f;
        text.color = c;
        text.transform.localScale = endScale;
    }
}
