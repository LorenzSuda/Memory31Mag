using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject card;
    [SerializeField] Vector3[] cards; //V3 perché ti sposti sulle carte
    [SerializeField] Texture2D[] images; //animali
    
    [SerializeField] float startX; //punto inizio distribuz. carte
    [SerializeField] float startY;
    [SerializeField] float planeZ;//asse su cui disegni carte
    [SerializeField] float deltaX = 1.1f; //distanza orizz tra le carte
    [SerializeField] float deltaY = 1.1f; //distanza vert tra le carte
    
    [SerializeField] private int columns = 5;
    [SerializeField] private int rows = 6;
    
    [SerializeField] GameObject winUI; //UI che compare quando vinci
    [SerializeField] AudioSource matchedPairAudioSource; //coppia trovata
    [SerializeField] GameObject ninja;
    [SerializeField] GameObject electricalSparksEffect; //effetto elettrico quando trovi una coppia
    
    int pairs; //coppie di carte

    InteractiveCard selectedCard1; //carta selezionata
    InteractiveCard selectedCard2;
    
    //animation
    
    private void Start()
    {
        //Check if we have enough images
        if (rows * columns != images.Length * 2) // per due perché le carte sono a coppie rinco!
        {
            Debug.LogWarning("Number of row * colum is not equal to provided cards, quit...");
            return;
        }
        
        pairs = columns * rows / 2; //ricalcola il numero di coppie

        //sistema per mescolare le immagini
        System.Random random = new System.Random();
        images = images.OrderBy(x => random.Next()).ToArray(); //qui usa Linq per "ordinare casualmente" le immagini (mischia)
                                                                        //toArray() per convertirlo in array visto che orderby restituisce
                                                                        //un IEnumerable
        
        
        cards = new Vector3 [rows * columns]; //crea array di V3 (x,y,z) per le posizioni delle carte

        //scialla qui sotto nel for in pratica invece di usare la mitica:
        // i = y * width + x per calcolare l'indice lineare in una griglia bidimensionale, usa 2 variabili: dx e dy
        //e un counter per contare le carte: infatti qui non stai in 2D (anno scorso) ma in 3D! ->
        //**MA OKKIO** quest'array è sempre bidimensionale, solo che la stai usando in 3D quando "aggiungi le posizioni sul planeZ!"
        float dx = startX;
        float dy = startY;

        int counter = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                cards[counter++] = new Vector3(dx, dy, planeZ); //e poi si salva la posizione claro no?
                dx += deltaX;
            }
            dx = startX;
            dy += deltaY;
        }

        cards = cards.OrderBy(x => random.Next()).ToArray(); 
        //qui rimischia di nuovo: ma proprio le carte per maggiore aleatorietà

        //Start creating (instantiate) cards, setting images etc
        counter = 0;

        int row = 0;
        foreach (Vector3 pos in cards) //vedi appunti perché il foreach va il doppio più lento del for! (app. 9apr)
        {
            GameObject go = Instantiate(card);

            //go.SetActive(true); //in teoria non serve, ma in pratica serve per attivare il prefab se non lo è

            go.transform.position = pos; //posizione della carta ->> okkio! non così banale: cards è un array di V3! ..."sapevatelo" (cardSSSS, esse)

            //We set card texture (using shader graph)
            go.GetComponent<MeshRenderer>().material.SetTexture("_MainTexture", images[row]); //imposta questa texture come
                                                                                                  //immagine dello shader
            
            #region FlashBackDelegati
            // Per la tua scarsa memoria. Con questa riga di codice stai dicendo:
            //
            //
            // 1. go.GetComponent<InteractiveCard>(): Ottieni il componente InteractiveCard associato al GameObject go.
            // 2. .OnClicked += SelectedCard;: Iscrivi il metodo SelectedCard all'evento OnClicked definito nella classe InteractiveCard.
            // 3. OnClicked: È un evento basato sul delegato ClickAction, che richiede un metodo con la firma (InteractiveCard card, bool selected).
            // 4. Quando l'evento OnClicked viene invocato, tutti i metodi iscritti (incluso SelectedCard) vengono eseguiti.
            //IN SINTESI: stai collegando il metodo SelectedCard all'evento OnClicked, che utilizza il delegato ClickAction per garantire (+=) che la firma sia corretta
            #endregion
            go.GetComponent<InteractiveCard>().OnClicked += SelectedCard;
            
            //We set Interactive card cover image name
            go.GetComponent<InteractiveCard>().imageName = images[row].name;
            
            go.GetComponent<InteractiveCard>().ninja = ninja; //passa il ninja al controller della carta
            go.GetComponent<InteractiveCard>().electricalSparksEffect = electricalSparksEffect; //passa l'effetto elettrico al controller della carta
            
            counter++;

            //Check if end of row, if this -> next row: la formula de l'altranno per sopostarsi "una carta sì e una no", come la scacchiera
            //sono due carte! 
            if (counter % 2 == 0)
            {
                row++;
            }
        }
    }
    
    //Questo metodo viene chiamato quando una carta viene selezionata: la singola carta dice al G.manager che è stata selezionata
    private void SelectedCard(InteractiveCard card, bool selected)
    {
        //null è usato come stato iniziale o di reset per indicare l'assenza di una selezione:
        // All'inizio, nessuna carta è "selected", quindi il valore predefinito è null
        if (selectedCard1 == null && selected) //clic sulla 1a card
        {
            selectedCard1 = card;
        }
        else if (selectedCard1 == card && !selected) //altro clic sulla stessa card
        {
            selectedCard1.ResetMe(); //se ho cliccato sulla stessa carta la resetto: la rigiro e mostro il retro
            selectedCard1 = null;
        }
        else if (selectedCard2 != null && card == selectedCard2 && !selected) //clic sulla 2a card e se l'hai ricliccata pure questa un'altra volta
        {
            selectedCard2.ResetMe(); //resetti pure questa...ma nn succederà "mai" perché andrebbe contro lo scopo del gioco: pensaci...FAI IL METODO CHEAT! ^^
            selectedCard2 = null;
        }
        //...infatti skipperesti subito qui a questa condizione
        else if (selectedCard2 == null && card != selectedCard1 && selected) //se la seconda carta è diversa dalla prima
        {
            selectedCard2 = card;

            if (selectedCard1.Compare(selectedCard2)) //qui confronti le due carte, 2 texture! suppongo vedi dopo...
            {
                //ok match!

                //matchedPairAudioSource.Play();

                selectedCard1.HideAndDestroy(); //hideANDdestroy distrugge la carta cambiando l'alpha dello shader
                selectedCard2.HideAndDestroy();

                selectedCard1 = null;
                selectedCard2 = null;

                pairs--;

                if (pairs == 0)
                {
                    //Game over

                    winUI.SetActive(true); //mostra la "canvas" di vittoria
                    //winUI.GetComponent<AudioSource>().Play();
                }
            }
            else
                {
                    //flip back
            
                    selectedCard1.ResetMe();
                    selectedCard2.ResetMe();
            
                    selectedCard1 = null;
                    selectedCard2 = null;
                }
        }
    } //okkio a ste parentesi cancella comment

    //CAMBIO SCENA
    //Questo metodo viene chiamato quando si clicca sul pulsante "Play Again" nella UI di vittoria
    public void PlayAgain()
    {
        //LoadScene resetta il gioco ricaricando la stessa scena che abbiamo caricato all'inizio. Infatti GetActiveScene() restituisce "il name" della scena attiva.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}