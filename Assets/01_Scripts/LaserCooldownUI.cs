using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserCooldownUI : MonoBehaviour
{
    [SerializeField] private LaserRay laser;
    [SerializeField] private Image circle;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private bool hideWhenLocked = true;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        if (group) { group.interactable = false; group.blocksRaycasts = false; }
    }

    void LateUpdate()
    {
        if (!laser || !circle) return;

        if (group)
            group.alpha = (hideWhenLocked && !laser.IsUnlocked) ? 0f : 1f;

        circle.fillAmount = laser.Cooldown01();
    }
}
