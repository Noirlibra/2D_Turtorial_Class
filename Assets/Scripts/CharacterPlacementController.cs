using UnityEngine;

public class CharacterPlacementController : MonoBehaviour
{
    public GameObject knightPrefab;  // Prefab của Knight
    public GameObject dragonPrefab;  // Prefab của Dragon
    public Transform playerStartPosition; // Vị trí khởi đầu của người chơi
    public Transform aiStartPosition; // Vị trí khởi đầu của AI

    private void Start()
    {
        string selectedCharacter = PlayerPrefs.GetString("SelectedCharacter");
        
        if (selectedCharacter == "Knight")
        {
            Instantiate(knightPrefab, playerStartPosition.position, Quaternion.identity);
            Instantiate(dragonPrefab, aiStartPosition.position, Quaternion.identity);
        }
        else if (selectedCharacter == "Dragon")
        {
            Instantiate(dragonPrefab, playerStartPosition.position, Quaternion.identity);
            Instantiate(knightPrefab, aiStartPosition.position, Quaternion.identity);
        }
    }
}
