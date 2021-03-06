﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GameAndHang.DAL;
using GameAndHang.Models;
using Microsoft.AspNet.Identity;
using static GameAndHang.Controllers.ManageController;
using System.IO;

namespace GameAndHang.Controllers
{
    public class UsersController : Controller
    {

        private GnHContext db = new GnHContext();

        // GET: Users 
        public async Task<ActionResult> Index()
        {
            var users = db.Users.Include(u => u.AspNetUser);
            return View(await users.ToListAsync());
        }

        public async Task<ActionResult> IndexUser(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";
            var userId = User.Identity.GetUserId();
            User findUser = await db.Users.FindAsync(userId);

            List<Event> UpcomingEvents = new List<Event>();

            var userID = findUser.ID;

            var userreviews = (from b in db.Reviews 
                               where b.Host_ID == userID 
                               select b.ReviewString).ToList();

            ViewBag.Reviews = userreviews;

            
            foreach(var hostedEvent in findUser.Events)
                    {
                if (hostedEvent.Date <= DateTime.Now.AddDays(30) && hostedEvent.Date > DateTime.Now.AddDays(-1))
                {
                    UpcomingEvents.Add(hostedEvent);
                }
            }
            
            ViewBag.upcomingEvents = new List<Event>(UpcomingEvents);

            return View(findUser);
        }

        public async Task<ActionResult> HostProfile(User host)
        {
            User FindUsr = await db.Users.FindAsync(host.ID);

            var userID = host.ID;

            var userreviews = (from b in db.Reviews
                               where b.Host_ID == userID
                               select b.ReviewString).ToList();

            ViewBag.Reviews = userreviews;

            return View(FindUsr);
        }

        public string GetUserName(string ID)
        {
            User UserName = db.Users.Find(ID);
            return (UserName.DisplayName);
        }

        // GET: Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.ID = User.Identity.GetUserId();
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,FirstName,LastName,DOB,DisplayName,Bio,ProfilePic")] User user)
        {
            //var userID = User.Identity.GetUserId();
            user.ID = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return RedirectToAction("Index","Users" );
            }
            ViewBag.ID = new SelectList(db.AspNetUsers, "Id", "Email", user.ID);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID = new SelectList(db.AspNetUsers, "Id", "Email", user.ID);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,FirstName,LastName,DOB")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.ID = new SelectList(db.AspNetUsers, "Id", "Email", user.ID);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            User user = await db.Users.FindAsync(id);
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public byte[] ConvertToByte(HttpPostedFileBase image)
        {
            byte[] tempByte = null;
            BinaryReader ImgReader = new BinaryReader(image.InputStream);
            tempByte = ImgReader.ReadBytes((int)image.ContentLength);
            return tempByte;
        }

        public void UploadPhotoToDB(HttpPostedFileBase file)
        {
            String myuserID = User.Identity.GetUserId();
            User CurrentUser = db.Users.Find(myuserID);
            CurrentUser.ProfilePic = ConvertToByte(file);
            db.SaveChanges();
        }

        public ActionResult IndexPhoto()
        {
            return View();
        }
        [HttpPost]
        public ActionResult IndexPhoto(HttpPostedFileBase file)
        {
            UploadPhotoToDB(file);
            return RedirectToAction("IndexUser", "Users"); ;
        }
    }
}
