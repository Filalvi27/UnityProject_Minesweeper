using UnityEngine;
using UnityEngine.Tilemaps; // dato che lavoriamo con una reference alla talemap è importante importare questa libreria



// ci serve per updatare la gameboard, gestirla visualmente, nello script cell invece conteniamo e gestiamo dati
// la visualizzazione e la gestione dati sono separati ma collaborabo

public class Board : MonoBehaviour
{
    // public getter, private setter    le altre cose accedono alla tilemap ma non devono essere in grado di cambiarla
    public Tilemap tilemap { get; private set; }
    // queste sono tutti i tipi di tile ( in ognuno di questo casi abbiamo uno sprite diverso sulla cella)
    public Tile tileUnknown;
    public Tile tileEmpty;
    public Tile tileMine;
   // public Tile tileExploded;
    public Tile tileFlag;
    public Tile tileNum1;

    public Tile tileNum2;

    public Tile tileNum3;

    //public Tile tileNum4;

   // public Tile tileNum5;

    //public Tile tileNum6;

   // public Tile tileNum7;

   // public Tile tileNum8;


    // built in function of unity
    // this is going to be called whenever unity inizialize the object when the game runs
    private void Awake()
    {
        tilemap =GetComponent<Tilemap>(); // it s going to look at the same game object and find component of the map and assign there in the variable
    }

    // ogni volta che lo stato del gioco cambia questo riscrive le caselle 
    // matrice di celle( sarebbe la grid, la tilemap)ù
    // ogni cella ha i dati che abbiamo messo nello script Cell
    public void Draw(Cell[,] state)
    {
        // abbiamo 2 lunghezze
        int width = state.GetLength(0); // 0 e 1 sono le posizione della matrice
        int height = state.GetLength(1);

        // x e y sono le coordinate
        // riempiamo tutte le celle o quelle che ci servono in questo modo

        for (int x= 0; x < width; x++)
        {
            for(int y=0; y < height; y++)
            {
                Cell cell = state[x,y];
                tilemap.SetTile(cell.position, GetTile(cell));
            }
        }

    }

    
    private Tile GetTile(Cell cell)
    {
        
        // se la cella è stata revelead dobbiamo fare e returnare qualcosa nella tile, cioè una delle public Tile che abbiamo generato sopra e a cui abbiamo assegnato lo sprite
        if (cell.revealed)
        {
            return GetRevealedTile(cell);

        } else if(cell.flagged){
            return tileFlag; 
        }
        else
        {
            return tileUnknown;
        }

        

    }

    // utilizziamo un altra funzione per separare la logica delle cella rivelate
    private Tile GetRevealedTile(Cell cell)
    {
        // empty, mine, number di prima nel Type
        switch(cell.type)
        {
            
            case Cell.Type.Empty: return tileEmpty;
            case Cell.Type.Mine: return tileMine;
            case Cell.Type.Number: return GetNumberTile(cell); // ora possiamo avere più numeri. creiamo una nuova funzione per separare la logica

            default: return null;
        }
    }

    public Tile GetNumberTile(Cell cell)
    {
        switch (cell.number)
        {
            case 1: return tileNum1;
            case 2: return tileNum2;
            case 3: return tileNum3;
                /*
            case 4: return tileNum4;
            case 5: return tileNum5;
            case 6: return tileNum6;
            case 7: return tileNum7;
            case 8: return tileNum8;
                */
            default: return null;
        }
    }
    
}

// prima della board dobbiamo avere una riferenza alla talemap so we can draw the correct tiles on it according to the data



// in pratica qui verifichiamo lo stato della cella, e di conseguenza mettiamo lo sprite giusto e facciamo quello che dobbiamo