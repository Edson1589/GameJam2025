using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BodyPart { Head, Legs, Arms, Torso }

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida aportada por cada parte")]
    [SerializeField] private int headHP = 40;
    [SerializeField] private int legsHP = 25;
    [SerializeField] private int armsHP = 20;
    [SerializeField] private int torsoHP = 35;

    [Header("UI")]
    [SerializeField] private Image healthFillImage;

    [Header("Refs")]
    [SerializeField] private PlayerRespawnHandler respawnHandler;
    [SerializeField] private PlayerController playerController;

    // Runtime
    private int finalMax;
    private int ownedMax;
    private int currentHP;

    void Awake()
    {
        finalMax = headHP + legsHP + armsHP + torsoHP;

        ownedMax = headHP;
        currentHP = ownedMax;

        UpdateUI();
    }

    public void InitializeFromParts(bool hasLegs, bool hasArms, bool hasTorso)
    {
        ownedMax = headHP;
        if (hasLegs) ownedMax += legsHP;
        if (hasArms) ownedMax += armsHP;
        if (hasTorso) ownedMax += torsoHP;

        currentHP = ownedMax;
        UpdateUI();
    }

    public void OnPartConnected(BodyPart part)
    {
        int add = GetPartHP(part);
        ownedMax += add;
        currentHP = Mathf.Min(currentHP + add, ownedMax);
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHP -= amount;
        if (currentHP <= 0)
        {
            DieAndRespawn();
        }
        else
        {
            UpdateUI();
        }
    }

    private void DieAndRespawn()
    {
        if (respawnHandler != null)
            respawnHandler.RespawnNow();

        currentHP = Mathf.Max(ownedMax, 1);
        UpdateUI();
    }

    private int GetPartHP(BodyPart part)
    {
        switch (part)
        {
            case BodyPart.Head: return headHP;
            case BodyPart.Legs: return legsHP;
            case BodyPart.Arms: return armsHP;
            case BodyPart.Torso: return torsoHP;
        }
        return 0;
    }

    private void UpdateUI()
    {
        if (healthFillImage != null)
        {
            float fill = finalMax > 0 ? (float)currentHP / finalMax : 0f;
            healthFillImage.fillAmount = Mathf.Clamp01(fill);
        }
    }

    public int GetCurrentHP() => currentHP;
    public int GetOwnedMax() => ownedMax;
    public int GetFinalMax() => finalMax;
}