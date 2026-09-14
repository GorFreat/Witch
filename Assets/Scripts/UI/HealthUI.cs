using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image _heartImage;
    [SerializeField] private Sprite _fullHeartSprite;
    [SerializeField] private Sprite _emptyHeartSprite;

    private List<Image> Hearts = new List<Image>();
    
    public void SetMaxHeart(int maxHeart)
    {
        foreach(Image heart in Hearts)
        {
            Destroy(heart.gameObject);
        }
        Hearts.Clear();

        for(int i = 0; i < maxHeart; i++)
        {
            Image newHeart = Instantiate(_heartImage, transform);
            newHeart.sprite = _fullHeartSprite;
            Hearts.Add(newHeart);
        }
    }

    public void UpdateHeart(int currentHealth)
    {
        for (int i = 0; i < Hearts.Count; i++)
        {
            if( i < currentHealth)
            {
                Hearts[i].sprite = _fullHeartSprite;
            }
            else
            {
                Hearts[i].sprite = _emptyHeartSprite;
            }
        }
    }

}
