using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fungus;
using static InGameMenuManager.CustomMenuDialog;

[System.Serializable]
public class CustomMenuDialog : MonoBehaviour
{
    [SerializeField] private Transform optionsParent; // Контейнер для кнопок
    [SerializeField] private Button optionButtonPrefab; // Префаб кнопки опции
    public InGameMenuManager menuManager;
    private List<MenuOption> currentOptions = new List<MenuOption>();
    public Flowchart flowchart;
    public class MenuOption
    {
        public string Text;
        public Block TargetBlock;
        public Button UIButton;
    }



    // Метод для получения текущих опций меню
    public List<MenuOption> GetOptions()
    {
        return currentOptions;
    }
    public void AddOption(string text, Block targetBlock, Flowchart flowchart)
    {
        Button button = Instantiate(optionButtonPrefab, optionsParent);
        var textComponent = button.GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }

        button.onClick.AddListener(() =>
        {
            flowchart.ExecuteBlock(targetBlock);
        });

        currentOptions.Add(new MenuOption
        {
            Text = text,
            TargetBlock = targetBlock,
            UIButton = button
        });
    }


    public void ClearOptions()
    {
        foreach (var option in currentOptions)
        {
            if (option.UIButton != null)
            {
                Destroy(option.UIButton.gameObject);
            }
        }
        currentOptions.Clear();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }
    private void ExecuteBlock(Block block)
    {
        if (block != null && flowchart != null)
        {
            flowchart.ExecuteBlock(block);
        }
    }

}
