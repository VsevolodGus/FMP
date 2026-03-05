using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Bioss.Ultrasound.Core.Data.Database.Entities;

[Table("record")]
public class RecordEntity
{
    [PrimaryKey, AutoIncrement]
    [Column("id")]
    public long Id { get; set; }

    [Column("startTime")]
    public DateTime StartTime { get; set; }

    [Column("endTime")]
    public DateTime EndTime { get; set; }

    [Column("deviceSerial")]
    public string DeviceSerialNumber { get; set; }

    //  relations
    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public DataEntity[] Datas { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public EventEntity[] Events { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.All)]
    public AudioEntity Audio { get; set; }

    [OneToOne(CascadeOperations = CascadeOperation.All)]
    public BiometricEntity Biometric { get; set; }
}
