﻿using Heuristics.Entities;
using Heuristics.Entities.MapsGoogle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Heuristics
{
    class SimpleHeuristicGoogleMaps
    {
        public static void Execute(string folderPath, List<LoadingPlace> loadingPlaces, List<MixerTruck> mixerTrucks,
            List<Order> orders, List<Delivery> deliveries, TrafficInfo trafficInfo,
            double DEFAULT_DIESEL_COST, double DEFAULT_RMC_COST, double FIXED_MIXED_TRUCK_COST,
            double FIXED_MIXED_TRUCK_CAPACIT_M3, double FIXED_L_PER_KM, int FIXED_LOADING_TIME,
            int FIXED_CUSTOMER_FLOW_RATE)
        {
            foreach (LoadingPlace loadingPlace in loadingPlaces)
            {
                LoadingPlace loadingPlaceSister = loadingPlaces.FirstOrDefault(
                lps => lps.CODCENTCUS != loadingPlace.CODCENTCUS &&
                lps.LATITUDE_FILIAL == loadingPlace.LATITUDE_FILIAL &&
                lps.LONGITUDE_FILIAL == loadingPlace.LONGITUDE_FILIAL);
                if(loadingPlaceSister != null)
                    loadingPlace.CODCENTCUSSISTER = loadingPlaceSister.CODCENTCUS;
                loadingPlace.MixerTrucks = mixerTrucks.Where(mt => mt.CODCENTCUS == loadingPlace.CODCENTCUS ||
                    (loadingPlaceSister != null && mt.CODCENTCUS == loadingPlaceSister.CODCENTCUS)).ToList();
            }

            foreach (Order order in orders)
            {
                order.TRIPS = deliveries.Where(d => d.CODPROGRAMACAO == order.CODPROGRAMACAO).ToList();
                foreach (LoadingPlace loadingPlace in loadingPlaces)
                {
                    TrafficInfo.DirectionsResult directionsResult = trafficInfo.DirectionsResults.FirstOrDefault(dr =>
                        Math.Round(dr.OriginLatitude, 6) == loadingPlace.LATITUDE_FILIAL &&
                        Math.Round(dr.OriginLongitude, 6) == loadingPlace.LONGITUDE_FILIAL &&
                        (Math.Round(dr.DestinyLatitude, 8) == order.LATITUDE_OBRA) &&
                        (Math.Round(dr.DestinyLongitude, 8) == order.LONGITUDE_OBRA));
                    LoadingPlaceInfo loadingPlaceInfo = loadingPlace.CreateInfo();
                    foreach (Delivery delivery in order.TRIPS)
                    {
                        TrafficInfo.DirectionsResult directionsResultDelivery = trafficInfo.DirectionsResults.FirstOrDefault(dr =>
                            Math.Round(dr.OriginLatitude, 6) == loadingPlace.LATITUDE_FILIAL &&
                            Math.Round(dr.OriginLongitude, 6) == loadingPlace.LONGITUDE_FILIAL &&
                            (Math.Round(dr.DestinyLatitude, 8) == order.LATITUDE_OBRA) &&
                            (Math.Round(dr.DestinyLongitude, 8) == order.LONGITUDE_OBRA) &&
                            ((dr.Hour == delivery.HORCHEGADAOBRA.Hour) || dr.Hour == (delivery.HORCHEGADAOBRA.Hour - 1)));
                        if (directionsResultDelivery == null)
                        {
                            directionsResultDelivery = directionsResult;
                        }
                        loadingPlaceInfo.Distance = directionsResultDelivery.Distance;
                        loadingPlaceInfo.TravelTime = (int)directionsResultDelivery.TravelTime;
                        if (delivery.CUSVAR <= 0)
                            delivery.CUSVAR = DEFAULT_RMC_COST;
                        loadingPlaceInfo.Cost += ((delivery.CUSVAR * delivery.VALVOLUMEPROG) + 
                            (loadingPlaceInfo.Distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST));
                    }
                    order.LoadingPlaceInfos.Add(loadingPlaceInfo);
                }
                order.LoadingPlaceInfos = order.LoadingPlaceInfos.OrderBy(lpi => lpi.Cost).ToList();
            }

            List<Delivery> deliveryResults = new List<Delivery>();

            orders = orders.OrderBy(o => o.HORSAIDACENTRAL).ToList();
            foreach(Order order in orders)
            {
                bool orderCouldBeServed = false;
                foreach(LoadingPlaceInfo loadingPlaceInfo in order.LoadingPlaceInfos)
                {
                    LoadingPlace loadingPlace = loadingPlaces.FirstOrDefault(lp => lp.CODCENTCUS == loadingPlaceInfo.CODCENTCUS &&
                        lp.MixerTrucks.Count > 0);
                    if(loadingPlace != null)
                    {
                        foreach(Delivery delivery in order.TRIPS)
                        {
                            if (DetermineMixerTruck(delivery, loadingPlace, loadingPlaceInfo, FIXED_LOADING_TIME, 
                                FIXED_CUSTOMER_FLOW_RATE, FIXED_L_PER_KM, DEFAULT_DIESEL_COST))
                            {
                                orderCouldBeServed = true;
                            }
                            else
                            {
                                orderCouldBeServed = false;
                                break;
                            }
                        }
                    }
                    if (orderCouldBeServed)
                    {
                        order.CODCENTCUS = loadingPlaceInfo.CODCENTCUS;
                        break;
                    }
                    else
                    {
                        order.CODCENTCUS = 0;
                    }
                }
                if(orderCouldBeServed == false)
                {
                    Console.WriteLine($"Order {order.CODPROGRAMACAO} could be served...");
                }
                deliveryResults.AddRange(order.TRIPS);
            }

            WriteResults(deliveryResults, loadingPlaces, FIXED_CUSTOMER_FLOW_RATE,
                FIXED_MIXED_TRUCK_COST, folderPath);
        }

        static bool DetermineMixerTruck(Delivery delivery, LoadingPlace loadingPlace,
            LoadingPlaceInfo loadingPlaceInfo, double FIXED_LOADING_TIME, 
            double FIXED_CUSTOMER_FLOW_RATE, double FIXED_L_PER_KM,
            double DEFAULT_DIESEL_COST)
        {
            delivery.Cost = ((delivery.CUSVAR * delivery.VALVOLUMEPROG) +
                (loadingPlaceInfo.Distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST));
            delivery.TravelTime = loadingPlaceInfo.TravelTime;
            delivery.ArrivalTimeAtConstruction = delivery.HORCHEGADAOBRA;
            delivery.BeginLoadingTime =
                delivery.ArrivalTimeAtConstruction.
                    AddMinutes(-loadingPlaceInfo.TravelTime).
                    AddMinutes(-FIXED_LOADING_TIME);
            delivery.EndLoadingTime =
                delivery.BeginLoadingTime.AddMinutes(FIXED_LOADING_TIME);
            delivery.DepartureTimeAtConstruction =
                delivery.ArrivalTimeAtConstruction.
                    AddMinutes((int)(FIXED_CUSTOMER_FLOW_RATE * delivery.VALVOLUMEPROG));
            delivery.ArrivalTimeAtLoadingPlace =
                delivery.DepartureTimeAtConstruction.AddMinutes(loadingPlaceInfo.TravelTime);

            List<MixerTruck> mixerTrucksAvailableInUse = loadingPlace.MixerTrucks.Where(mt =>
                mt.EndOfTheLastService != DateTime.MinValue &&
                mt.EndOfTheLastService <= delivery.BeginLoadingTime).ToList();
            int codVeiculoSelected = 0;
            if (mixerTrucksAvailableInUse.Count > 0)
            {
                TimeSpan idleTime = TimeSpan.MaxValue;
                for (int k = 0; k < mixerTrucksAvailableInUse.Count; k++)
                {
                    TimeSpan currentIdleTime = delivery.BeginLoadingTime.
                        Subtract(mixerTrucksAvailableInUse[k].EndOfTheLastService);
                    if (currentIdleTime < idleTime)
                    {
                        idleTime = currentIdleTime;
                        codVeiculoSelected = mixerTrucksAvailableInUse[k].index;
                    }
                    currentIdleTime = TimeSpan.MaxValue;
                }
            }
            else
            {
                MixerTruck mixerTruckAvailableNotInUse = loadingPlace.MixerTrucks.FirstOrDefault(mt =>
                    mt.EndOfTheLastService == DateTime.MinValue);
                codVeiculoSelected = mixerTruckAvailableNotInUse != null ? mixerTruckAvailableNotInUse.index : 0;
                TimeSpan lateness = TimeSpan.MaxValue;
                if (codVeiculoSelected == 0)
                {
                    foreach (MixerTruck mixerTruck in loadingPlace.MixerTrucks)
                    {
                        TimeSpan currentLateness =
                            mixerTruck.EndOfTheLastService.Subtract(delivery.BeginLoadingTime);
                        if (currentLateness < lateness)
                        {
                            lateness = currentLateness;
                            codVeiculoSelected = mixerTruck.index;
                        }
                    }
                    if (lateness.TotalMinutes > 15)
                        return false;
                    delivery.Cost = ((delivery.CUSVAR * delivery.VALVOLUMEPROG) +
                        (loadingPlaceInfo.Distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST));
                    delivery.TravelTime = loadingPlaceInfo.TravelTime;
                    delivery.Lateness = (int)lateness.TotalMinutes;
                    delivery.BeginLoadingTime =
                        delivery.BeginLoadingTime.AddMinutes(lateness.TotalMinutes);
                    delivery.EndLoadingTime =
                        delivery.EndLoadingTime.AddMinutes(lateness.TotalMinutes);
                    delivery.ArrivalTimeAtConstruction =
                        delivery.ArrivalTimeAtConstruction.AddMinutes(lateness.TotalMinutes);
                    delivery.DepartureTimeAtConstruction =
                        delivery.DepartureTimeAtConstruction.AddMinutes(lateness.TotalMinutes);
                    delivery.ArrivalTimeAtLoadingPlace =
                        delivery.ArrivalTimeAtLoadingPlace.AddMinutes(lateness.TotalMinutes);
                }
            }
            MixerTruck mixerTruckSelected = loadingPlace.MixerTrucks.FirstOrDefault(mt =>
                mt.index == codVeiculoSelected);
            if (mixerTruckSelected != null)
            {
                delivery.CODVEICULO = codVeiculoSelected;
                delivery.CODCENTCUSVIAGEM = loadingPlaceInfo.CODCENTCUS;
                mixerTruckSelected.EndOfTheLastService = delivery.ArrivalTimeAtLoadingPlace;
                return true;
            }
            else
            {
                return false;
            }
        }

        static void WriteResults(List<Delivery> deliveryResults, List<LoadingPlace> loadingPlaces, double FIXED_CUSTOMER_FLOW_RATE,
            double FIXED_MIXED_TRUCK_COST, string folderPath)
        {
            Result result = new Result();
            result.numberOfDeliveries = deliveryResults.Count;
            result.numberOfLoadingPlaces = loadingPlaces.Count;
            result.numberOfMixerTrucks = deliveryResults.GroupBy(d => d.CODVEICULO).Count();
            result.trips = new List<Result.ResultTrip>();
            foreach (Delivery delivery in deliveryResults)
            {
                result.trips.Add(new Result.ResultTrip()
                {
                    OrderId = delivery.CODPROGRAMACAO,
                    Delivery = delivery.CODPROGVIAGEM,
                    MixerTruck = delivery.CODVEICULO,
                    LoadingBeginTime = (delivery.BeginLoadingTime.Hour * 60) + delivery.BeginLoadingTime.Minute,
                    ServiceTime = (delivery.ArrivalTimeAtConstruction.Hour * 60) + delivery.ArrivalTimeAtConstruction.Minute,
                    ReturnTime = (delivery.ArrivalTimeAtLoadingPlace.Hour * 60) + delivery.ArrivalTimeAtLoadingPlace.Minute,
                    LoadingPlant = delivery.CODCENTCUSVIAGEM,
                    Revenue = (int)((delivery.VLRVENDA * delivery.VALVOLUMEPROG) - delivery.Cost),
                    BeginTimeWindow = (delivery.BeginLoadingTime.Hour * 60) + delivery.BeginLoadingTime.Minute,
                    EndTimeWindow = (delivery.ArrivalTimeAtLoadingPlace.Hour * 60) + delivery.ArrivalTimeAtLoadingPlace.Minute,
                    TravelTime = delivery.TravelTime,
                    TravelCost = (int)delivery.Cost,
                    DurationOfService = (int)(FIXED_CUSTOMER_FLOW_RATE * delivery.VALVOLUMEPROG),
                    IfDeliveryMustBeServed = 1,
                    CodDelivery = delivery.CODPROGVIAGEM,
                    CodOrder = delivery.CODPROGRAMACAO,
                    Lateness = delivery.Lateness
                });
            }
            result.objective = (int)(result.trips.Sum(rt => rt.Revenue) - (result.numberOfMixerTrucks * FIXED_MIXED_TRUCK_COST));
            string jsonString = JsonSerializer.Serialize(result);
            File.WriteAllText(folderPath + "\\ResultGoogleMapsSimpleHeuristic.json", jsonString);
        }
    }
}
