using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Bioss.Ultrasound.Data.Database.Entities
{
    [Table("data")]
    public class DataEntity
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public long Id { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("heartRate")]
        public byte HeartRate { get; set; }

        [Column("toco")]
        public byte Toco { get; set; }


        //  relations foreign keys
        [ForeignKey(typeof(RecordEntity))]
        [Column("recordId")]
        public long RecordId { get; set; }
    }
}
