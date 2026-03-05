using SQLite;
using SQLiteMigrations;

namespace Bioss.Ultrasound.Core.Data.Database.Migrations;

public class Migration2To3 : IMigration
{
    public int OldVersion => 2;

    public int NewVersion => 3;

    public void Migrate(SQLiteConnection connection)
    {
        connection.Execute("ALTER TABLE record ADD COLUMN deviceSerial VARCHAR");
    }
}
