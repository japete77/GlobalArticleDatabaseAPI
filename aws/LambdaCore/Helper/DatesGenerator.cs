using System;

namespace LambdaCore.Helper
{
    public class DatesGenerator
    {
        public void PrintDates(DateTime start, DateTime end)
        {
            while (start < end)
            {
                if (start.DayOfWeek == DayOfWeek.Monday ||
                    start.DayOfWeek == DayOfWeek.Thursday)
                {
                    Console.WriteLine(start.ToString("dd/MM/yyyy"));
                }

                start = start.AddDays(1);
            }
        }
    }
}
