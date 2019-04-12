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
    private VariantsManager variantsManager;


    private class GenericKeyValue {

        public object key;
        public object value;

        public void setKeyValue<T, U>(T k, U v) {key = k; value = v; }
    }
    Dictionary<Toggle, GenericKeyValue> dict = new Dictionary<Toggle, GenericKeyValue>();

    void Start()
    {
        // Get the last imported model and init GUI with its variants
        if (FindObjectsOfType<ImportStamp>().Length == 0) { this.gameObject.SetActive(false); return; };
        importedModel = Importer.LatestModelImportedObject.gameObject;

        // Checks if imported model exists and has variants
        if (importedModel == null) { this.gameObject.SetActive(false); return; };
        variantsManager = importedModel.GetComponentInChildren<VariantsManager>();
        if (variantsManager == null) { this.gameObject.SetActive(false); return; }

        // Init GUI
        setVariantsContent<VariantsManager.MaterialSwitch, Material>(materialContent, variantsManager.MaterialSwitchList);
        setVariantsContent<VariantsManager.TransformSwitch, TransformVariant>(transformContent, variantsManager.TransformSwitchList);
    }

    void setVariantsContent<T, U>(RectTransform variantsContent, List<T> switchList)
    {
        if (switchList.Count == 0) { variantsContent.transform.parent.parent.parent.gameObject.SetActive(false); return; }
        float offset = 4.0f;
        float bottom = 0.0f;
        foreach (var switchObject in switchList) {

            var name = switchObject.GetFieldValue<string>("name");
            List<U> variants = switchObject.GetFieldValue<List<U>>("variants");

            GameObject subContent = CreateSubContent(tranformSubContent, variantsContent, -offset, name);
            subContent.name = name;
            float size = InsertVariantsInContent(toggle, subContent.GetChildren(false, false)[0], variantsContent, switchObject, variants);

            subContent.GetComponent<RectTransform>().sizeDelta = new Vector2(subContent.GetComponent<RectTransform>().sizeDelta[0], subContent.GetComponent<RectTransform>().rect.height + size);
            subContent.transform.localPosition = new Vector3(subContent.transform.localPosition.x, -subContent.GetComponent<RectTransform>().rect.height / 2.0f - offset, subContent.transform.localPosition.z);
            offset += subContent.GetComponent<RectTransform>().rect.height + 5.0f;

            subContent.GetChildren(false, false)[0].GetComponent<ToggleGroup>().SetAllTogglesOff();
            subContent.GetChildren(false, false)[0].transform.GetChild(0).GetComponent<Toggle>().isOn = true;

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

        GameObject switchTextObject = instance.GetChildren(false, false)[0];
        switchTextObject.GetComponent<Text>().text = t;
        switchTextObject.AddComponent<ToggleGroup>();

        GameObject variantsToggleGroup = instance.GetChildren(false, false)[0];

        return instance;
    }

    // prefabToggle: prefab of the toggles to instanciate
    // content: father of the toggle (subcontent created in CreateSubContent)
    // variantsContent: transform or materials content
    // variants: list of variants names corresponding to the subcontent
    float InsertVariantsInContent<T, U>(GameObject prefabToggle, GameObject content, RectTransform variantsContent, T switchObject, List<U> variants)
    {
        float offset = 30.0f;
        foreach (var variant in variants) {
            GameObject m_Toggle = Instantiate(toggle, content.transform);

            if (variant == null) { continue; }
            string name = variant.GetPropertyValue<string>("name");
            m_Toggle.name = name;
            m_Toggle.GetComponentInChildren<Text>().text = name;
            m_Toggle.GetComponent<RectTransform>().localPosition = new Vector3(5, -offset);
            m_Toggle.GetComponent<Toggle>().group = content.GetComponent<ToggleGroup>();
            m_Toggle.GetComponent<Toggle>().onValueChanged.AddListener(delegate { ToggleValueChanged(m_Toggle.GetComponent<Toggle>(), variantsContent); });

            GenericKeyValue switchVariant = new GenericKeyValue();
            switchVariant.setKeyValue<T, U> (switchObject, variant);

            dict.Add(m_Toggle.GetComponent<Toggle>(), switchVariant);
            
            offset += m_Toggle.GetComponent<RectTransform>().rect.height;
        }
        return offset - 20;
    }
    
    void ToggleValueChanged(Toggle m_Toggle, RectTransform variantsContent)
    {
        if (m_Toggle.isOn) {
            GenericKeyValue pair = dict[m_Toggle];
            var switchObject = pair.key as VariantsManager.Switch;
            var variant = pair.value;
            switchObject.selectVariant(variant);
        }
    }
    
    
    
    
}
