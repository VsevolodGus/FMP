using System;
using System.Linq;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Bioss.Ultrasound.Data.Database.Entities
{
    [Table("audio")]
    public class AudioEntity
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public long Id { get; set; }

        [Column("raw")]
        public byte[] Raw { get; set; }

        //  relations foreign keys
        [ForeignKey(typeof(RecordEntity))]
        [Column("recordId")]
        public long RecordId { get; set; }


        public static byte[] ToBytes(short[] raw)
        {
            return raw.SelectMany(BitConverter.GetBytes).ToArray();
        }

        public static short[] ToShorts(byte[] bytes)
        {
            return Array.ConvertAll(bytes, b => (short)b);
        }
    }
}
