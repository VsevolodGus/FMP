using PdfSharpCore.Fonts;
using System.IO;
using System.Reflection;

namespace Bioss.Ultrasound.Tools
{
    public class MyFontResolver : IFontResolver
	{
        private const string RESOURCES_PATH = "Bioss.Ultrasound.Resources";

        private static bool _fontResolverAlreadySet;

		public string DefaultFontName => "Arial";

		public byte[] GetFont(string faceName)
		{
			using (var ms = new MemoryStream())
			{
				var assembly = typeof(MyFontResolver).GetTypeInfo().Assembly;
                var resourceName = $"{RESOURCES_PATH}.Fonts.{faceName}.ttf";
				using (var rs = assembly.GetManifestResourceStream(resourceName))
				{
					rs.CopyTo(ms);
					ms.Position = 0;
					return ms.ToArray();
				}
			}
		}

		public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
		{
            // Игнорировать регистр имен шрифтов
            var name = familyName.ToLower().TrimEnd('#');
            // Выбираем шрифты которые у нас есть
            switch (name)
            {
                case "arial":
                    if (isBold)
                    {
                        if (isItalic)
                            return new FontResolverInfo("Arial-BoldItalic");
                        return new FontResolverInfo("Arial-Bold");
                    }
                    if (isItalic)
                        return new FontResolverInfo("Arial-Italic");
                    return new FontResolverInfo("Arial");

                case "segoe ui":
                    if (isBold)
                    {
                        if (isItalic)
                            return new FontResolverInfo("SegoeUI-BoldItalic");
                        return new FontResolverInfo("SegoeUI-Bold");
                    }
                    if (isItalic)
                        return new FontResolverInfo("SegoeUI-Italic");
                    return new FontResolverInfo("SegoeUI");
            }

            // Все остальные запросы шрифтов передаем обработчику по умолчанию.
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }

        internal static void Apply()
        {
            if (!_fontResolverAlreadySet)
            {
                GlobalFontSettings.FontResolver = new MyFontResolver();
                _fontResolverAlreadySet = true;
            }
        }
    }
}
