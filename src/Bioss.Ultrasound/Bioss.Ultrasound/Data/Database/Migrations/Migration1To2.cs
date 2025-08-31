using Bioss.Ultrasound.Data.Database.Entities;
using SQLite;
using SQLiteMigrations;

namespace Bioss.Ultrasound.Data.Database.Migrations
{
    public class Migration1To2 : IMigration
    {
        public int OldVersion => 1;

        public int NewVersion => 2;

        public void Migrate(SQLiteConnection connection)
        {
            connection.CreateTable<BiometricEntity>();
            connection.CreateTable<EventEntity>();
        }
    }
}
