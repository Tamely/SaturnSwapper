#include "Structs.rd"

from_ar = import "My/File/Path";

Color color = new Color(1.0, 0.0, 0.0, 1.0); // Blue
from_ar.Seek(100, SeekOrigin.Begin);
Color existingColor = from_ar.Read<Color>();
if (existingColor.A == 1.0) // Not practical, but just to showcase conditions
{
    from_ar.Write<Color>(color);
}

Color[] colors = new Color[10];
for (int i = 0; i < colors.Length(); i++) // Not practical, but just to showcase conditions
{
    colors[i] = from_ar.Read<Color>();
}

for (int i = 0; i < colors.Length(); i++) // Not practical, but just to showcase conditions
{
    from_ar.Write<Color>(colors[i]);
}
