using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuOption : MonoBehaviour, IInteractableUI
{
    [SerializeField] TMP_Text TMtext;
    [SerializeField] Material material;
    [SerializeField]
    string text;
    int textLength;
    TMP_CharacterInfo charInfo;
    TMP_TextInfo textInfo;
    bool highlighted = false;

    const float AMPLITUDE = 1.2f;

    public void SetText(string text)
    {
        this.text = text;
        textLength = text.Length;
        TMtext.text = this.text;
        textInfo = TMtext.textInfo;
        highlighted = false;
    }

    #region UI Interface Methods
    public IEnumerator CloseAnimation()
    {
        yield return null;
    }

    public IEnumerator EnterAnimation()
    {
        yield return null;
    }

    public IEnumerator HighlightedAnimation()
    {
        highlighted = true;

        float[] noiseValues = new float[textLength];
        Mesh mesh;
        Vector3[] verts;
        int startVertIndex;

        while (highlighted)
        {
            mesh = TMtext.mesh;
            verts = mesh.vertices;

            //Check for text effects and apply up to the current character
            for (int i = 0; i < textLength; i++)
            {
                charInfo = textInfo.characterInfo[i];
                startVertIndex = charInfo.vertexIndex;

                //****** Check what effects apply to each character ******

                //Wave effect
                /*
                verts[startVertIndex] += new Vector3(tempVector.x * tempFloat, tempVector.y * tempFloat, 0);
                verts[startVertIndex + 1] += new Vector3(tempVector.x * tempFloat, tempVector.y * tempFloat, 0);
                verts[startVertIndex + 2] += new Vector3(tempVector.x * tempFloat, tempVector.y * tempFloat, 0);
                verts[startVertIndex + 3] += new Vector3(tempVector.x * tempFloat, tempVector.y * tempFloat, 0);
                */
            }
            yield return null;
        }

        yield return null;
    }

    public IEnumerator UnhighlightedAnimation()
    {
        highlighted = false;

        while(!highlighted)
        {
            yield return null;
        }

        yield return null;
    }

    public IEnumerator IdleAnimation()
    {
        yield return null;
    }

    public IEnumerator SelectAnimation()
    {
        yield return null;
    }


    #endregion
}
