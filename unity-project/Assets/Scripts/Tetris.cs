using UnityEngine;

// Minimal 2D Tetris. The whole scene (camera, board, pieces, UI) is built in code at runtime.
public class Tetris : MonoBehaviour
{
    const int W = 10, H = 20;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindAnyObjectByType<Tetris>() == null)
            new GameObject("Tetris").AddComponent<Tetris>();
    }

    // 7 tetrominoes (I, O, T, S, Z, J, L) as cell offsets.
    static readonly Vector2Int[][] Shapes =
    {
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) }, // I
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) },  // O
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) }, // T
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // S
        new[] { new Vector2Int(-1,1), new Vector2Int(0,1), new Vector2Int(0,0), new Vector2Int(1,0) }, // Z
        new[] { new Vector2Int(-1,1), new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0) },// J
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1) }, // L
    };

    static readonly Color[] Colors =
    {
        new Color(0.0f, 0.9f, 0.95f), new Color(0.95f, 0.9f, 0.1f), new Color(0.65f, 0.2f, 0.9f),
        new Color(0.2f, 0.85f, 0.2f), new Color(0.95f, 0.2f, 0.2f), new Color(0.2f, 0.35f, 0.95f),
        new Color(0.95f, 0.55f, 0.1f),
    };
    static readonly Color EmptyColor = new Color(0.12f, 0.12f, 0.16f);

    int[,] grid = new int[W, H];          // 0 = empty, otherwise piece index + 1
    SpriteRenderer[,] cells = new SpriteRenderer[W, H];
    Vector2Int[] piece; int pieceType; Vector2Int pos;
    int nextType;
    int score, lines, level = 1;
    float fallTimer, repeatTimer;
    bool gameOver;
    GUIStyle style, bigStyle;

    void Start()
    {
        Application.targetFrameRate = 60;

        // Camera
        var cam = new GameObject("Main Camera").AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = H / 2f + 1f;
        cam.transform.position = new Vector3(W / 2f - 0.5f + 3f, H / 2f - 0.5f, -10f);
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Square sprite generated in code (white with a soft darker border)
        const int S = 16;
        var tex = new Texture2D(S, S) { filterMode = FilterMode.Point };
        for (int x = 0; x < S; x++)
            for (int y = 0; y < S; y++)
            {
                bool border = x == 0 || y == 0 || x == S - 1 || y == S - 1;
                tex.SetPixel(x, y, border ? new Color(0.6f, 0.6f, 0.6f) : Color.white);
            }
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);

        // Board frame
        var frame = new GameObject("Frame").AddComponent<SpriteRenderer>();
        frame.sprite = sprite; frame.color = new Color(0.5f, 0.5f, 0.6f);
        frame.transform.position = new Vector3(W / 2f - 0.5f, H / 2f - 0.5f, 1f);
        frame.transform.localScale = new Vector3(W + 0.4f, H + 0.4f, 1f);
        frame.sortingOrder = -1;

        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                var sr = new GameObject($"Cell {x},{y}").AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.transform.position = new Vector3(x, y, 0);
                cells[x, y] = sr;
            }

        Restart();
    }

    void Restart()
    {
        grid = new int[W, H];
        score = 0; lines = 0; level = 1; gameOver = false;
        nextType = Random.Range(0, 7);
        Spawn();
    }

    void Spawn()
    {
        pieceType = nextType;
        nextType = Random.Range(0, 7);
        piece = (Vector2Int[])Shapes[pieceType].Clone();
        pos = new Vector2Int(W / 2 - 1, H - 2);
        fallTimer = 0;
        if (!Fits(piece, pos)) gameOver = true;
    }

    bool Fits(Vector2Int[] p, Vector2Int at)
    {
        foreach (var c in p)
        {
            int x = c.x + at.x, y = c.y + at.y;
            if (x < 0 || x >= W || y < 0) return false;
            if (y < H && grid[x, y] != 0) return false;
        }
        return true;
    }

    bool TryMove(Vector2Int d)
    {
        if (!Fits(piece, pos + d)) return false;
        pos += d; return true;
    }

    void Rotate()
    {
        if (pieceType == 1) return; // O does not rotate
        var r = new Vector2Int[4];
        for (int i = 0; i < 4; i++) r[i] = new Vector2Int(-piece[i].y, piece[i].x);
        foreach (var kick in new[] { 0, -1, 1, -2, 2 })
        {
            var p = pos + new Vector2Int(kick, 0);
            if (Fits(r, p)) { piece = r; pos = p; return; }
        }
    }

    void Lock()
    {
        foreach (var c in piece)
        {
            int x = c.x + pos.x, y = c.y + pos.y;
            if (y >= H) { gameOver = true; return; }
            grid[x, y] = pieceType + 1;
        }
        int cleared = 0;
        for (int y = 0; y < H; y++)
        {
            bool full = true;
            for (int x = 0; x < W; x++) if (grid[x, y] == 0) { full = false; break; }
            if (!full) continue;
            cleared++;
            for (int yy = y; yy < H - 1; yy++)
                for (int x = 0; x < W; x++) grid[x, yy] = grid[x, yy + 1];
            for (int x = 0; x < W; x++) grid[x, H - 1] = 0;
            y--;
        }
        int[] points = { 0, 100, 300, 500, 800 };
        score += points[cleared] * level;
        lines += cleared;
        level = 1 + lines / 10;
        Spawn();
    }

    void Update()
    {
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R))
                Restart();
            Draw();
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) { TryMove(Vector2Int.left); repeatTimer = 0.18f; }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) { TryMove(Vector2Int.right); repeatTimer = 0.18f; }
        bool holdL = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        bool holdR = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        if (holdL || holdR)
        {
            repeatTimer -= Time.deltaTime;
            if (repeatTimer <= 0) { TryMove(holdL ? Vector2Int.left : Vector2Int.right); repeatTimer = 0.05f; }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Rotate();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int d = 0;
            while (TryMove(Vector2Int.down)) d++;
            score += d * 2;
            Lock();
            Draw();
            return;
        }

        bool soft = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        float interval = Mathf.Max(0.08f, 0.8f - (level - 1) * 0.07f);
        if (soft) interval = Mathf.Min(interval, 0.05f);
        fallTimer += Time.deltaTime;
        if (fallTimer >= interval)
        {
            fallTimer = 0;
            if (TryMove(Vector2Int.down)) { if (soft) score += 1; }
            else Lock();
        }
        Draw();
    }

    void Draw()
    {
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                cells[x, y].color = grid[x, y] == 0 ? EmptyColor : Colors[grid[x, y] - 1];
        if (gameOver || piece == null) return;

        // Ghost piece
        var g = pos;
        while (Fits(piece, g + Vector2Int.down)) g += Vector2Int.down;
        foreach (var c in piece)
        {
            int x = c.x + g.x, y = c.y + g.y;
            if (y < H && grid[x, y] == 0) cells[x, y].color = Color.Lerp(EmptyColor, Colors[pieceType], 0.3f);
        }
        foreach (var c in piece)
        {
            int x = c.x + pos.x, y = c.y + pos.y;
            if (y < H) cells[x, y].color = Colors[pieceType];
        }
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(14, Screen.height / 30) };
            style.normal.textColor = Color.white;
            bigStyle = new GUIStyle(style) { fontSize = Mathf.Max(24, Screen.height / 14), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            bigStyle.normal.textColor = Color.white;
        }
        float px = Screen.width * 0.62f, py = Screen.height * 0.08f, lh = style.fontSize * 1.6f;
        GUI.Label(new Rect(px, py, 400, lh), "TETRIS", style);
        GUI.Label(new Rect(px, py + lh * 1.5f, 400, lh), "Score: " + score, style);
        GUI.Label(new Rect(px, py + lh * 2.5f, 400, lh), "Lines: " + lines, style);
        GUI.Label(new Rect(px, py + lh * 3.5f, 400, lh), "Level: " + level, style);
        GUI.Label(new Rect(px, py + lh * 4.5f, 400, lh), "Next: " + "IOTSZJL"[nextType], style);
        GUI.Label(new Rect(px, py + lh * 6.5f, 500, lh * 6),
            "Left/Right: Move\nUp: Rotate\nDown: Soft drop\nSpace: Hard drop", style);
        if (gameOver)
        {
            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, Screen.height * 0.15f), "GAME OVER", bigStyle);
            GUI.Label(new Rect(0, Screen.height * 0.5f, Screen.width, Screen.height * 0.1f), "Press Space to restart",
                new GUIStyle(style) { alignment = TextAnchor.MiddleCenter });
        }
    }
}
