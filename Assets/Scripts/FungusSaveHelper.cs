using UnityEngine;
using Fungus;

[ExecuteAlways]
public class FungusSaveHelper : MonoBehaviour
{
    public string blockName;
    public int commandIndex;

    private void OnEnable()
    {
        // ������������� ������� ��� ����� � ������, ���� ��� �������� (� ���������)
#if UNITY_EDITOR
        if (TryGetComponent<Command>(out var cmd))
        {
            var parent = cmd.ParentBlock;
            if (parent != null)
            {
                blockName = parent.BlockName;
                commandIndex = parent.CommandList.IndexOf(cmd);
            }
        }
#endif
    }
}
