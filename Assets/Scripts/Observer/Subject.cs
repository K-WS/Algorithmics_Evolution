using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subject {

    object sender; 
    List<Observer> observers = new List<Observer>();

    public Subject(object sender) {
        this.sender = sender;
    }

    // * AddObserver - Used by observers to register to this subject
    public void AddObserver(Observer obs)
    {
        observers.Add(obs);
    }

    // * RemoveObserver - Used by observers to deregister from this subject
    public void removeObserver(Observer obs)
    {
        observers.Remove(obs);
    }

    // * Notify - Used by owner of this subject to notify observers, that something has happened.   
    public void Notify()
    {
        foreach (Observer o in observers)
        {
            o.SubjectUpdate(sender);
        }
            
    }
}