using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Game : MonoBehaviour
{
    // lunghezza e larghezza della grid
    public int widht = 16;
    public int height = 16;
    // numero mine esempio
    public int mineCount = 32;
    // variabile board per richiamare lo script board
    private Board board;

    private Cell[,] state;

    private bool gameover;

    private bool minesGenerated = false;


    // svegliamo la grid
    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    // questa è il main, la prima funzione chiamata da unity
    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        // creiamo la matrice di celle che popoleranno la grid del gioco
        state = new Cell[widht, height];
        // appena iniziamo il gioco assicuriamoci che la variabile game over sia false
        gameover = false;
        minesGenerated = false;

        GenerateCells();


        // se notiamo e non mettiamo questo la camera sarà fuori dal centro se aumentiamo i parametri, cosi siamo sicuri sia centratata
        Camera.main.transform.position = new Vector3 (widht/2f, height/2f, -10f);

        board.Draw(state);
    }

    // generiamo le cell data
    private void GenerateCells()
    {
        for (int x = 0; x < widht; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                // ignoriamo la z
                cell.position = new Vector3Int(x, y, 0);
                // al inizio la cella èvuota
                cell.type = Cell.Type.Empty;
                // ora che abbiamo generato tutto quelle che serve per una cella possiamo assegnarla nella matrice
                state[x, y] = cell;

            }
        }
    }


    //generating the cell


    //generating the mines

    // Genera mine evitando una posizione (e le sue 8 vicine) — usata al primo click
    private void GenerateMinesAvoiding(Vector2Int avoidPos)
    {
        HashSet<int> occupied = new HashSet<int>(); // store encoded positions already mines
        // precompute forbidden positions (avoidPos and neighbors)
        HashSet<int> forbidden = new HashSet<int>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = avoidPos.x + dx;
                int ny = avoidPos.y + dy;
                if (IsValid(nx, ny))
                {
                    forbidden.Add(nx + ny * widht);
                }
            }
        }

        // place mines randomly but not in forbidden and no duplicates
        int placed = 0;
        int attempts = 0;
        int maxAttempts = mineCount * 20 + widht * height; // safety break

        System.Random rng = new System.Random();

        while (placed < mineCount && attempts < maxAttempts)
        {
            attempts++;
            int x = rng.Next(0, widht);
            int y = rng.Next(0, height);
            int code = x + y * widht;
            if (forbidden.Contains(code) || occupied.Contains(code))
                continue;

            // place mine
            state[x, y].type = Cell.Type.Mine;
            occupied.Add(code);
            placed++;
        }

        // In rare case we couldn't place enough (shouldn't happen), fill sequentially skipping forbidden
        if (placed < mineCount)
        {
            for (int x = 0; x < widht && placed < mineCount; x++)
            {
                for (int y = 0; y < height && placed < mineCount; y++)
                {
                    int code = x + y * widht;
                    if (forbidden.Contains(code) || occupied.Contains(code)) continue;
                    state[x, y].type = Cell.Type.Mine;
                    occupied.Add(code);
                    placed++;
                }
            }
        }

        // dopo aver piazzato le mine, generiamo i numeri
        GenerateNumbers();
        minesGenerated = true;
    }



    //importante che i numeri siano generati dopo le mine

    //genereting the numbers
    private void GenerateNumbers()
    {
        for (int x = 0; x < widht; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if(cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                cell.number = CountMines(x, y);
                // dopo il conto mettiamo il numero della cella
                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                state[x, y] = cell;
            }
        }
    }


    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        // se siamo all edge of the board il ciclo andra out of bound
        for (int adjecntX = -1; adjecntX<=1;adjecntX++)
        {

            for (int adjecntY = -1; adjecntY <= 1; adjecntY++)
            {
                // se siamo entrambi a 0 siamo nella casella del numero
                if(adjecntX ==0 && adjecntY == 0)
                {
                    continue;
                }

                int x = cellX + adjecntX;
                int y = cellY + adjecntY;


                if (GetCell(x,y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }



        return count;
    }

    // la preparazione del gioco è finita

    // ora gestiamo l interazione con l utente
    // update is built in and  is callde every single frame 
    private void Update()
    {
        // se il gioco è finito per una ragione mettiamo un tasto per restartare
        // con r restartiamo
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
        }
        // se abbiamo il gameover non si aggiorna piu niente
        else if (!gameover)
        {

            // right click
            // flagging
            if (Input.GetMouseButtonDown(1))
            {
                Flag();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }

        }
    }

    private void Flag()
    {
        // dobbiamo convertire la posizione del mouse in coordinate
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // ora convertiamo world position in posizione della cella
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        // ci potrebbe essere il caso nel cui il click avviene fuori dal grid, quindi risulterebbe invalido
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        // se è invalida la posizione o se la cella è gia stata rivelata andiamo avanti
        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }


        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }


    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        if (!IsValid(cellPosition.x, cellPosition.y)) return;

        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        // se la cella è invalida o è gia stata rivelata o è flaggata non va
        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        // se prima click: genera mine evitando la cella cliccata e le sue vicine
        if (!minesGenerated)
        {
            GenerateMinesAvoiding(new Vector2Int(cellPosition.x, cellPosition.y));
        }

        // ricarico la cella (ora che le mine sono generare il suo tipo potrebbe essere cambiato)
        cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Mine)
        {
            Explode(cell);
            board.Draw(state);
            return;
        }
        else if (cell.type == Cell.Type.Empty)
        {
            Flood(cell);
        }
        else // Number
        {
            cell.revealed = true;
            state[cell.position.x, cell.position.y] = cell;
        }

        board.Draw(state);
        CheckWinConditions();
    }


    // parte piu difficile 
    private void Flood(Cell cell)
    {
        // recursion. we need a stop or the function will repeat in loop
        // the function will call itself
        // le condizioni di stop del flooding sono se incontriamo una cella che è gia stat rivelata, se incontriamo una mina

        if (cell.revealed)
        {
            return;
        }
        if(cell.type ==  Cell.Type.Mine || cell.type ==  Cell.Type.Invalid)
        {
            return;
        }


        // se prendiamo un numero pero non vogliamo subito fermarci, vogliamo che il numero venga rivelato e n0n vogliamo continuare il flood
        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        // usiamo questa logica finche non esauriamo tutte le celle vicine 
        if ( cell.type == Cell.Type.Empty)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = cell.position.x + dx;
                    int ny = cell.position.y + dy;
                    if (!IsValid(nx, ny)) continue;
                    Flood(GetCell(nx, ny));
                }
            }
        }

    }


    private void Explode(Cell cell)
    {
        // magari facciamo un display qui di un menu
        Debug.Log("Game Over!");
        gameover = true;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        for (int x = 0; x < widht; x++)
        {
            for(int y = 0; y < height; y++)
            {
                cell = state[x, y];
                if(cell.type == Cell.Type.Mine)
                {

                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }

        // la prima cosa che voglio fare è che il game è over e non puo premre altre cose
        // vado a farlo nella funzione update
    }

    // funzione per la vittoria

    private void CheckWinConditions() 
    { 
        // se tutte le celle che non sono mine sono rivelate abbiamo vinto
        for (int x = 0;x<widht; x++)
        {
            for (int y = 0; y < widht; y++)
            {
                
                Cell cell = state[x, y];
                // se è vero sappiamo che non abbiamo vinto e usciamo
                if (cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }


            }
        }

        Debug.Log("Winner!");
        gameover = true; // così l utente non potrà piu fare nulla

        // cosi quando vinciamo flaggiamo tutte le mine
        for (int x = 0; x < widht; x++)
        {

            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {

                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }

    }

    // helper function per il controllo
    private Cell GetCell(int x, int y)
    {
        if (IsValid(x,y))
        {
            return state[x, y];
        }
        else
        {
            return new Cell();
        }
    }

    private bool IsValid( int x, int y)
    {
        return x >= 0 && x<widht &&  y>= 0 && y< height;
    }

}

