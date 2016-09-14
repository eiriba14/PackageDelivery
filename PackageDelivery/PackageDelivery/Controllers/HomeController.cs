﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PackageDelivery.Models;

namespace PackageDelivery.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public ActionResult Index(string message)
        {
            ViewBag.Success = message;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize(Roles = "Owner,Admin,Customer")]
        public ActionResult Order()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            var adress = context.Adresses.Find(user.AdressId);
            var delAdress = new DeliveryAdress
            {
                StreetAdress = adress.StreetAdress,
                PostCode = adress.PostCode,
                Suburb = adress.Suburb,
                State = adress.State
            };
            var info = new PackageInfo
            {
                RecieverName = user.Fname + ' ' + user.Lname
            };
            var model = new OrderModel
            {
                DeliveryAdress = delAdress,
                PackageInfo = info
                
            };

            return View(model);
        }

        [Authorize(Roles = "Owner,Admin,Customer")]
        [HttpPost]
        public ActionResult Order(OrderModel model)
        {
            var message = "Something went wrong, sorry";
            if (ModelState.IsValid) { 
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            ApplicationUser user = manager.FindByIdAsync(User.Identity.GetUserId()).Result;
            var pickupAdress = new Adresses
            {
                StreetAdress = model.PickupAdress.StreetAdress,
                PostCode = model.PickupAdress.PostCode,
                Suburb = model.PickupAdress.Suburb,
                State = model.PickupAdress.State,

            };
                var pickup = adressExist(pickupAdress);
                if (pickup == null)
                {
                    context.Adresses.Add(pickupAdress);
                    context.SaveChanges();
                }
                else
                {
                    pickupAdress = pickup;
                }
            var deliveryAdress = new Adresses
            {
                StreetAdress = model.DeliveryAdress.StreetAdress,
                PostCode = model.DeliveryAdress.PostCode,
                Suburb = model.DeliveryAdress.Suburb,
                State = model.DeliveryAdress.State,

            };
                var delivery = adressExist(deliveryAdress);
                if (delivery == null)
                {
                    context.Adresses.Add(deliveryAdress);
                    context.SaveChanges();
                }
                else
                {
                    deliveryAdress = delivery;
                }
            var order = new Orders
            {
                OrderTime = DateTime.Now,
                PickupAdressId = pickupAdress.AdressId,
                OrderPriority = model.PackageInfo.Priority,
                OrderStatus = "Order recieved",
                ReadyForPickupTime = model.PackageInfo.ReadyForPickupTIme,
                PaymentType = model.PackageInfo.PaymentType
            };
                context.Orders.Add(order);
                context.SaveChanges();

                var package = new Packages
                {
                    SenderId = user.Id,
                    RecieverName = model.PackageInfo.RecieverName,
                    Weight = model.PackageInfo.Weight,
                    SpecialInstructions = model.PackageInfo.sInstructions,
                    RecieverAdressId = deliveryAdress.AdressId,
                    OrderId = order.OrderId,
                    Cost = 234.4
          
                };

                context.Packages.Add(package);
                context.SaveChanges();
                message = "Your order has been recieved, thank you!";
            }
            return RedirectToAction("Index","Home", new {message=message});
        }

        //Check if adress is already in database
        public Adresses adressExist(Adresses adress)
        {
            var Adresses = context.Set<Adresses>();
            foreach (var Adress in Adresses)
            {
                if (adress.StreetAdress == Adress.StreetAdress && adress.PostCode == Adress.PostCode)
                {
                    return Adress;
                }  
            }
            return null;
        }
    }
}