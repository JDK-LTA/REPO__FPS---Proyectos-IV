﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    private void Awake()
    {
        Instance = this;

        wSprites = new Sprite[6] { g28Rifle, gunHeavy, pistol28, pistol30, pistol31, scifiGun };
        wMeshes = new Mesh[6] { g28RifleM, gunHeavyM, pistol28M, pistol30M, pistol31M, scifiGunM };
    }

    [SerializeField] private GameObject nrwPanelParent;
    [SerializeField] private GameObject buildPanelParent;
    [SerializeField] private int nOfWeaponsToChooseFrom = 3;
    [SerializeField] private GameObject panelTemplatePrefab;
    [SerializeField] private GameObject nextWaveButton;

    [SerializeField] private Sprite g28Rifle, gunHeavy, pistol28, pistol30, pistol31, scifiGun;
    [SerializeField] private Mesh g28RifleM, gunHeavyM, pistol28M, pistol30M, pistol31M, scifiGunM;
    private Sprite[] wSprites;
    private Mesh[] wMeshes;

    private List<PickablePanel> newWeaponPanels;
    private List<PickablePanel> buildPanels;
    private List<PickablePanel> lastRoundPickablePanels;

    [SerializeField] private PickablePanel panelHovering;
    [SerializeField] private PickablePanel panelClicked;
    [SerializeField] private bool anyPanelClicked = false;
    private bool addedNewWeapon = false;

    public PickablePanel PanelHovering { get => panelHovering; set => panelHovering = value; }
    public PickablePanel PanelClicked { get => panelClicked; set => panelClicked = value; }
    public bool AnyPanelClicked { get => anyPanelClicked; set => anyPanelClicked = value; }
    public bool AddedNewWeapon { get => addedNewWeapon; set => addedNewWeapon = value; }

    private void OnEnable()
    {
        newWeaponPanels = new List<PickablePanel>();
        buildPanels = new List<PickablePanel>();
        lastRoundPickablePanels = new List<PickablePanel>();
        CreateAllPanels();

        foreach (AudioSource item in FindObjectsOfType<AudioSource>())
        {
            if (item.gameObject.name != "Music")
            {
                item.Stop();
            }
        }
    }
    private void OnDisable()
    {
        DestroyAllPanels();
    }
    public void BeginNextWave()
    {
        WaveManager.Instance.BeginNextWave();
    }

    public void CreateAllPanels()
    {
        CreateNrwPanels();
        CreateBuildPanels();
    }
    private void CreateNrwPanels()
    {
        List<int> rng = new List<int>();
        for (int i = 0; i < nOfWeaponsToChooseFrom; i++)
        {
            int ran;
            do
            {
                ran = Random.Range(0, WeaponPrefabsLists.Instance.weaponPrefabLists[WaveManager.Instance.CurrentWave + 1].Count);
            } while (rng.Contains(ran));
            rng.Add(ran);

            GameObject go = Instantiate(panelTemplatePrefab, nrwPanelParent.transform);
            PickablePanel tempPP = go.GetComponent<PickablePanel>();
            tempPP.PositionInBuild = i;
            tempPP.Weapon = WeaponPrefabsLists.Instance.weaponPrefabLists[WaveManager.Instance.CurrentWave + 1][ran];

            MeshToSprite(tempPP);
            tempPP.UpdateTooltip();

            tempPP.CurrentTrueNewFalse = false;

            newWeaponPanels.Add(tempPP);
        }
    }
    private void CreateBuildPanels()
    {
        for (int i = 0; i < WeaponManager.Instance.Weapons.Count; i++)
        {
            CreateBuildPanel(i);
        }
    }

    private void CreateBuildPanel(int i)
    {
        GameObject go = Instantiate(panelTemplatePrefab, buildPanelParent.transform);
        PickablePanel tempPP = go.GetComponent<PickablePanel>();
        tempPP.PositionInBuild = i;
        tempPP.Weapon = WeaponManager.Instance.Weapons[i];

        MeshToSprite(tempPP);
        tempPP.UpdateTooltip();

        tempPP.CurrentTrueNewFalse = true;

        buildPanels.Add(tempPP);
    }

    private void MeshToSprite(PickablePanel tempPP)
    {
        Mesh m = tempPP.Weapon.GetComponent<MeshFilter>().sharedMesh;
        for (int i = 0; i < wMeshes.Length; i++)
        {
            if (m == wMeshes[i])
            {
                tempPP.WeaponImage.sprite = wSprites[i];
                break;
            }
        }
    }

    private void DestroyAllPanels()
    {
        for (int i = newWeaponPanels.Count - 1; i >= 0; i--)
        {
            Destroy(newWeaponPanels[i].gameObject);
        }
        for (int i = buildPanels.Count - 1; i >= 0; i--)
        {
            Destroy(buildPanels[i].gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && panelHovering == null && !addedNewWeapon)
        {
            panelClicked = null;
            anyPanelClicked = false;
            ActivateAllPanels();
        }
    }

    public void DeactivateUnusablePanels(bool all)
    {
        for (int i = 0; i < newWeaponPanels.Count; i++)
        {
            if (all)
            {
                newWeaponPanels[i].PanelSetActive(false);
            }
            else
            {
                newWeaponPanels[i].PanelSetActive(newWeaponPanels[i].Picked);
            }
        }
    }

    private void ActivateAllPanels()
    {
        for (int i = 0; i < newWeaponPanels.Count; i++)
        {
            newWeaponPanels[i].PanelSetActive(true);
        }
    }

    public void AddWeapon()
    {
        addedNewWeapon = true;
        nextWaveButton.SetActive(true);

        GameObject tempNewWeapon = Instantiate(panelClicked.Weapon, WeaponManager.Instance._player.weaponsParent.transform);
        WeaponManager.Instance.Weapons.Add(tempNewWeapon);
        int p = WeaponManager.Instance.Weapons.Count - 1;
        WeaponManager.Instance.Weapons[p].GetComponent<PositionInBuild>().positionInBuild = p;
        StartCoroutine(WeaponManager.Instance.UpdateWeaponsCoroutine());

        newWeaponPanels.Remove(panelClicked);
        Destroy(panelClicked.gameObject);
        panelClicked = null;
        anyPanelClicked = false;
        CreateBuildPanel(p);
        DeactivateUnusablePanels(true);
    }

    public void SwapPickAndBuildPanels()
    {
        addedNewWeapon = true;
        nextWaveButton.SetActive(true);

        int indexPicked = newWeaponPanels.IndexOf(panelClicked);
        int indexBuild = buildPanels.IndexOf(panelHovering);

        newWeaponPanels.Insert(indexPicked, panelHovering);
        buildPanels.Insert(indexBuild, panelClicked);

        newWeaponPanels.Remove(panelClicked);
        buildPanels.Remove(panelHovering);

        int aux = panelHovering.PositionInBuild;

        GameObject tempRemovingW = WeaponManager.Instance.Weapons[aux];
        WeaponManager.Instance.Weapons.Remove(WeaponManager.Instance.Weapons[aux]);
        Destroy(tempRemovingW);

        GameObject tempNewWeapon = Instantiate(panelClicked.Weapon, WeaponManager.Instance._player.weaponsParent.transform);
        WeaponManager.Instance.Weapons.Insert(aux, tempNewWeapon);
        WeaponManager.Instance.Weapons[aux].GetComponent<PositionInBuild>().positionInBuild = aux;

        panelHovering.PositionInBuild = panelClicked.PositionInBuild;
        panelClicked.PositionInBuild = aux;
        panelHovering.CurrentTrueNewFalse = false;
        panelClicked.CurrentTrueNewFalse = true;

        panelHovering.transform.parent = nrwPanelParent.transform;
        panelClicked.transform.parent = buildPanelParent.transform;

        SortChildren(buildPanelParent.transform);
        SortChildren(nrwPanelParent.transform);

        RestartPanelsVariables();
        DeactivateUnusablePanels(true);

        //BETTER UPDATEWEAPONS WHEN CLICKING "DONE" BUTTON, BUT LET'S KEEP THIS FOR DEBUGGING PURPOSES FOR NOW
        StartCoroutine(WeaponManager.Instance.UpdateWeaponsCoroutine());
    }

    private void RestartPanelsVariables()
    {
        panelClicked = null;
        anyPanelClicked = false;
    }

    public void SwapBuildPanels()
    {
        int aux = panelHovering.PositionInBuild;

        WeaponManager.Instance.Weapons[aux].GetComponent<PositionInBuild>().positionInBuild = panelClicked.PositionInBuild;
        WeaponManager.Instance.Weapons[panelClicked.PositionInBuild].GetComponent<PositionInBuild>().positionInBuild = aux;

        panelHovering.PositionInBuild = panelClicked.PositionInBuild;
        panelClicked.PositionInBuild = aux;

        SortChildren(buildPanelParent.transform);
        RestartPanelsVariables();

        if (!addedNewWeapon)
        {
            ActivateAllPanels();
        }

        //BETTER UPDATEWEAPONS WHEN CLICKING "DONE" BUTTON, BUT LET'S KEEP THIS FOR DEBUGGING PURPOSES FOR NOW
        StartCoroutine(WeaponManager.Instance.UpdateWeaponsCoroutine());
    }

    private void SortChildren(Transform parent)
    {
        List<Transform> children = new List<Transform>();

        int aux = parent.childCount - 1;

        for (int i = aux; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            children.Add(child);
            child.parent = null;
        }

        children.Sort((Transform t1, Transform t2) => { return t1.GetComponent<PickablePanel>().PositionInBuild.CompareTo(t2.GetComponent<PickablePanel>().PositionInBuild); });

        foreach (Transform child in children)
        {
            child.parent = parent;
        }
    }
}
