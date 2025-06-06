using UnityEngine;
using Fungus;

[ExecuteAlways]
public class FungusSaveHelper : MonoBehaviour
{
    public string blockName;
    public int commandIndex;

    private void OnEnable()
    {
        // Автоматически обновим имя блока и индекс, если это возможно (в редакторе)
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
