using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectController : MonoBehaviour
{
    public void SelectKnight()
    {
        // Lưu thông tin chọn Knight
        PlayerPrefs.SetString("SelectedCharacter", "Knight");
        // Chuyển tới scene game
        SceneManager.LoadScene("GameScene");
    }

    public void SelectDragon()
    {
        // Lưu thông tin chọn Dragon
        PlayerPrefs.SetString("SelectedCharacter", "Dragon");
        // Chuyển tới scene game
        SceneManager.LoadScene("GameScene");
    }
}
