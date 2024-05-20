using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextBox : MonoBehaviour
{
    string speakerName;
    string body;
    
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //Sets the name and the body
    public void SetText(string name, string body)
    {
        this.speakerName = name;
        this.body = body;
    }

    //Sets just the body and hides the name
    public void SetText(string body)
    {

    }

    
}
