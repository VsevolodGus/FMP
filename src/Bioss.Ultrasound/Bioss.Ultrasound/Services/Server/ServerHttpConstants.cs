using System;

namespace Bioss.Ultrasound.Services.Server
{
    public struct ServerHttpConstants
    {
        public static readonly Uri Uri = new Uri("https://dev.dxnich.ru/");
        public const string JsonMediaType = "application/json";
        public const string UserAgent = "Bipuls.v1";
    }
}
