using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEnergyCounter : MonoBehaviour, Observer
{
    Dictionary<CharacterMovement, int> actorStopped = new Dictionary<CharacterMovement, int>();
    // Start is called before the first frame update
    void Start()
    {
        var chars = FindObjectsOfType<CharacterMovement>();

        foreach (var chara in chars)
        {
            chara.sub.AddObserver(this);
        }
    }

    //Called when Observer is destroyed
    void OnDestroy()
    {
        var chars = FindObjectsOfType<CharacterMovement>();

        foreach (var chara in chars)
        {
            chara.sub.removeObserver(this);
        }
    }

    public void SubjectUpdate(object sender)
    {
        CharacterMovement stopper = sender as CharacterMovement;
        if (stopper == null)
            return;
        //Hold on, this might not be completely correct, 
    }


 
}
