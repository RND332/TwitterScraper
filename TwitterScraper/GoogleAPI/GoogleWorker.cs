using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitterScraper.GoogleAPI
{
    internal class GoogleClient : IGoogleInterface 
    {
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "RND Twitter Slave";

        /// <summary>
        /// Api key for accsess to spreadsheets
        /// </summary>
        protected string Key;

        /// <summary>
        /// SpreadSheeID storage
        /// </summary>
        protected string SpreadSheetId = "1K8-YjqVAY2d8AzDXCvd5tRwo_FkEfD6-5uscTuTABvA";

        /// <summary>
        /// All Sheets for this SpreadSheet
        /// </summary>
        protected List<string> Sheets = new List<string> { "TEST" };

        protected SpreadsheetsResource Spreadsheet;

        /// <summary>
        /// Servise who used to read/write 
        /// </summary>
        protected SheetsService service;

        private List<string> GetSheets() 
        {
            try
            {
                SpreadsheetsResource.GetRequest request = service.Spreadsheets.Get(SpreadSheetId);
                Spreadsheet response = request.Execute();
                Console.WriteLine(response);
                List<string> sheets = new List<string>();

                foreach (var sheet in response.Sheets)
                {
                    sheets.Add(sheet.Properties.Title);
                }
                return sheets;
            }
            catch (Google.GoogleApiException ex) // Catch wrong key/spreadsheet
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private bool TestConnection() 
        {
            Console.WriteLine("Testing connection...");

            // Get Service Spreadsheets for Get method
            this.Spreadsheet = service.Spreadsheets;

            try
            {
                var range = $"{GetSheets()[0]}!A1:A100";
                var request = service.Spreadsheets.Values.Get(SpreadSheetId, range);

                var responce = request.Execute();

                if (responce.Values != null)
                {
                    return true;
                }
                else 
                {
                    return false;   
                }
            }
            catch (Google.GoogleApiException ex) // Catch wrong key/spreadsheet
            {
                // return false, due to failed test
                return false;
            }
        }

        /// <summary>
        /// Make new GoogleClient object for read/write/clear/new functions
        /// </summary>
        /// <param name="NeedToTest">true for make test connection and check spreadshee, false for dont do test</param>
        public GoogleClient(bool NeedToTest = true) 
        {
            GoogleCredential googleClient;
            using (Stream stream = new FileStream("creds.json", FileMode.Open, FileAccess.Read))
            {
                googleClient = GoogleCredential.FromStream(stream).CreateScoped();
            }

            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = googleClient,
                ApplicationName = "RNDTwitterSlave",

            });

            if (NeedToTest)
            {
                if (!TestConnection()) 
                {
                    throw new Exception("Test failed");
                }
            }
        }
        public void ClearTable(string TableName)
        {
            try
            {
                var range = $"{TableName}!A1:Z100";
                var request = service.Spreadsheets.Values.Clear(new ClearValuesRequest(),SpreadSheetId, range);

                var responce = request.Execute();
            }
            catch (Google.GoogleApiException ex) // Catch wrong key/spreadsheet
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void NewTable(string TableName)
        {
            try
            {
                // Making new Sheet propirties
                var addSheetRequest = new AddSheetRequest
                {
                    Properties = new SheetProperties()
                    {
                        Title = TableName
                    }
                };

                // Wrap into BatchUpdateSpreadsheetRequest class for execution
                var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Request>
                    {
                        new Request
                        {
                            AddSheet = addSheetRequest
                        }
                    }
                };

                var batchUpdateRequest =
                    service.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, SpreadSheetId);

                batchUpdateRequest.Execute();
            }
            catch (Google.GoogleApiException ex) // Catch wrong key/spreadsheet
            {
                Console.WriteLine(ex.Message);
            }
        }

        public ValueRange ReadSheet(string TableName,
            string StartCell,
            string EndCell,
            SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum @enum = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.COLUMNS)
        {
            try
            {
                var range = $"{TableName}!{StartCell}:{EndCell}";
                var request = service.Spreadsheets.Values.Get(SpreadSheetId, range);
                request.MajorDimension = @enum;

                return request.Execute();
            }
            catch (Google.GoogleApiException ex) // Catch wrong key/spreadsheet
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void WriteSheet(List<List<string>> data, string Dimension = "COLUMNS",  string TableName = null, string StartCell = "A1", string EndCell = "Z100")
        {
            try
            {
                if (TableName == null)
                {
                    NewTable("Comments_" + DateTime.Today.Date.ToString("yyyy-MM-dd"));
                    TableName = "Comments_" + DateTime.Today.Date.ToString("yyyy-MM-dd");
                }
                else if (!(GetSheets().Contains(TableName)))
                {
                    NewTable(TableName);
                }
                else 
                {
                    ClearTable(TableName);
                }

                var range = $"{TableName}!{StartCell}:{EndCell}";

                var nnn = new List<IList<object>>();
                foreach (var item in data)
                {
                    var tmp = new List<object>();
                    tmp.AddRange(item);
                    nnn.Add(tmp);
                }

                var Value = new ValueRange()
                {
                    Values = nnn,
                    MajorDimension = Dimension,
                };
                var request = service.Spreadsheets.Values.Update(Value, SpreadSheetId, range);
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

                request.Execute();
            }
            catch (Google.GoogleApiException ex) // Catch wrong key/spreadsheet
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
