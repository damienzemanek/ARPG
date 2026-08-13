using UnityEngine;

public class EnemyActionIntentions : MonoBehaviour
{
    public GameObject action1;
    public GameObject action2;
    public GameObject action3;
    public GameObject bufferBtwx1and2;
    public GameObject bufferBtwx2and3;

    public void EnableActions(int amount)
    {
        if (amount == 1)
        {
            action1.SetActive(true);
            action2.SetActive(false);
            action3.SetActive(false);
            bufferBtwx1and2.SetActive(false);
            bufferBtwx2and3.SetActive(false);
        }
        else if (amount == 2)
        {
            action1.SetActive(true);
            action2.SetActive(true);
            action3.SetActive(false);
            bufferBtwx1and2.SetActive(true);
            bufferBtwx2and3.SetActive(false);
        }
        else if (amount == 3)
        {
            action1.SetActive(true);
            action2.SetActive(true);
            action3.SetActive(true);
            bufferBtwx1and2.SetActive(true);
            bufferBtwx2and3.SetActive(true);
        }
        else
            Debug.LogError("Invalid amount of actions : " + amount + " Must be 1, 2 or 3.");
    }
}
