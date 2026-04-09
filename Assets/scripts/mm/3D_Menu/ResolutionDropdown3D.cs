using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResolutionDropdown3D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private GameObject dropdownPanel;
    [SerializeField] private TextMeshPro currentResolutionText;
    [SerializeField] private Transform optionsRoot;
    [SerializeField] private GameObject optionTemplate;

    [Header("Layout")]
    [SerializeField] private float optionSpacingY = 0.38f;

    private readonly List<Resolution> uniqueResolutions = new();
    private readonly List<GameObject> spawnedOptions = new();

    private int currentResolutionIndex = -1;
    private bool isOpen;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        BuildResolutionList();
        BuildOptions();
        RefreshCurrentResolutionText();

        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
    }

    public void ToggleDropdown()
    {
        isOpen = !isOpen;

        if (dropdownPanel != null)
            dropdownPanel.SetActive(isOpen);
    }

    public void CloseDropdown()
    {
        isOpen = false;

        if (dropdownPanel != null)
            dropdownPanel.SetActive(false);
    }

    public void SelectResolution(int index)
    {
        if (index < 0 || index >= uniqueResolutions.Count)
            return;

        Resolution r = uniqueResolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);

        currentResolutionIndex = index;
        RefreshCurrentResolutionText();
        CloseDropdown();
    }

    private void BuildResolutionList()
    {
        uniqueResolutions.Clear();

        Resolution[] all = Screen.resolutions;
        HashSet<string> seen = new HashSet<string>();

        for (int i = 0; i < all.Length; i++)
        {
            string key = all[i].width + "x" + all[i].height;
            if (seen.Contains(key))
                continue;

            seen.Add(key);
            uniqueResolutions.Add(all[i]);
        }

        int currentW = Screen.currentResolution.width;
        int currentH = Screen.currentResolution.height;

        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            if (uniqueResolutions[i].width == currentW &&
                uniqueResolutions[i].height == currentH)
            {
                currentResolutionIndex = i;
                break;
            }
        }

        if (currentResolutionIndex < 0 && uniqueResolutions.Count > 0)
            currentResolutionIndex = uniqueResolutions.Count - 1;
    }

    private void BuildOptions()
    {
        if (optionsRoot == null || optionTemplate == null)
            return;

        for (int i = 0; i < spawnedOptions.Count; i++)
        {
            if (spawnedOptions[i] != null)
                Destroy(spawnedOptions[i]);
        }

        spawnedOptions.Clear();

        optionTemplate.SetActive(false);

        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            GameObject option = Instantiate(optionTemplate, optionsRoot);
            option.name = "Resolution_Option_" + i;
            option.SetActive(true);

            option.transform.localPosition = new Vector3(0f, -i * optionSpacingY, 0f);
            option.transform.localRotation = Quaternion.identity;
            option.transform.localScale = Vector3.one;

            ResolutionOption3DNoCollider optionScript = option.GetComponent<ResolutionOption3DNoCollider>();
            if (optionScript != null)
                optionScript.Setup(this, targetCamera, i);

            TextMeshPro tmp = option.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
                tmp.text = uniqueResolutions[i].width + "x" + uniqueResolutions[i].height;

            spawnedOptions.Add(option);
        }
    }

    private void RefreshCurrentResolutionText()
    {
        if (currentResolutionText == null)
            return;

        if (currentResolutionIndex < 0 || currentResolutionIndex >= uniqueResolutions.Count)
        {
            currentResolutionText.text = "N/A";
            return;
        }

        Resolution r = uniqueResolutions[currentResolutionIndex];
        currentResolutionText.text = r.width + "x" + r.height;
    }
}