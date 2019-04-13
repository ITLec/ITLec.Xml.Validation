using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITLec.XmlValidation.Csv
{


    public class CsvHelper
    {


        public static string SaveLastPrintedTableToCSV(string filePath, DataTable dataTable)
        {
            
            bool exists = System.IO.Directory.Exists(filePath);
            


            //   using (System.IO.StreamWriter sr = new System.IO.StreamWriter(filePath, true, Encoding.UTF8))
            using (ReadWriteCsv.CsvFileWriter writer = new ReadWriteCsv.CsvFileWriter(filePath))
            {

                ReadWriteCsv.CsvRow header = new ReadWriteCsv.CsvRow();
                foreach (System.Data.DataColumn dataColumn in dataTable.Columns)
                {
                    header.Add(dataColumn.ColumnName);
                }
                writer.WriteRow(header);

                foreach (System.Data.DataRow dataRow in dataTable.Rows)
                {
                    ReadWriteCsv.CsvRow row = new ReadWriteCsv.CsvRow();
                    foreach (System.Data.DataColumn dataColumn in dataRow.Table.Columns)
                    {
                        row.Add(dataRow[dataColumn.ColumnName].ToString());
                    }
                    writer.WriteRow(row);
                }
            }

            return filePath;
        }





        public static System.Data.DataTable ConvertCSVToDataTable(string csvFilePath, string[] headers = null)

        {
            System.Data.DataTable dataTable = loadCSVWithHeaderToDataTable(csvFilePath);

            if (headers != null && headers.Length > 0)
            {
                foreach (string header in headers)
                {
                    if (!dataTable.Columns.Contains(header))
                    {
                        throw new Exception($"{header} is not existed header. filePath: {csvFilePath}");
                    }
                }
            }



            return dataTable;
        }
        private static System.Data.DataTable loadCSVWithHeaderToDataTable(string csvFilePath)

        {
            int counter = 0;
            System.Data.DataTable dataTable = new System.Data.DataTable();


            using (ReadWriteCsv.CsvFileReader reader = new ReadWriteCsv.CsvFileReader(csvFilePath))

            {

                ReadWriteCsv.CsvRow row = new ReadWriteCsv.CsvRow();

                while (reader.ReadRow(row))

                {

                    if (counter == 0)

                    {

                        foreach (var column in row)

                        {

                            dataTable.Columns.Add(new System.Data.DataColumn(column, typeof(string)));
                        }

                    }

                    else

                    {

                        System.Data.DataRow dr = dataTable.NewRow();

                        int index = 0;

                        foreach (var column in row)

                        {
                            dr[index++] = column;

                        }

                        dataTable.Rows.Add(dr);

                    }

                    counter++;

                }

            }
            return dataTable;
        }
    }

}
