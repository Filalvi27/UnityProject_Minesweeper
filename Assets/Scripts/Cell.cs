using UnityEngine;

// data structure
public struct Cell
{
    // definiamo con un enum Type possiamo raggruppare valori di diverso tipo
    public enum Type
    { 

        // il primo valore dovrebbe essere quello di default
        Invalid,
        Empty,
        Mine,
        Number
    }

    // conoscere la posizione della cell nella board
    // lo facciamo con vector3, abbiamo cosi xyz interi
    public Vector3Int position;

    // vera e propria variabile per enum
    public Type type;
    // se c'è un  numero che numeroe è?
    public int number;
    // stato della cella
    public bool revealed;
    public bool flagged;
    public bool exploded;
}