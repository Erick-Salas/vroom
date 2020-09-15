using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vroom.AddDbContext;
using vroom.Models;
using vroom.Helpers;
namespace vroom.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Executive)]
    public class MakeController : Controller
    {
        //[Route("Make")]
        //[Route("Make/Bikes")]
        //public IActionResult Bikes()
        //{
        //   Make make = new Make {Id=1, Name = "Harley Davidson" };

        //    return View(make);

        //    //ContentResult cr = new ContentResult { Content = "Hello World" };

        //    //return cr;

        //    //return Content("Hey this is the string from helper method");
        //    //return Redirect("/home");
        //    //return RedirectToAction("About", "Home");

        //    //return new EmptyResult();//no retorna nada al view

        //}

        //[Route("make/bikes/{year:int:length(4)}/{month:int:range(1,12)}")]
        //public IActionResult ByYearMoth(int year, int month)
        //{
        //    return Content(year+";"+month);
        //}

        private readonly VroomDbContext _db;

        public MakeController(VroomDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_db.Makes.ToList());
        }
        //Get method
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Make make)
        {

            if (ModelState.IsValid)
            {
                _db.Add(make);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(make);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {

            var make = _db.Makes.Find(id);
            if (make==null)
            {
                return NotFound();
            }
            _db.Makes.Remove(make);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {

            var make = _db.Makes.Find(id);
            if (make == null)
            {
                return NotFound();
            }
            
            return View(make);
        }
        [HttpPost]
        public IActionResult Edit(Make make)
        {

            if (ModelState.IsValid)
            {
                _db.Update(make);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(make);
        }

    }
}