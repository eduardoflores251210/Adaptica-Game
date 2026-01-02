float3 WavelengthColor(float3 inputColor, float wavelengthFactor)
{
    // ajustar cada canal en función del factor
    // aquí hacemos un mapeo simple: rojo aumenta al atardecer, azul disminuye
    float3 result;

    // rojo aumenta con factor, verde se mantiene más o menos, azul disminuye
    result.r = saturate(inputColor.r + wavelengthFactor * 0.5);
    result.g = saturate(inputColor.g + wavelengthFactor * 0.2);
    result.b = saturate(inputColor.b - wavelengthFactor * 0.5);

    return result;
}
float3 WavelengthColor_float(float3 inputColor, float wavelengthFactor)
{
    // ajustar cada canal en función del factor
    // aquí hacemos un mapeo simple: rojo aumenta al atardecer, azul disminuye
    float3 result;

    // rojo aumenta con factor, verde se mantiene más o menos, azul disminuye
    result.r = saturate(inputColor.r + wavelengthFactor * 0.5);
    result.g = saturate(inputColor.g + wavelengthFactor * 0.2);
    result.b = saturate(inputColor.b - wavelengthFactor * 0.5);

    return result;
}
