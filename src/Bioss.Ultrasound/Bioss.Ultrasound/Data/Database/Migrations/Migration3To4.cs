using Bioss.Ultrasound.Data.Database.Entities;
using SQLite;
using SQLiteMigrations;

namespace Bioss.Ultrasound.Data.Database.Migrations
{
    public class Migration3To4 : IMigration
    {
        public int OldVersion => 3;

        public int NewVersion => 4;

        public void Migrate(SQLiteConnection connection)
        {
            connection.CreateTable<LogEntity>();
        }
    }
}
