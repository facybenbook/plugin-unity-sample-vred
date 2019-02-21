using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PiXYZ.Plugin.Unity;
using PiXYZ.Plugin.Unity.VRED;

public class VariantsDisplay : MonoBehaviour {

    public RectTransform transformContent;
    public RectTransform materialContent;
    public GameObject tranformSubContent;
    public GameObject toggle;


    private GameObject importedModel;
    private PiXYZ.Plugin.Unity.VRED.VariantsManager variantsManager;

    void Start()
    {
        if (FindObjectsOfType<ImportedModel>().Length == 0) return;
        importedModel = Importer.LatestModelImportedObject.gameObject;
        if (importedModel == null) return;
        variantsManager = importedModel.GetComponentInChildren<VariantsManager>();
        if (variantsManager == null) { this.gameObject.SetActive(false); return; }
        SetVariantsContent(materialContent);
        SetVariantsContent(transformContent);
    }

    // Set the variants contents (materials and transform)
    // -> list the switches, their variants and creates subcontents with toggles
    // -> adjust the size of the different contents
    // -> create toggle groups and subscribe the toggles to the ToggleValueChanged
    void SetVariantsContent(RectTransform variantsContent)
    {
        List<string> switches = new List<string>();
        if (variantsContent.Equals(transformContent)) {
            switches = variantsManager.GetTransformSwitches();
        } else if (variantsContent.Equals(materialContent)) {
            switches = variantsManager.GetMaterialSwitches();
        }
        float offset = 4.0f;
        float bottom = 0.0f;
        foreach (string switchName in switches) {
            List<string> variants = new List<string>();
            if (variantsContent.Equals(transformContent)) {
                variants = variantsManager.GetTransformVariants(switchName);
            } else if (variantsContent.Equals(materialContent)) {
                variants = variantsManager.GetMaterialVariants(switchName);
            }
            if (variants.Count == 0) continue;
            GameObject subContent = CreateSubContent(tranformSubContent, variantsContent, -offset, switchName);
            subContent.name = switchName;
            float size = InsertVariantsInContent(toggle, subContent.GetChildren()[0], variantsContent, variants);

            subContent.GetComponent<RectTransform>().sizeDelta = new Vector2(subContent.GetComponent<RectTransform>().sizeDelta[0], subContent.GetComponent<RectTransform>().rect.height + size);
            subContent.transform.localPosition = new Vector3(subContent.transform.localPosition.x, -subContent.GetComponent<RectTransform>().rect.height / 2.0f - offset, subContent.transform.localPosition.z);
            offset += subContent.GetComponent<RectTransform>().rect.height + 5.0f;

            subContent.GetChildren()[0].GetComponent<ToggleGroup>().SetAllTogglesOff();
            subContent.GetChildren()[0].transform.GetChild(0).GetComponent<Toggle>().isOn = true;

            bottom = -subContent.transform.localPosition.y + subContent.GetComponent<RectTransform>().sizeDelta[1] / 2;
        }
        variantsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(variantsContent.GetComponent<RectTransform>().sizeDelta[0], bottom);
    }

    // content: Content to instanciate
    // variantsContent: variantsContent window (father of content to instanciate)
    // position: where to put the content
    // t: title of the content
    GameObject CreateSubContent(GameObject content, Transform variantsContent, float yPosition, string t)
    {
        GameObject instance = Instantiate(content, variantsContent);
        instance.GetComponent<RectTransform>().localPosition = new Vector2(instance.transform.localPosition.x, yPosition);

        GameObject switchTextObject = instance.GetChildren()[0];
        switchTextObject.GetComponent<Text>().text = t;
        switchTextObject.AddComponent<ToggleGroup>();

        GameObject variantsToggleGroup = instance.GetChildren()[0];

        return instance;
    }

    // prefabToggle: prefab of the toggles to instanciate
    // content: father of the toggle (subcontent created in CreateSubContent)
    // variantsContent: transform or materials content
    // variants: list of variants names corresponding to the subcontent
    float InsertVariantsInContent(GameObject prefabToggle, GameObject content, RectTransform variantsContent, List<string> variants)
    {
        float offset = 30.0f;
        foreach (string variant in variants) {
            GameObject m_Toggle = Instantiate(toggle, content.transform);
            m_Toggle.name = variant;
            m_Toggle.GetComponentInChildren<Text>().text = variant;
            m_Toggle.GetComponent<RectTransform>().localPosition = new Vector3(5, -offset);
            m_Toggle.GetComponent<Toggle>().group = content.GetComponent<ToggleGroup>();
            m_Toggle.GetComponent<Toggle>().onValueChanged.AddListener(delegate { ToggleValueChanged(m_Toggle.GetComponent<Toggle>(), variantsContent); });

            offset += m_Toggle.GetComponent<RectTransform>().rect.height;
        }
        return offset - 20;
    }

    void ToggleValueChanged(Toggle m_Toggle, RectTransform variantsContent)
    {
        if (m_Toggle.isOn) {
            if (variantsContent.Equals(transformContent)) {
                variantsManager.SelectTransformVariant(m_Toggle.GetComponentInParent<Text>().text, m_Toggle.GetComponentInChildren<Text>().text);
            } else if (variantsContent.Equals(materialContent)) {
                variantsManager.SelectMaterialVariant(m_Toggle.GetComponentInParent<Text>().text, m_Toggle.GetComponentInChildren<Text>().text);
            }
        }
    }

}
