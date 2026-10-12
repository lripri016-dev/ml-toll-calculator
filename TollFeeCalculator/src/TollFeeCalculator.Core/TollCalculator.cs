using TollFeeCalculator.Core.Data;
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

        if (TollFreeCalendar.IsTollFreeDate(date) || vehicle.IsTollFree()) return 0;

        return TollFeeTimeIntervals.GetFee(date);
    }

}