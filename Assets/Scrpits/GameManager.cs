using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using  Unity.Mathematics;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    //[SerializeField] private GameObject winPanel;


    public List<LevelSO> levels = new List<LevelSO>();
    
    private List<Transform> pieces = new List<Transform>();
    private int emptyLocation;
    private int size;
    private int step;
    public string name;
    private bool suffling = false;
    public bool isSuffled = false;
    public bool gameOver = false;
    
    

    // Tao game voi x size pieces
    private void CreateGamePieces(float gapThickness)
    {
        float width =  1 / (float)size;
        for(int row = 0; row < size; row++)
            for (int col = 0; col < size; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                pieces.Add(piece);
                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                    +1 - (2 * width * row) - width,
                    0);
                
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row*size+col)}";

                if (row == size - 1 && col == size - 1)
                {
                    emptyLocation = (size*size) - 1;
                    piece.gameObject.SetActive(false);
                }
                else
                {
                    float gap = gapThickness / 2;
                    Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                    Vector2[] uv = new Vector2[4];
                    
                    uv[0] = new Vector2((width * col) + gap, 1 - ((width*(row+1)) - gap));
                    uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width*(row+1)) - gap));
                    uv[2] = new Vector2((width * col) + gap, 1 - ((width*(row)) + gap));
                    uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width*(row)) - gap));
                    
                    mesh.uv = uv;
                }
            }
        
    }

    private bool SwapIfValid(int i, int offset, int colCheck)
    {
        if ((i % size) != colCheck && ((i + offset) == emptyLocation))
        {
            // Swap them in game state
            (pieces[i], pieces[i + offset]) = (pieces[i + offset], pieces[i]); 
            // Swap their transform
            (pieces[i].localPosition, pieces[i + offset].localPosition) = (pieces[i + offset].localPosition, pieces[i].localPosition);
            
            emptyLocation = i;
            return true;
        }
        return false; 
    }
    
    private IEnumerator MovePiece(Transform piece, Vector3 targetPos, float duration)
    {
        Vector3 startPos = piece.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            piece.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        piece.localPosition = targetPos;
    }

    public void ChoiceLevel()
    {
        if (UIManager.Instance.levelIsChoice <= 0 || UIManager.Instance.levelIsChoice > levels.Count)
        {
            Debug.LogError("Invalid level selected!");
            return;
        }
        Clear(); // 👈 THÊM DÒNG NÀY
        var level = levels[UIManager.Instance.levelIsChoice-1];
        UIManager.Instance.isCounting = true;
        piecePrefab = level.textrue.transform;
        step = level.step;
        size = level.size;
        name = level.name;
        UIManager.Instance.levelBoardUI.text = $"LEVEL {level.level}";
        UIManager.Instance.spriteWinUI.sprite = level.image;
        UIManager.Instance.imageOGUI.sprite = level.image;
        UIManager.Instance.bestTimeUI.text = level.time;
        CreateGamePieces(0.01f);
        CheckGameOver();
        
    }

    private void Update()
    {
        
        if (Input.GetMouseButtonDown(0) && !CheckComplete())
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                SoundManager.Instance.playClickSound(SoundManager.Instance.clickClip);
                for (int i = 0; i < pieces.Count; i++)
                {
                    if (pieces[i] == hit.transform)
                    {
                        if (SwapIfValid(i, -size, size))
                        {
                            break;
                        }

                        if (SwapIfValid(i, +size, size))
                        {
                            break;
                        }
                        if (SwapIfValid(i, -1, 0))
                        {
                            break;
                        }
                        if (SwapIfValid(i, +1, size-1))
                        {
                            break;
                        }
                    }
                    
                }
            }
            CheckGameOver();
        }
    }

    private bool CheckComplete()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].name != $"{i}")
            {
                return false;
            }
        }
        return true;
    }

    public void CheckGameOver()
    {
        if (!suffling && CheckComplete())
        {
            if (isSuffled)
            {
                OverGame();
            }
            else
            {
                StartGame();
            }
        }
    }

    public void StartGame()
    {
        suffling = true;
        StartCoroutine(WaitShuffle(0.5f));
    }

    public void OverGame()
    {
        gameOver = true;
        Debug.Log("Game Over");
        UIManager.Instance.isCounting = false;

        var level = levels[UIManager.Instance.levelIsChoice - 1];
        string newTime = UIManager.Instance.textTimer.text;

        LoadBestTime(level); // load best hiện tại

        if (level.time == "00:00" || CompareTime(newTime, level.time))
        {
            SaveBestTime(level, newTime); // chỉ lưu nếu tốt hơn
        }
    }
    
    private bool CompareTime(string newTime, string oldTime)
    {
        TimeSpan newTs = TimeSpan.Parse(newTime);
        TimeSpan oldTs = TimeSpan.Parse(oldTime);
        return newTs > oldTs;
    }
    
    
    private IEnumerator WaitShuffle(float duration)
    {
        yield return new WaitForSeconds(duration);
        Shuffle(step);
        suffling = false;

    }

    public void Shuffle(int step)
    {
        int maxAttempt = 1000;
        int attempt = 0;
        isSuffled = false;

        while (attempt < maxAttempt)
        {
            attempt++;
            int last = -1;

            for (int i = 0; i < step;)
            {
                int rnd = Random.Range(0, size * size);
                if (rnd == last) continue;

                if (SwapIfValid(rnd, -size, size) ||
                    SwapIfValid(rnd, +size, size) ||
                    SwapIfValid(rnd, -1, 0) ||
                    SwapIfValid(rnd, +1, size - 1))
                {
                    last = rnd;
                    i++;
                }
            }

            if (!CheckComplete()) // ❗Chỉ dừng khi khác hoàn chỉnh
            {
                isSuffled = true;
                break;
            }
        }

        if (!isSuffled)
            Debug.LogWarning("Shuffle không thành công sau nhiều lần thử");
    }

    public void Clear()
    {
        foreach (var piece in pieces)
        {
            if (piece != null)
                if (Application.isPlaying)
                    Destroy(piece.gameObject);
                else
                    DestroyImmediate(piece.gameObject);
        }
        pieces.Clear();
        emptyLocation = 0;
        suffling = false;
        isSuffled = false;
        gameOver = false;
        
    }
    
    public void RestartGame()
    {
        Clear();
        ChoiceLevel();
    }

    public void MainMenu()
    {
        Clear();
    }

    public void NextGame()
    {
        Clear();
        UIManager.Instance.levelIsChoice ++;
        ChoiceLevel();
    }
    
    private void LoadBestTime(LevelSO level)
    {
        string key = UIManager.Instance.textTimer.text;
        if (PlayerPrefs.HasKey(key))
        {
            level.time = PlayerPrefs.GetString(key);
        }
        else
        {
            level.time = "00:00";
        }
    }
    
    private void SaveBestTime(LevelSO level, string newTime)
    {
        string key = UIManager.Instance.textTimer.text;
        level.time = newTime;
        PlayerPrefs.SetString(key, newTime);
        PlayerPrefs.Save(); // ghi đè lên ổ đĩa
    }
}
