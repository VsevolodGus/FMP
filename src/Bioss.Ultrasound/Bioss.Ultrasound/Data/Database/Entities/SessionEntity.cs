using SQLite;
using System;

namespace Bioss.Ultrasound.Data.Database.Entities
{
    [Table("sessions")]
    public class SessionEntity
    {
        [MaxLength(511)]
        [PrimaryKey]
        [Column("token")]
        public string Token { get; set; }

        [Column("createdDate")]
        public DateTime CreatedDate{ get; set; }
    }
}
