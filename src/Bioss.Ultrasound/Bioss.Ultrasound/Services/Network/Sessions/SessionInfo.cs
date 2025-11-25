using System;

namespace Bioss.Ultrasound.Services.Network.Sessions
{
    public class SessionInfo
    {
        public string Token { get; set; }
        // Где должно быть обновление этого поля?
        public DateTimeOffset LastActivityDate { get; set; }
    }
}
