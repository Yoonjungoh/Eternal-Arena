using Server.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Extensions
{
    public static bool SaveChangeEx(this GameDbContext db)
    {
        try
        {
            db.SaveChanges();
            return false;
        }
        catch (Exception ex)
        {
            ConsoleLogManager.Instance.Log($"[Error] DB Update Exception: {ex.Message}");
            return false;
        }
    }
}