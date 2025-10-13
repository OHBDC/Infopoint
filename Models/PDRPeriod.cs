namespace InfoPoint.Models
{
    /// <summary>
    /// Helper class for managing PDR periods and auto-detection based on current date
    /// </summary>
    public static class PDRPeriod
    {
        public const string OctDec = "Oct-Dec";
        public const string FebMar = "Feb-Mar";
        public const string JunJul = "Jun-Jul";

        /// <summary>
        /// All available PDR periods in the order they occur during the academic year
        /// </summary>
        public static readonly List<string> AllPeriods = new()
        {
            OctDec,  // Oct-Dec (First Review)
            FebMar,  // Feb-Mar (Mid-Year Review)
            JunJul   // Jun-Jul (End of Year Review)
        };

        /// <summary>
        /// Gets the current PDR period based on today's date
        /// </summary>
        public static string GetCurrentPeriod()
        {
            return GetPeriodForDate(DateTime.Now);
        }

        /// <summary>
        /// Gets the appropriate PDR period for a given date
        /// </summary>
        public static string GetPeriodForDate(DateTime date)
        {
            var month = date.Month;

            // October-December window
            if (month >= 10 || month == 12)
                return OctDec;

            // February-March window
            if (month >= 2 && month <= 3)
                return FebMar;

            // June-July window
            if (month >= 6 && month <= 7)
                return JunJul;

            // Default fallback:
            // Jan -> Previous period was Oct-Dec, but we're now in Feb-Mar prep
            if (month == 1)
                return FebMar;

            // Apr-May -> Last period was Feb-Mar, next is Jun-Jul
            if (month >= 4 && month <= 5)
                return JunJul;

            // Aug-Sep -> Last period was Jun-Jul, next is Oct-Dec
            if (month >= 8 && month <= 9)
                return OctDec;

            // Default to Oct-Dec if somehow we didn't match
            return OctDec;
        }

        /// <summary>
        /// Gets the start month for a period (used for sorting and the Month field)
        /// </summary>
        public static int GetStartMonth(string period)
        {
            return period switch
            {
                OctDec => 10,
                FebMar => 2,
                JunJul => 6,
                _ => DateTime.Now.Month
            };
        }

        /// <summary>
        /// Gets the due date for a period in a given year
        /// </summary>
        public static DateTime GetDueDate(string period, int year)
        {
            return period switch
            {
                OctDec => new DateTime(year, 12, 31),  // End of December
                FebMar => new DateTime(year, 3, 31),   // End of March
                JunJul => new DateTime(year, 7, 31),   // End of July
                _ => DateTime.Now.AddDays(30)          // Default: 30 days from now
            };
        }

        /// <summary>
        /// Gets a display-friendly name for a period
        /// </summary>
        public static string GetDisplayName(string period)
        {
            return period switch
            {
                OctDec => "October - December (First Review)",
                FebMar => "February - March (Mid-Year Review)",
                JunJul => "June - July (End of Year Review)",
                _ => period
            };
        }

        /// <summary>
        /// Validates if a period string is valid
        /// </summary>
        public static bool IsValidPeriod(string? period)
        {
            if (string.IsNullOrEmpty(period))
                return false;

            return AllPeriods.Contains(period);
        }
    }
}
