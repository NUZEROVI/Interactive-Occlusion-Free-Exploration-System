using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TypewriterText : MonoBehaviour
{

    public Text text;
   
    public float letterPause = 0.1f;
   
    public int sentencePause = 1;
  
    private string sentence = "Press any key to enter ...";
  
    void Start()
    {
        StartCoroutine(Delay(sentence));
    }


    IEnumerator Delay(string str)
    {
        yield return new WaitForSeconds(sentencePause);
        StartCoroutine(TypeText(str));
    }

    IEnumerator TypeText(string str)
    {
        foreach (var word in str)
        {
            text.text += word;
            yield return new WaitForSeconds(letterPause);
        }
    }
}
