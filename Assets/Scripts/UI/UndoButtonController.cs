using UnityEngine;
using UnityEngine.UI;

public class UndoButtonController : MonoBehaviour
{
    public CubeManager cubeManager;
    public Button undoButton;

    void Update()
    {
        undoButton.gameObject.SetActive(cubeManager.HasUndo());
    }

    public void OnUndoClicked()
    {
        cubeManager.UndoLastMove();
    }
}
