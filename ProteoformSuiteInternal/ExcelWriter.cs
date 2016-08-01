//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Excel = Microsoft.Office.Interop.Excel; //ProteoformSuiteInternal > Add > Assemblies > Microsoft.Office.Interop.Excel
//using System.Reflection;
//using System.IO;

//namespace ProteoformSuiteInternal
//{
//    class ExcelWriter
//    {
//        Excel.Application excel = new Excel.Application();

//        private void add_sheets_to_workbook()
//        {
//            var workbook = (Excel._Workbook) (excel.Workbooks.Add(Missing.Value));

//            for (var i = 0; i < 9; i++) //there are 9 lists to export
//            {
//                if (workbook.Sheets.Count <= i)
//                {
//                    workbook.Sheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
//                }

//                //NOTE: Excel #ing goes from 1 to n
//                var sheet = (Excel._Worksheet)workbook.Sheets[i + 1];
                

//            }


//        }

//        private void add_cells_to_sheets<T>(List<T> list_to_add, Excel._Worksheet sheet)
//        {
//            var range = sheet.get_Range("A1", "A1");
//            range.Value2 = "test";

//            string cellName;
//            int counter = 1;
            
//            foreach(var item in list_to_add)
//            {
//                cellName = "A" + counter.ToString();
//                range = sheet.get_Range(cellName, cellName);
//                range.Value2 = item.as_tsv_row();
//                ++counter;
//            }
//        }

        
 
//    }
//}
