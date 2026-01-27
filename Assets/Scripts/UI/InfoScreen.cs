using UnityEngine;
using UnityEngine.UIElements;

public class InfoScreen : MonoBehaviour
{
    public UIDocument uiDocument;
    public string Name;
    private Label infoLabel;

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        infoLabel = root.Q<Label>(Name);

        string version = Application.version;
        string companyName = Application.companyName;
        string productName = Application.productName;

        infoLabel.text = $"{productName} Versión {version} hecho por {companyName}";
    }
}
