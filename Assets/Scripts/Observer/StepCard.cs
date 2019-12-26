using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepCard : MonoBehaviour {

    public Text cardName;
    public Text steps;

	public void SetName(string text)
    {
        cardName.text = text;
    }

    public void SetSteps(int i)
    {
        steps.text = i.ToString();
    }


}
