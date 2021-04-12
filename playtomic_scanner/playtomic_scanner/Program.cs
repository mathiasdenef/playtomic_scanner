using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace playtomic_scanner
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        private static List<Availability> availabilities;

        static void Main(string[] args)
        {
            //Thread printer = new Thread(new ThreadStart(InvokeMethod));

            //Console.Read();

            while (true)
            {
                //Thread.Sleep(1000 * 60 * 5); // 5 Minutes
                Thread.Sleep(1000 * 5); // 5 seconden

                var newAvailabilities = GetAvailabilitiesForWeek();

                // check difference between availabilities and newAvailabilities
                if (availabilities != null)
                {
                    var differences = GetDifferences(availabilities, newAvailabilities);
                }
                // send discord message if difference is "important"

                availabilities = newAvailabilities;
            }
        }

        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var availabilities = GetAvailabilities(DateTime.Now);
        }

        static List<Availability> GetAvailabilitiesForWeek()
        {
            var availabilitiesForWeek = new List<Availability>();

            var today = DateTime.Now;

            for (var i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                var availabilitiesForDay = GetAvailabilities(date);
                availabilitiesForWeek.AddRange(availabilitiesForDay);
            }
            return availabilitiesForWeek;
        }

        static List<Availability> GetAvailabilities(DateTime date)
        {
            var dateString = date.ToString("yyyy-MM-dd");

            HttpResponseMessage response = client.GetAsync("https://playtomic.io/api/v1/availability?user_id=me&tenant_id=98e6583b-0ba1-4f15-8129-720205de3f4e&sport_id=PADEL&local_start_min=" + dateString + "T00%3A00%3A00&local_start_max=" + dateString + "T23%3A59%3A59").Result;
            response.EnsureSuccessStatusCode();
            string result = response.Content.ReadAsStringAsync().Result;

            var test = JsonConvert.DeserializeObject<List<Availability>>(result);
            return test;
        }

        static List<Availability> GetDifferences(List<Availability> currentList, List<Availability> newList)
        {
            var result = new List<Availability>();

            foreach (var item in newList)
            {
                var matchingItem = currentList.Find(x => x.Resource_id == item.Resource_id && x.Start_date == item.Start_date);

                // if no match is found, it means it's a new one and should be returned
                if (matchingItem == null)
                {
                    result.Add(item);
                }
                else
                {
                    foreach (var slot in item.Slots)
                    {
                        var matchingSlot = matchingItem.Slots.Find(x => x.Start_time == slot.Start_time && x.Duration == slot.Duration);
                        // if no match is found, it means it's a new one and should be returned
                        if (matchingSlot == null)
                        {
                            result.Add(item);
                            continue;
                        }
                    }
                }
            }

            return result;
        }
    }
}
