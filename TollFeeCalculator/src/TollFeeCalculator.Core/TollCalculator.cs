using TollFeeCalculator.Core.Vehicles;

public class TollCalculator
{
    /// <summary>No vehicle pays more than 60 SEK per day.</summary>
    private const int MaxDailyFee = 60;

    /// <summary>Passes at most 60 minutes apart are charged as one.</summary>
    private const int WindowMinutes = 60;

    /**
     * Calculate the total toll fee for one day
     *
     * @param vehicle - the vehicle
     * @param dates   - date and time of all passes on one day
     * @return - the total toll fee for that day
     */
    public int GetTollFee(IVehicle vehicle, DateTime[] dates)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        ArgumentNullException.ThrowIfNull(dates);

        if (dates.Length == 0) return 0;

        DateTime intervalStart = dates[0];  // the first date in the array is the start of the interval
        int totalFee = 0;
        foreach (DateTime date in dates)
        {
            int nextFee = GetTollFee(date, vehicle);
            int tempFee = GetTollFee(intervalStart, vehicle);

            TimeSpan timeSinceWindowStart = date - intervalStart;   // FIX-2: DateTime.Millisecond is only the 0-999 sub-second component
            double minutes = timeSinceWindowStart.TotalMinutes;     // real minutes between this pass and the previous one

            if (minutes <= WindowMinutes)
            {
                if (totalFee > 0) totalFee -= tempFee;
                if (nextFee >= tempFee) tempFee = nextFee;
                totalFee += tempFee;
            }
            else
            {
                totalFee += nextFee;
            }

            // FIX-3 the window is sliding. Each pass is compared with the previous
            // pass, not always with the first pass of the day (intervalStart was never updated)
            intervalStart = date;
        }

        return totalFee > MaxDailyFee ? MaxDailyFee : totalFee;
    }

    public int GetTollFee(DateTime date, IVehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (IsTollFreeDate(date) || vehicle.IsTollFree()) return 0;

        int hour = date.Hour;
        int minute = date.Minute;

        if (hour == 6 && minute >= 0 && minute <= 29) return 8;
        else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
        else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
        else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
        else if (hour >= 8 && hour <= 14) return 8;                         // FIX-1
        else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
        else if (hour == 15 || hour == 16) return 18;                       // IMPROVE-1
        else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
        else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
        else return 0;
    }

    private Boolean IsTollFreeDate(DateTime date)
    {
        int year = date.Year;
        int month = date.Month;
        int day = date.Day;

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        if (year == 2013)
        {
            if (month == 1 && day == 1 ||
                month == 3 && (day == 28 || day == 29) ||
                month == 4 && (day == 1 || day == 30) ||
                month == 5 && (day == 1 || day == 8 || day == 9) ||
                month == 6 && (day == 5 || day == 6 || day == 21) ||
                month == 7 ||
                month == 11 && day == 1 ||
                month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
            {
                return true;
            }
        }
        return false;
    }

}