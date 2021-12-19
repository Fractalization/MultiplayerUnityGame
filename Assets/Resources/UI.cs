using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private int frameCount = 0;
    private float time = 0f;
    private float pollTime = 1f;


    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        frameCount++;

        if(time >= pollTime){
            int fps = (int) (frameCount / time);
            fpsText.text = "FPS:" + fps.ToString();

            time = 0;//subtracting pollTime doesn't make sense to me
            frameCount = 0;
        }
    }
}
