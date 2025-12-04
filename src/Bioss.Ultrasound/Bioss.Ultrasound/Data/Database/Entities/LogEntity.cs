using SQLite;

namespace Bioss.Ultrasound.Data.Database.Entities
{
    [Table("logs")]
    public class LogEntity
    {
        [Column("id")]
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        [Column("level")]
        public byte Level { get; set; }

        [Column("sessionToken")]
        [MaxLength(511)]
        public string SessionToken { get; set; }

        [Column("message")]
        [MaxLength(4095)]
        public string Message { get; set; }
    }
}
