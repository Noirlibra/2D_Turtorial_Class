using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] private GameObject[] backgrounds; // Mảng để chứa các background

    private void Start()
    {
        // Kích hoạt một background ngẫu nhiên và tắt các background khác
        SetRandomBackground();
    }

    private void SetRandomBackground()
    {
        // Tắt tất cả các background trước
        foreach (GameObject background in backgrounds)
        {
            background.SetActive(false);
        }

        // Chọn ngẫu nhiên một background để bật
        int randomIndex = Random.Range(0, backgrounds.Length);
        backgrounds[randomIndex].SetActive(true);
    }
}
