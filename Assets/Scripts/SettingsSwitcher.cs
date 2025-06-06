using UnityEngine;

public class SettingsSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject inGameMenuPanel;  // ��� ������� ����
    [SerializeField] private GameObject settingsPanel;    // ������ �������� (��������, �� �������� ����)

    private EnhancedSettingsMenu settingsMenu;

    private void Awake()
    {
        if (settingsPanel != null)
        {
            settingsMenu = settingsPanel.GetComponent<EnhancedSettingsMenu>();
            settingsPanel.SetActive(false);
        }
    }

    public void OpenSettings()
    {
        if (settingsMenu != null)
        {
            inGameMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
            settingsMenu.OpenSettings();
        }
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        inGameMenuPanel.SetActive(true);
    }
   

}
