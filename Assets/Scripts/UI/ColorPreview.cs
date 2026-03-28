using UnityEngine;
using UnityEngine.UIElements;

public class ColorPreview : MonoBehaviour
{
    public UIDocument uiDocument;
    public string ElementName, HSlider, SSlider, VSlider;
    private void Update()
    {
        var root = uiDocument.rootVisualElement;
        if (root != null )
        {
            var hh  = root.Q<SliderInt>(HSlider);
            var ss  = root.Q<SliderInt>(SSlider);
            var vv  = root.Q<SliderInt>(VSlider);
            byte Hval = (byte)hh.value;
            byte Sval = (byte)ss.value;
            byte Vval = (byte)vv.value;
            ChangeBackgroundColor(ElementName, Hval, Sval, Vval);
        }
    }
    public void ChangeBackgroundColor(string elementName, byte h, byte s, byte v)
    {
        if (uiDocument == null)
        {
            Debug.LogError("No hay UIDocument asignado.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        VisualElement targetElement = root.Q<VisualElement>(elementName);

        if (targetElement == null)
        {
            Debug.LogWarning($"Elemento con nombre '{elementName}' no encontrado en el UI Document.");
            return;
        }

        Color newColor = Color.HSVToRGB(h / 255f, s / 255f, v / 255f);

        targetElement.style.backgroundColor = new StyleColor(newColor);

        //Debug.Log($"Color de fondo del elemento '{elementName}' cambiado a RGB({r}, {g}, {b}).");
    }
    public Color GetColor()
    {
        var root = uiDocument.rootVisualElement;
        if (root != null)
        {
            var hh = root.Q<SliderInt>(HSlider);
            var ss = root.Q<SliderInt>(SSlider);
            var vv = root.Q<SliderInt>(VSlider);
            byte Hval = (byte)hh.value;
            byte Sval = (byte)ss.value;
            byte Bval = (byte)vv.value;
            return InternalGetColor(Hval, Sval, Bval);
        }
        else return Color.black;
    }
    public Color GetColor(float Alpha)
    {
        var BaseColor = GetColor();
        BaseColor.a = Alpha;
        return BaseColor;
    }
    public void SetColor(Color color)
    {
        var root = uiDocument.rootVisualElement;
        float rf, gf, bf;
        rf = color.r; gf = color.g;
        bf = color.b;
        byte R,G, B;
        R = (byte)(rf * 255);
        G = (byte)(gf * 255);
        B = (byte)(bf * 255);
        var hh = root.Q<SliderInt>(HSlider);
        var ss = root.Q<SliderInt>(SSlider);
        var vv = root.Q<SliderInt>(VSlider);
        hh.value = R;
        ss.value = G;
        vv.value = B;
    }
    private Color InternalGetColor(byte h, byte s, byte v)
    {
        return Color.HSVToRGB(h / 255f, s / 255f, v / 255f);
    }
}
