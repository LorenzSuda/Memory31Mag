using UnityEngine;
using System.Collections;
//PER I POSTERI...quindi sempre te stesso: questa classe c'ha dentro una coroutine che si iscrive a un delegato per far flippare la carta
public class InteractiveCard : MonoBehaviour
{
    bool selected; //è selezionata?
    bool rotating = false; //è in rotazione?
    float fadeSpeed = 1; //velocità di rotazione
    //AudioSource audioSource;

    public delegate void ClickAction(InteractiveCard card, bool selected); //Il GM si sottoscrive e attende che carta comunichi che è selected
    public event ClickAction OnClicked; //evita di fare danni con la sottoscrizione/deiscrizione al delegato
    //**perché? vedi region FlashBackDelegati in GM**


    private string _imageName;

    public GameObject ninja;
    public GameObject electricalSparksEffect;
    public string imageName
    {
        get => _imageName;
        set => _imageName = value;
    } // nome dell'immagine della carta

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, 180, 0); //ruota tutte le carte di 180 gradi (sul retro)
       // audioSource = GetComponent<AudioSource>();
    }

    //controlli se la carta è girata per continuare a interagire
    private void OnMouseUp()
    {
        if (rotating) return;

        rotating = true;

        selected = !selected;
        //invochi coroutine per ruotare la carta: e attendi !0.8 secondi! (come tutte le coroutines attivi un timer..remember!)
        StartCoroutine(RotateMe(Vector3.zero, 0.8f, selected));
    }

    public void ResetMe()
    {
        selected = false;
        StartCoroutine(RotateMe(Vector3.up * -180, 0.8f, selected)); //deseleziona la carta e mostra il retro (-180)
    }
    
//anche se non lo ricorderai mai questa coroutine è un modo per fluidificare (slerp) i movimenti indipendentemente dal framerate (Time.deltaTime...)!  
IEnumerator RotateMe(Vector3 byAngles, float inTime, bool isSelected)
    {
       //audioSource.Play();

        var fromAngle = transform.rotation; //angolo di partenza
        var toAngle = Quaternion.Euler(byAngles); //angolo di rotazione
        
        //WARNING! CRAZY CODE INCOMING:il for NON è per NON fare un timer credo, ma per rendere tutto "framerate indipendent"
        //Infatti: le "regole" delle coroutine sono le stesse (fico però:rifallo come esercizio!)
        for (var t = 0f; t <= 1; t += Time.deltaTime / inTime)
        {
        transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t); //Aggiorna la rotazione dell'oggetto interpolando tra fromAngle e toAngle in base al tempo t.
        yield return null; //Mette in Pausa l'esecuzione della coroutine fino al frame successivo
                           //così: aggiorna la rotazione gradualmente
        }
        OnClicked(this, isSelected); //Una volta completata la rotazione, invoca l'evento OnClicked,
                                         //notificando al GameManager che la carta è stata selezionata/deselez..
        rotating = false; //la carta ha terminato la rotazione
    }

    public bool Compare(InteractiveCard other)
    {
        return imageName == other.imageName; //questo se poteva fa + fico dice...prova tu!
    }

    internal void HideAndDestroy()
    {
        //animation fade, or script fade or shader fade...
        var material = GetComponent<Renderer>().material;
        StartCoroutine(FadeAndHideCoroutine(material));
    }
    

    //todo claro (se scrivi to-do diventa azzurro perché è inglese ^^ vabeh) ma rivedi dove e COME inverti l'alpha nello shader...vedi video e appunti
    IEnumerator FadeAndHideCoroutine(Material mat)
    {
        while (mat.GetFloat("_Alpha") < 1)
        {
            //OKKIO SOLO CHE: tu all'alpha della texture nello shader gli sottrai l'alpha per farlo bianco.
            //Poi glielo risommi qui nel while, ecco perché sembra che l'alpha è al contrario!
            var newAlpha = Mathf.MoveTowards(mat.GetFloat("_Alpha"), 1, fadeSpeed * Time.deltaTime);
            mat.SetFloat("_Alpha", newAlpha);
            yield return null;
        }

        Destroy(gameObject);
        Instantiate(ninja, gameObject.transform.position, Quaternion.identity);
        Instantiate(electricalSparksEffect, gameObject.transform.position, Quaternion.identity); // Instanzia il particellare: con posiz. carta distrutta
    }
}

 

