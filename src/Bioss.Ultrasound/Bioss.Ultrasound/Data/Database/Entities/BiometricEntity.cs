using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Bioss.Ultrasound.Data.Database.Entities
{
    [Table("biometric")]
    public class BiometricEntity
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public long Id { get; set; }

        [Column("comment")]
        public string Comment { get; set; }

        //  celsius
        [Column("temperature")]
        public double Temperature { get; set; }

        //  ммоль/л
        [Column("sugar")]
        public double Sugar { get; set; }

        //  давление
        [Column("systolicPressure")]
        public int Systolic { get; set; }

        [Column("diastolicPressure")]
        public int Diastolic { get; set; }

        //  уд/мин
        [Column("pulse")]
        public int Pulse { get; set; }

        //  relations foreign keys
        [ForeignKey(typeof(RecordEntity))]
        [Column("recordId")]
        public long RecordId { get; set; }
    }
}
