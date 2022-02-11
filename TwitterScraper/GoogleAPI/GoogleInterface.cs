using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterScraper.GoogleAPI
{
    internal interface IGoogleInterface
    {
        /// <summary>
        /// Returns a two-dimensional string array of strings, from startCell to endCell
        /// </summary>
        /// <param name="TableName">Name of sheet to read</param>
        /// <param name="StartCell"></param>
        /// <param name="EndCell"></param>
        /// <returns></returns>
        public ValueRange ReadSheet(string TableName, string StartCell, string EndCell, SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum @enum);

        /// <summary>
        /// Write the <paramref name="data"/> to <paramref name="TableName"/> starting on <paramref name="StartCell"/>
        /// </summary>
        /// <param name="TableName">Name of Table</param>
        /// <param name="data">Data to wrute into <paramref name="TableName"/> 
        /// example: [[[1], [1 first], [1 second]],
        ///          [[2], [2 first], [2 second]]]
        ///          </param>
        /// <param name="StartCell">The cell where write starting, example: A1 or B17</param>
        /// <returns></returns>
        public void WriteSheet(List<List<string>> data, string Dimension, string TableName, string StartCell, string EndCell);

        /// <summary>
        /// Make new Table
        /// </summary>
        /// <param name="TableName">Name of the new Table</param>
        public void NewTable(string TableName);

        /// <summary>
        /// Deleate all data from sheet
        /// </summary>
        /// <param name="TableName">The name of table to be cleared</param>
        public void ClearTable(string TableName);
    }
}
