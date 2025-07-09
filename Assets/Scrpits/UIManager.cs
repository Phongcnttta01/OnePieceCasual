using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] public TextMeshProUGUI textTimer;
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gamePlayUI;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject levelChoiceUI;
    //[SerializeField] private GameObject soundUI;
    [SerializeField] private TextMeshProUGUI victoryTimerUI;
    [SerializeField] private TextMeshProUGUI nameUI;
    [SerializeField] public TextMeshProUGUI levelBoardUI;
    [SerializeField] public Image spriteWinUI;
    [SerializeField] public Image imageOGUI;
    [SerializeField] public TextMeshProUGUI bestTimeUI;
    [SerializeField] public GameObject loadingUI;
    [SerializeField] public GameObject InfoUI;
   
    [SerializeField] private RectTransform leftPart;
    [SerializeField] private RectTransform rightPart;
    public float timer = 0f;
    public bool isCounting = false;
    [SerializeField] public int levelIsChoice;
    public bool isMuted = false;
    


    private void Update()
    {
        if (isCounting)
        {
            timer += Time.deltaTime;
            UpdateTimerText();
        }

        if (GameManager.Instance.isSuffled && GameManager.Instance.gameOver)
        {
            nameUI.text = GameManager.Instance.name;
            victoryUI.SetActive(true);
            victoryTimerUI.text = textTimer.text;
        }
    }
    
    public void StartTimer()
    {
        timer = 0f;
        isCounting = true;
    }

    public void ResetTimer()
    {
        timer = 0f;
        UpdateTimerText();
    }
    
    public void PauseTimer()
    {
        isCounting = false;
    }

    public void ResumeTimer()
    {
        isCounting = true;
    }
    
    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        textTimer.text = $"{minutes:00}:{seconds:00}";
    }
    
    

    public void PlaySplitTransition(System.Action onHalf, System.Action onComplete)
    {
        loadingUI.SetActive(true);

        float screenWidth = Screen.width;

        leftPart.anchoredPosition = new Vector2(-screenWidth, 0f);
        rightPart.anchoredPosition = new Vector2(screenWidth, 0f);

        Sequence seq = DOTween.Sequence();

        // Di chuyển 2 bên vào giữa
        seq.Append(leftPart.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutQuad));
        seq.Join(rightPart.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutQuad));

        // Gọi hành động nửa chừng (tắt levelChoice UI)
        seq.AppendCallback(() => {
            onHalf?.Invoke(); // chạy giữa hiệu ứng
        });

        // Giữ lại một chút
        seq.AppendInterval(0.3f);

        // Di chuyển 2 bên ra lại
        seq.Append(leftPart.DOAnchorPos(new Vector2(-screenWidth, 0f), 0.4f).SetEase(Ease.InQuad));
        seq.Join(rightPart.DOAnchorPos(new Vector2(screenWidth, 0f), 0.4f).SetEase(Ease.InQuad));

        seq.OnComplete(() =>
        {
            loadingUI.SetActive(false);
            onComplete?.Invoke();
        });
    }
    
    public void Restart()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        GameManager.Instance.RestartGame();
        pauseUI.SetActive(false);
        victoryUI.SetActive(false);
        PlaySplitTransition(
            onHalf: () =>
            {
                
            },
            onComplete: () =>
            {
                isCounting = true;
                timer = 0f;
            });
    }

    public void Pause()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        isCounting = false;
        pauseUI.SetActive(true);
    }

    public void Resume()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        isCounting = true;
        pauseUI.SetActive(false);
    }

    public void MainMenu()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        GameManager.Instance.MainMenu();
        victoryUI.SetActive(false);
        gamePlayUI.SetActive(false);
        pauseUI.SetActive(false);
        PlaySplitTransition(
            onHalf: () =>
            {
                GameManager.Instance.MainMenu();
                victoryUI.SetActive(false);
                gamePlayUI.SetActive(false);
                pauseUI.SetActive(false);
               levelChoiceUI.SetActive(true);
            },
            onComplete: () =>
            {
                isCounting = true;
                timer = 0f;
            });
    }

    public void NextLevel()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        if (levelIsChoice == 5)
        {
            PlaySplitTransition(
                onHalf: () =>
                {
                    victoryUI.SetActive(false);
                    gamePlayUI.SetActive(false);
                    pauseUI.SetActive(false);
                },
                onComplete: () =>
                {
                    InfoUI.SetActive(true);
                });
            return;
        }
        
        PlaySplitTransition(
            onHalf: () =>
            {
                victoryUI.SetActive(false);
                GameManager.Instance.NextGame();
            },
            onComplete: () =>
            {
                isCounting = true;
                timer = 0f;
            });
        
        
        
    }

    public void Mute()
    {
        if (!isMuted)
        {
            SoundManager.Instance.musicSource.volume = 0f;
            isMuted = true;
        }
        else
        {
            SoundManager.Instance.musicSource.volume = 1f;
            isMuted = false;
        }
    }

    public void ChoiceLevel1()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        Debug.Log("Choosing level 1");
        levelIsChoice = 1;
        gamePlayUI.SetActive(true);
        //GameManager.Instance.isStart = true;
        GameManager.Instance.ChoiceLevel();
        
        PlaySplitTransition(
            onHalf: () =>
            {
                levelChoiceUI.SetActive(false); // ← Tắt UI khi loading mới nửa chừng
            },
            onComplete: () =>
            {
                isCounting = true;
                timer = 0f;
            });
    }
    
    public void ChoiceLevel2()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        levelIsChoice = 2;
        gamePlayUI.SetActive(true);
        //GameManager.Instance.isStart = true;
        GameManager.Instance.ChoiceLevel();
        
        PlaySplitTransition(
            onHalf: () =>
            {
                levelChoiceUI.SetActive(false); // ← Tắt UI khi loading mới nửa chừng
            },
            onComplete: () =>
            {
                isCounting = true;
                timer = 0f;
            });
    }
    
    public void ChoiceLevel3()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        levelIsChoice = 3;
        gamePlayUI.SetActive(true);
        //GameManager.Instance.isStart = true;
        GameManager.Instance.ChoiceLevel();
        
        PlaySplitTransition(
            onHalf: () =>
            {
                levelChoiceUI.SetActive(false); // ← Tắt UI khi loading mới nửa chừng
            },
            onComplete: () =>
            {
                isCounting = true;
                timer = 0f;
            });
    }
    
    public void ChoiceLevel4()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        levelIsChoice = 4;
        gamePlayUI.SetActive(true);
        //GameManager.Instance.isStart = true;
        GameManager.Instance.ChoiceLevel();
        
        PlaySplitTransition(
            onHalf: () =>
            {
                levelChoiceUI.SetActive(false); // ← Tắt UI khi loading mới nửa chừng
            },
            onComplete: () =>
            {
                isCounting = true;
                timer = 0f;
            });
    }
    
    public void ChoiceLevel5()
    {
        SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
        levelIsChoice = 5;
        gamePlayUI.SetActive(true);
        //GameManager.Instance.isStart = true;
        GameManager.Instance.ChoiceLevel();
        
        PlaySplitTransition(
            onHalf: () =>
            {
                levelChoiceUI.SetActive(false); // ← Tắt UI khi loading mới nửa chừng
            },
            onComplete: () =>
            {
                isCounting = true;
                timer = 0f;
            });
    }
}
