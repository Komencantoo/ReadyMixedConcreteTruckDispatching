﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Heuristics.ConstructiveHeuristics
{
    public class DeliveryByDeliveryAllocation
    {
        public static void Execute(string folderPath)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            #region GetInputParameters
            int nc = 0;
            int np = 0;
            int nv = 0;

            string[] lines = File.ReadAllLines(folderPath + "\\BianchessiReal.dat");

            int lineCounter = 1;
            int nptt = 0;
            int npcc = 0;
            foreach (string line in lines)
            {
                if (lineCounter <= 3)
                {
                    string auxStr = string.Empty;
                    for (int i = 0; i < line.Length; i++)
                        if (Char.IsDigit(line[i]))
                            auxStr += line[i];
                    if (auxStr.Length > 0)
                    {
                        if (lineCounter == 1)
                            nc = int.Parse(auxStr);
                        else if (lineCounter == 2)
                            np = int.Parse(auxStr);
                        else if (lineCounter == 3)
                            nv = int.Parse(auxStr);
                    }
                }
                else
                {
                    break;
                }
                lineCounter++;
            }
            int[,] tt = new int[np, nc];
            int[,] cc = new int[np, nc];
            int[] s = new int[nc];
            int[] cfr = new int[nc];
            int[] vold = new int[nc];
            int[] codLoadingPlants = new int[np];
            int[] codOrders = new int[nc];
            int[] codDeliveries = new int[nc];
            lineCounter = 0;
            foreach (string line in lines)
            {
                if (lineCounter >= 4 && lineCounter < (4 + np))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    var values = val.Split(',');
                    int nvtt = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            tt[nptt, nvtt] = int.Parse(value);
                            nvtt++;
                        }
                    }
                    nptt++;
                }
                else if (lineCounter >= (4 + np + 2) && lineCounter < (4 + np + 2 + np))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    var values = val.Split(',');
                    int nvcc = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            cc[npcc, nvcc] = int.Parse(value);
                            nvcc++;
                        }
                    }
                    npcc++;
                }
                else if (lineCounter == (4 + np + 2 + np + 1))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    val = val.Replace("s", "");
                    var values = val.Split(',');
                    int nvs = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            s[nvs] = int.Parse(value);
                            nvs++;
                        }
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 2))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    val = val.Replace("c", "");
                    val = val.Replace("f", "");
                    val = val.Replace("r", "");
                    var values = val.Split(',');
                    int nvcfr = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            cfr[nvcfr] = int.Parse(value);
                            nvcfr++;
                        }
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 3))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    val = val.Replace("v", "");
                    val = val.Replace("o", "");
                    val = val.Replace("l", "");
                    val = val.Replace("d", "");
                    var values = val.Split(',');
                    int nvvold = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            vold[nvvold] = int.Parse(value);
                            nvvold++;
                        }
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 4))
                {
                    int index = line.IndexOf("codLoadingPlants");
                    string cleanPath = (index < 0)
                        ? line
                        : line.Remove(index, "codLoadingPlants".Length);
                    string val = cleanPath.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    var values = val.Split(',');
                    int nvvold = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            codLoadingPlants[nvvold] = int.Parse(value);
                            nvvold++;
                        }
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 5))
                {
                    int index = line.IndexOf("codOrders");
                    string cleanPath = (index < 0)
                        ? line
                        : line.Remove(index, "codOrders".Length);
                    string val = cleanPath.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    var values = val.Split(',');
                    int nvcodOrders = 0;
                    foreach (var value in values)
                    {
                        codOrders[nvcodOrders] = int.Parse(value);
                        nvcodOrders++;
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 6))
                {
                    int index = line.IndexOf("codDeliveries");
                    string cleanPath = (index < 0) ? line : line.Remove(index, "codDeliveries".Length);
                    string val = cleanPath.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    var values = val.Split(',');
                    int nvcodDeliveries = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            codDeliveries[nvcodDeliveries] = int.Parse(value);
                            nvcodDeliveries++;
                        }
                    }
                }
                lineCounter++;
            }
            #endregion

            List<Delivery> deliveries = new List<Delivery>();
            List<LoadingPlace> loadingPlaces = new List<LoadingPlace>();
            for (int j = 0; j < np; j++)
            {
                LoadingPlace loadingPlaceInfo = new LoadingPlace();
                loadingPlaceInfo.LoadingPlaceId = j;
                loadingPlaceInfo.CodLoadingPlace = codLoadingPlants[j];
                loadingPlaces.Add(loadingPlaceInfo);
            }
            for (int i = 0; i < nc; i++)
            {
                Delivery newDelivery = new Delivery();
                newDelivery.DeliveryId = i;
                newDelivery.CodDelivery = codDeliveries[i];
                newDelivery.CodOrder = codOrders[i];
                newDelivery.ServiceTime = s[i];
                newDelivery.CustomerFlowRate = cfr[i];
                newDelivery.Volume = vold[i];
                newDelivery.LoadingPlaceInfos = new List<LoadingPlaceInfo>();
                for (int j = 0; j < np; j++)
                {
                    LoadingPlaceInfo loadingPlaceInfo = new LoadingPlaceInfo();
                    loadingPlaceInfo.LoadingPlaceId = j;
                    loadingPlaceInfo.CodLoadingPlace = codLoadingPlants[j];
                    loadingPlaceInfo.TripDuration = tt[j, i];
                    loadingPlaceInfo.Cost = cc[j, i];
                    newDelivery.LoadingPlaceInfos.Add(loadingPlaceInfo);
                }
                deliveries.Add(newDelivery);
            }

            deliveries = deliveries.OrderBy(d => d.ServiceTime).ToList();
            List<Route> routes = new List<Route>();
            foreach (Delivery delivery in deliveries)
            {
                delivery.LoadingPlaceInfos = delivery.LoadingPlaceInfos.OrderBy(lp => lp.Cost).ToList();
                var cheapestLoadingPlace = delivery.LoadingPlaceInfos.FirstOrDefault();
                int maximalDeliveryLoadingTime = (delivery.ServiceTime + 15) -
                    cheapestLoadingPlace.TripDuration - 10;
                Route route = routes.FirstOrDefault(r => r.NextAvailableTime <= maximalDeliveryLoadingTime);
                if (route == null)
                {
                    route = new Route();
                    route.Deliveries = new List<Delivery>();
                    route.LoadingPlaceId = cheapestLoadingPlace.LoadingPlaceId;
                    route.CodLoadingPlace = cheapestLoadingPlace.CodLoadingPlace;
                    route.RouteString = $"Base [{route.LoadingPlaceId}] -> Custommer [{delivery.DeliveryId}]";
                    routes.Add(route);
                }
                else
                {
                    route.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                }
                delivery.BaseLoadingPlaceId = cheapestLoadingPlace.LoadingPlaceId;
                delivery.CodLoadingPlace = cheapestLoadingPlace.CodLoadingPlace;
                if(!route.NextAvailableTime.HasValue)
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                }
                else if (route.NextAvailableTime.HasValue && 
                    (route.NextAvailableTime <= (maximalDeliveryLoadingTime - 15)))
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                }
                else if (route.NextAvailableTime.HasValue && 
                    (route.NextAvailableTime <= maximalDeliveryLoadingTime))
                {

                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10 + 
                        (route.NextAvailableTime - (maximalDeliveryLoadingTime - 15));
                    delivery.BeginServiceTime = delivery.LoadingBeginTime +
                        cheapestLoadingPlace.TripDuration + 10;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                }
                delivery.Delay = delivery.BeginServiceTime.Value - delivery.ServiceTime;
                route.NextAvailableTime = delivery.ArrivaTimeAtPlant;
                route.NumberOfCustomersInRoute++;
                route.TotalCost += cheapestLoadingPlace.Cost;
                route.Deliveries.Add(Heuristics.Extensions.ObjectExtensions.DeepClone<Delivery>(delivery));
            }

            double minCost = 0;
            int trucksRouteCount = 1;
            foreach (var route in routes)
            {
                minCost += route.TotalCost;
                route.MixerTruck = trucksRouteCount;
                Console.WriteLine($"Truck Route [{trucksRouteCount}] : " + route.RouteString);
                trucksRouteCount++;
            }
            Console.WriteLine($"\n\nTotal Cost = {(routes.Count * 50) + minCost}");

            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            Console.WriteLine($"\n\nTotal Elapsed Time: {Convert.ToInt32(stopwatchElapsed.TotalSeconds)}\n\n");

            List<int> diferentLoadingPlaces = routes.Select(r => r.LoadingPlaceId).Distinct().ToList();
            foreach(int loadingPlaceId in diferentLoadingPlaces)
            {
                var routesFromLp = routes.Where(r => r.LoadingPlaceId == loadingPlaceId).ToList();
                Vns(routes);
            }
        }

        public static void Vns(List<Route> routes)
        {
            List<Route> refRoutes = Heuristics.Extensions.ObjectExtensions.DeepClone<List<Route>>(routes);
            int k = 0;
            int maxK = 1000;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int numberOfTrucksUsed = routes.Count();
            double totalRouteCost = routes.Sum(r => r.TotalCost) + (50 * numberOfTrucksUsed);
            double currentRouteCost = totalRouteCost;
            List<Delivery> deliveriesToBeReallocated = new List<Delivery>();
            while (currentRouteCost >= totalRouteCost || deliveriesToBeReallocated.Count() > 0)
            {
                int firstRouteIndex = GetRandomRoute(routes, numberOfTrucksUsed);
                Route firstRoute = routes[firstRouteIndex];
                int tripIndexOfFirstRoute = GetRandomTripOfRoute(routes, firstRouteIndex);
                Delivery tripToBeReallocatedInSecondRoute = firstRoute.Deliveries[tripIndexOfFirstRoute];

                int secondRouteIndex = GetRandomRouteDifferentFromAnotherRoute(numberOfTrucksUsed, firstRouteIndex);
                Route secondRoute = routes[secondRouteIndex];
                int tripIndexOfSecondRoute = GetRandomTripOfRoute(routes, secondRouteIndex);
                Delivery tripToBeRemovedFromSecondRoute = secondRoute.Deliveries[tripIndexOfSecondRoute];

                Route copySecondRoute = Heuristics.Extensions.ObjectExtensions.DeepClone<Route>(secondRoute);
                copySecondRoute.Deliveries.Add(tripToBeReallocatedInSecondRoute);
                Route newSecondRoute = new Route();
                newSecondRoute.Deliveries = new List<Delivery>();

                copySecondRoute.Deliveries.OrderBy(d => d.ServiceTime);
                foreach (Delivery delivery in copySecondRoute.Deliveries)
                {
                    if(delivery.DeliveryId != tripToBeRemovedFromSecondRoute.DeliveryId)
                    {
                        delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        delivery.LoadingPlaceInfos[0].TripDuration - 10;
                        delivery.BeginServiceTime = delivery.ServiceTime;
                        delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                            (delivery.Volume * delivery.CustomerFlowRate));
                        delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                            delivery.LoadingPlaceInfos[0].TripDuration;
                        delivery.WaitingTimeBeforeLoading = null;
                        if (newSecondRoute.Deliveries.Count() == 0)
                        {
                            delivery.Delay = 0;
                            newSecondRoute.NumberOfCustomersInRoute++;
                            newSecondRoute.RouteString = $"Base [{delivery.LoadingPlaceInfos[0].LoadingPlaceId}] -> Custommer [{delivery.DeliveryId}]";
                            newSecondRoute.NextAvailableTime = delivery.ArrivaTimeAtPlant;
                            newSecondRoute.MixerTruck = secondRoute.MixerTruck;
                            newSecondRoute.LoadingPlaceId = delivery.LoadingPlaceInfos[0].LoadingPlaceId;
                            newSecondRoute.CodLoadingPlace = delivery.LoadingPlaceInfos[0].CodLoadingPlace;
                            newSecondRoute.TotalCost = delivery.LoadingPlaceInfos[0].Cost;
                            newSecondRoute.Deliveries.Add(delivery);
                        }
                        else
                        {
                            if (newSecondRoute.NextAvailableTime <= delivery.LoadingBeginTime)
                            {
                                delivery.Delay = 0;
                                delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - newSecondRoute.NextAvailableTime;
                                newSecondRoute.NumberOfCustomersInRoute++;
                                newSecondRoute.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                                newSecondRoute.NextAvailableTime = delivery.ArrivaTimeAtPlant;
                                newSecondRoute.TotalCost = delivery.LoadingPlaceInfos[0].Cost;
                                newSecondRoute.Deliveries.Add(delivery);
                            }
                            else if (newSecondRoute.NextAvailableTime <= (delivery.LoadingBeginTime + 15))
                            {
                                delivery.LoadingBeginTime = newSecondRoute.NextAvailableTime;
                                delivery.BeginServiceTime = delivery.LoadingBeginTime + 10 +
                                    delivery.LoadingPlaceInfos[0].TripDuration;
                                delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                                    (delivery.Volume * delivery.CustomerFlowRate));
                                delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                                    delivery.LoadingPlaceInfos[0].TripDuration;
                                delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - newSecondRoute.NextAvailableTime;
                                delivery.Delay = delivery.BeginServiceTime.Value - delivery.ServiceTime;
                                newSecondRoute.NumberOfCustomersInRoute++;
                                newSecondRoute.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                                newSecondRoute.NextAvailableTime = delivery.ArrivaTimeAtPlant;
                                newSecondRoute.TotalCost += delivery.LoadingPlaceInfos[0].Cost;
                                newSecondRoute.Deliveries.Add(delivery);
                            }
                        }
                    }
                }

                if (newSecondRoute.TotalWaitingTime < secondRoute.TotalWaitingTime)
                {
                    foreach (Delivery delivery in newSecondRoute.Deliveries)
                        copySecondRoute.Deliveries.RemoveAll(d => d.DeliveryId == delivery.DeliveryId);
                    routes.Remove(secondRoute);
                    routes.Add(newSecondRoute);
                    firstRoute.Deliveries.Remove(tripToBeReallocatedInSecondRoute);
                    if (firstRoute.Deliveries.Count() == 0)
                        routes.Remove(firstRoute);
                    numberOfTrucksUsed = routes.Count();
                    currentRouteCost = routes.Sum(r => r.TotalCost) + (50 * numberOfTrucksUsed);
                    deliveriesToBeReallocated.AddRange(copySecondRoute.Deliveries);
                }
                else
                {
                    if(k > maxK)
                    {
                        routes = Heuristics.Extensions.ObjectExtensions.DeepClone<List<Route>>(refRoutes);
                        deliveriesToBeReallocated = new List<Delivery>();
                        k = 0;
                        currentRouteCost = totalRouteCost;
                        numberOfTrucksUsed = routes.Count();
                    }
                }
                deliveriesToBeReallocated = ReallocateRemovedTrips(routes, deliveriesToBeReallocated);
                k++;
            }
            double minCost = 0;
            int trucksRouteCount = 1;
            foreach (var route in routes)
            {
                minCost += route.TotalCost;
                route.MixerTruck = trucksRouteCount;
                Console.WriteLine($"Truck Route [{trucksRouteCount}] : " + route.RouteString);
                trucksRouteCount++;
            }
            Console.WriteLine($"\n\nTotal Cost = {(routes.Count * 50) + minCost}");

            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            Console.WriteLine($"\n\nTotal Elapsed Time: {Convert.ToInt32(stopwatchElapsed.TotalSeconds)}\n\n");
        }
        private static List<Delivery> ReallocateRemovedTrips(List<Route> routes, List<Delivery> deliveriesToBeReallocated)
        {
            List<Delivery> copyDeliveriesToBeReallocated = Heuristics.Extensions.ObjectExtensions.DeepClone<List<Delivery>>(deliveriesToBeReallocated);
            foreach (Delivery delivery in deliveriesToBeReallocated)
            {
                int delay = 0;
                Route route = routes.FirstOrDefault(r => r.NextAvailableTime <= 
                    (delivery.ServiceTime - delivery.LoadingPlaceInfos[0].TripDuration - 10));
                if(route == null)
                {
                    route = routes.FirstOrDefault(r => r.NextAvailableTime <= 
                        (delivery.ServiceTime + 15 - delivery.LoadingPlaceInfos[0].TripDuration - 10));
                    if(route != null)
                    {
                        delay = route.NextAvailableTime.Value - 
                            (delivery.ServiceTime - delivery.LoadingPlaceInfos[0].TripDuration - 10);
                    }
                }
                if(route != null)
                {
                    delivery.LoadingBeginTime = delivery.ServiceTime + delay -
                        delivery.LoadingPlaceInfos[0].TripDuration - 10;
                    delivery.BeginServiceTime = delivery.LoadingBeginTime + 10 +
                                delivery.LoadingPlaceInfos[0].TripDuration;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        delivery.LoadingPlaceInfos[0].TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                    delivery.Delay = delivery.BeginServiceTime.Value - delivery.ServiceTime;
                    route.Deliveries.Add(delivery);
                    copyDeliveriesToBeReallocated.RemoveAll(d => d.DeliveryId == delivery.DeliveryId);
                }
            }
            return copyDeliveriesToBeReallocated;
        }
        private static int GetRandomRoute(List<Route> routes, int numberOfTrucksUsed)
        {
            int smallestNumberOfTripsPerRoute = routes.Select(r => r.NumberOfCustomersInRoute).Min();
            Random r = new Random();
            int firstRoute = r.Next(0, numberOfTrucksUsed);
            while (true)
            {
                if (routes[firstRoute].NumberOfCustomersInRoute == smallestNumberOfTripsPerRoute)
                    break;
                firstRoute = r.Next(0, numberOfTrucksUsed);
            }
            return firstRoute;
        }
        private static int GetRandomRouteDifferentFromAnotherRoute(int numberOfTrucksUsed, int firstRoute)
        {
            Random r = new Random();

            int secondRoute = firstRoute;
            while(secondRoute == firstRoute)
                secondRoute = r.Next(0, numberOfTrucksUsed);
            return secondRoute;
        }
        private static int GetRandomTripOfRoute(List<Route> routes, int indexOfRoute)
        {
            Random r = new Random();
            int tripOfFirstRoute = r.Next(0, routes[indexOfRoute].Deliveries.Count);
            return tripOfFirstRoute;
        }
    }
    [Serializable]
    public class Route
    {
        public int NumberOfCustomersInRoute { get; set; }
        public string RouteString { get; set; }
        public int? NextAvailableTime { get; set; }
        public int? MixerTruck { get; set; }
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
        public double TotalCost { get; set; }
        
        public int TotalWaitingTime 
        {
            get 
            {
                return Deliveries.Sum(d => d.WaitingTimeBeforeLoading.HasValue ? d.WaitingTimeBeforeLoading.Value : 0);
            }
        }

        public List<Delivery> Deliveries { get; set; }
    };
    [Serializable]
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public int CodOrder { get; set; }
        public int CodDelivery { get; set; }
        public int ServiceTime { get; set; }
        public double CustomerFlowRate { get; set; }
        public double Volume { get; set; }
        public List<LoadingPlaceInfo> LoadingPlaceInfos { get; set; }

        public int? BaseLoadingPlaceId { get; set; }
        public int? CodLoadingPlace { get; set; }
        public int? WaitingTimeBeforeLoading { get; set; }
        public int Delay { get; set; }
        public int? LoadingBeginTime { get; set; }
        public int? BeginServiceTime { get; set; }
        public int? EndServiceTime { get; set; }
        public int? ArrivaTimeAtPlant { get; set; }
    }
    [Serializable]
    public class LoadingPlaceInfo
    {
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
        public int TripDuration { get; set; }
        public double Cost { get; set; }
    }
    [Serializable]
    public class LoadingPlace
    {
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
    }
}
