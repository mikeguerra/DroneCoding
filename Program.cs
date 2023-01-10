using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace DroneCodingTest
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string[] lines = File.ReadAllLines("Input.txt");

            Console.WriteLine("Reading File...");

            int lineNumber = 0;

            List<Drone> drones = new List<Drone>();
            List<Package> packages = new List<Package>();

            List<Trip> trips = new List<Trip>();

            foreach (string line in lines)
            {
                if (lineNumber == 0)
                {
                    //Reading Drones from Text File
                    List<string> strings = line.Split(',').ToList();
                    for (int i = 0; i < strings.Count - 1; i++)
                    {
                        drones.Add(new Drone() { Name = strings[i], MaxWeight = Convert.ToDecimal(strings[i + 1]) });
                        i++;
                    }
                }
                else
                {
                    //Reading Destinies from Text File
                    List<string> strings = line.Split(',').ToList();
                    for (int i = 0; i < strings.Count - 1; i++)
                    {
                        packages.Add(new Package() { Location = strings[i], Weight = Convert.ToDecimal(strings[i + 1]) });
                        i++;
                    }
                }
                Console.WriteLine(line);
                lineNumber++;
            }

            //Trip Algorithm

            //Verify Packages too heavy cannot deliver by drone
            Decimal DroneWithMaxLoad = drones.OrderByDescending(n => n.MaxWeight).First().MaxWeight;

            foreach (var item in packages.Where(n => !n.Assigned && (n.Weight > DroneWithMaxLoad || n.Weight <= 0)))
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Too heavy...");
                Console.WriteLine(item.Location);
                item.Assigned = true;
                item.Status = "Invalid weight";
            }

            //Packages sent with the Drone more capable

            while (packages.Where(n => !n.Assigned).Count() > 0)
            {
                List<Package> packs = packages.Where(n => !n.Assigned).OrderByDescending(n => n.Weight).ToList();

                GetCombinations(ref packs, ref trips, DroneWithMaxLoad);

            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Results...");

            foreach (Drone drone in drones)
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine(drone.Name);
                int tripNumber = 1;
                if (drone.MaxWeight == DroneWithMaxLoad)
                {
                    foreach (var trip in trips.Where(n=>n.Drone == null))
                    {
                        trip.Drone = drone;
                        Console.WriteLine("Trip #" + tripNumber);
                        Console.WriteLine(trip.Name + "==> Total Weight: " + trip.Weight);
                        //Console.WriteLine(Environment.NewLine);
                        tripNumber++;
                    }
                }
            }



            Console.ReadKey();
        }
        private static void GetCombinations(ref List<Package> packages, ref List<Trip> trips, decimal DroneWithMaxLoad)
        {
            //Find max Total of combinations
            double count = Math.Pow(2, packages.Count);
            decimal MaxTotal = 0;
            for (int i = 1; i <= count - 1; i++)
            {
                decimal SemiMaxTotal = 0;
                string str = Convert.ToString(i, 2).PadLeft(packages.Count, '0');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        SemiMaxTotal += packages[j].Weight;
                        //Console.Write(packages[j].Weight);
                    }
                }
                if (SemiMaxTotal > MaxTotal && SemiMaxTotal <= DroneWithMaxLoad) MaxTotal = SemiMaxTotal;

                //Console.WriteLine();
                //Console.Write(SemiMaxTotal + " " + MaxTotal);
                //Console.WriteLine();
            }
            //Assign the best combination
            count = Math.Pow(2, packages.Count);
            for (int i = 1; i <= count - 1; i++)
            {
                Trip trip = new Trip()
                {
                    Name = "".ToString(),
                    Packages = new List<Package>(),
                    Weight = 0,
                };
                string str = Convert.ToString(i, 2).PadLeft(packages.Count, '0');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {

                        trip.Weight += packages[j].Weight;
                        trip.Packages.Add(packages[j]);
                        trip.Name += " " + packages[j].Location + "[" + packages[j].Weight + "]";
                        //Console.Write(packages[j]);
                    }
                }
                //Assign only the best trip to the High capacity Drone
                if (trip.Packages.Sum(n => n.Weight) == MaxTotal)
                {
                    foreach (var pack in trip.Packages)
                    {
                        pack.Assigned = true;
                        pack.Status = "Assigned";
                    }

                    trips.Add(trip);
                    break;
                }
                //Console.WriteLine();
            }
        }
    }



    public class Drone
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Decimal MaxWeight { get; set; }

        public List<Trip> Trips { get; set; }

    }

    public class Trip
    {

        public Drone Drone { get; set; }
        public Decimal Weight { get; set; }
        public string Home { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Package> Packages { get; set; }
    }

    public class Package
    {
        public Trip Trip { get; set; }
        public Decimal Weight { get; set; }
        public string Location { get; set; }
        public int Order { get; set; }

        public bool Assigned { get; set; }
        public string Status { get; set; }

    }

}
