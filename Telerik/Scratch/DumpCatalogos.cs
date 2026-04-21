using System;
using System.Data;
using Telerik.Models.DAL;
using System.Collections.Generic;

namespace Telerik.Scratch
{
    public class DumpCatalogos
    {
        public static void Main()
        {
            try
            {
                DataTable dt = SqlHelper.ExecuteDataTable("SELECT pkTipoServicio, descripcion FROM TiposServicio");
                Console.WriteLine("--- TiposServicio ---");
                foreach (DataRow r in dt.Rows)
                {
                    Console.WriteLine($"{r["pkTipoServicio"]} - {r["descripcion"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
