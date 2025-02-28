using TMPro;
using UnityEngine;

public class AddressableTest : MonoBehaviour
{
    public TMP_Text testText;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Managers.Resource.LoadAllAsync<GameObject>("default", (name,loadCount,totalCount) =>
            {
                Debug.Log(name + ", " + loadCount + ", " + totalCount);
                testText.text = name + ", " + loadCount + ", " + totalCount;
            });
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Managers.Resource.Instantiate("Test");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
