using Bioss.Ultrasound.Core.Data.Database.Entities.Enums;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Bioss.Ultrasound.Core.Data.Database.Entities;

[Table("events")]
public class EventEntity
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public long Id { get; set; }

    [Column("time")]
    public DateTime Time { get; set; }

    [Column("event")]
    public Events Event { get; set; }

    //  relations foreign keys
    [ForeignKey(typeof(RecordEntity))]
    [Column("recordId")]
    public long RecordId { get; set; }
}
