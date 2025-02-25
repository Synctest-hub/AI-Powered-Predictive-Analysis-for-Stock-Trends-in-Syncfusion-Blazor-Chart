using System.Globalization;

namespace AIStockTrends.Components.Pages
{
    public partial class Home
    {
        public List<StockDataModel> StockData { get; set; } = new List<StockDataModel>();
        public double ZoomFactor { get; set; } = 1;
        public DateTime StripStartDate { get; set; }
        public DateTime StripEndDate { get; set; }
        public bool IsOneMonthSelected { get; set; }
        public bool IsThreeMonthsSelected { get; set; }
        public bool IsSixMonthsSelected { get; set; }


        public double totalPoints = 0;

        protected override async Task OnInitializedAsync()
        {
            StockData = await Http.GetFromJsonAsync<List<StockDataModel>>(NavigationManager.BaseUri + "data/financial-data.json");
        }

        //To show last one month data.
        private void GetOneMonthData()
        {
            totalPoints = StockData.Count;
            ZoomFactor = 30 / totalPoints;
            IsOneMonthSelected = true;
            IsThreeMonthsSelected = IsSixMonthsSelected = false;
        }

        //To show last three months data.
        private void GetThreeMonthsData()
        {
            totalPoints = StockData.Count;
            ZoomFactor = 60 / totalPoints;
            IsThreeMonthsSelected = true;
            IsOneMonthSelected = IsSixMonthsSelected = false;
        }

        //To show last six months data.
        private void GetSixMonthsData()
        {
            totalPoints = StockData.Count;
            ZoomFactor = 180 / totalPoints;
            IsSixMonthsSelected = true;
            IsOneMonthSelected = IsThreeMonthsSelected = false;
        }

        private async Task AIButtonClicked()
        {
            List<StockDataModel> last10Items = StockData.Skip(Math.Max(0, StockData.Count - 10)).Take(10).ToList();

            string prompt = AIService.GeneratePrompt(last10Items);

            string value = await AIService.GetResponseFromOpenAI(prompt);

            if (string.IsNullOrWhiteSpace(value)) return;

            await GenerateCollection(value);
        }

        private async Task GenerateCollection(string dataSource)
        {
            StripStartDate = StockData.Last().Period;
            string[] rows = dataSource.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (StockData == null)
                StockData = new List<StockDataModel>();

            foreach (string row in rows)
            {
                string[] columns = row.Split(':');

                if (columns.Length < 5) continue;

                if (!DateTime.TryParse(columns[0].Trim(), out DateTime period)) continue;

                StockDataModel item = new StockDataModel
                {
                    Period = period,
                    Open = GetDouble(columns[3]),
                    High = GetDouble(columns[1]),
                    Low = GetDouble(columns[2]),
                    Close = GetDouble(columns[4])
                };

                StripEndDate = period;
                StockData.Add(item);
                await InvokeAsync(StateHasChanged);

                // Simulate live update every 500ms
                await Task.Delay(500);
            }
        }

        public double GetDouble(string text)
        {
            double i = 0;

            if (double.TryParse(text, out i))
            {
                return i;
            }

            return i;
        }

        public DateTime GetDateTime(string text)
        {
            var i = DateTime.Now;

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out i))
            {
                return i;
            }

            return i;
        }

        public class StockDataModel
        {
            public DateTime Period { get; set; }
            public double High { get; set; }
            public double Low { get; set; }
            public double Open { get; set; }
            public double Close { get; set; }
        }
    }
}
