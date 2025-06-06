using UnityEngine;
using CGTUnity.Fungus.SaveSystem;

public class SaveMenuController : MonoBehaviour
{
    public int currentSaveSlot = 0; // номер слота дл€ сохранени€
    public Fungus.Flowchart flowchart; // ссылка на Flowchart Fungus
    public FungusSaveHelper saveHelper; // ссылка на компонент, фиксирующий позицию Fungus

    public void OnSaveButtonClicked()
    {
        var newSave = SaveManager.S.gameSaver.CreateSave(currentSaveSlot);

        // «аписываем позицию Fungus в формате строки
        newSave.SetData("FungusBlockName", saveHelper.blockName);
        newSave.SetData("FungusCommandIndex", saveHelper.commandIndex.ToString());

        SaveManager.S.AddSave(newSave, true);
    }

    public void OnLoadButtonClicked()
    {
        var save = SaveManager.S.GetSave(currentSaveSlot);
        if (save == null)
        {
            Debug.LogWarning("Ќет сохранени€ в этом слоте!");
            return;
        }

        string blockName = save.GetDataByType("FungusBlockName");
        string commandIndexStr = save.GetDataByType("FungusCommandIndex");

        int commandIndex = 0;
        if (!string.IsNullOrEmpty(commandIndexStr))
        {
            int.TryParse(commandIndexStr, out commandIndex);
        }

        var block = flowchart.FindBlock(blockName);
        if (block != null)
        {
            flowchart.ExecuteBlock(block, commandIndex);
        }
        else
        {
            Debug.LogWarning("Ѕлок Fungus не найден: " + blockName);
        }
    }
}
