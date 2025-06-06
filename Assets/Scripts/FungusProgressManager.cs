using UnityEngine;

public class FungusProgressManager : MonoBehaviour
{
    public string currentBlockName = "";
    public int currentCommandIndex = 0;

    // Метод вызывается из Fungus Execute Method Command
    public void SaveProgress(string blockName, int commandIndex)
    {
        currentBlockName = blockName;
        currentCommandIndex = commandIndex;

        Debug.Log($"Progress saved: Block = {blockName}, Command = {commandIndex}");
    }
    public void SaveProgressBlockName(string blockName)
    {
        currentBlockName = blockName;
        Debug.Log($"BlockName сохранён: {blockName}");
    }

    public void SaveProgressCommandIndex(int commandIndex)
    {
        currentCommandIndex = commandIndex;
        Debug.Log($"CommandIndex сохранён: {commandIndex}");
    }

}
