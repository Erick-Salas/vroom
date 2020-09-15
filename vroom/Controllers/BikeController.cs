using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vroom.AddDbContext;
using vroom.Models;
using vroom.Models.ViewModel;
using vroom.Helpers;
using System.IO;
using Microsoft.AspNetCore.Hosting.Internal;
using cloudscribe.Pagination.Models;

namespace vroom.Controllers
{
    [Authorize(Roles = Roles.Admin + "," + Roles.Executive)]
    public class BikeController : Controller
    {
        private readonly VroomDbContext _db;
        private readonly HostingEnvironment _hostingEnvironment;


        [BindProperty]
        public BikeViewModel BikeVM { get; set; }

        public BikeController(VroomDbContext db, HostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;

            BikeVM = new BikeViewModel()
            {
                Makes = _db.Makes.ToList(),
                Models = _db.Models.ToList(),
                Bike = new Models.Bike()
            };
        }

        
        [AllowAnonymous]
        public IActionResult Index(string searchString, string sortOrder, int pageNumber=1, int pageSize=25)
        {

            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.PriceSortParam = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            int ExcludeRecords = (pageSize * pageNumber) - pageSize;
            var Bikes = from b in _db.Bikes.Include(m => m.Make).Include(m => m.Model)
                        select b;

            var BikeCount = Bikes.Count();

            if (!String.IsNullOrEmpty(searchString))
            {
                Bikes = Bikes.Where(b => b.Make.Name.Contains(searchString));
                BikeCount = Bikes.Count();

            }

            //Sorting Logic
            switch (sortOrder)
            {
                case "price_desc":
                    Bikes = Bikes.OrderByDescending(b => b.Price);
                    break;
                default:
                    Bikes = Bikes.OrderBy(b => b.Price);
                    break;
            }

            Bikes=Bikes.Skip(ExcludeRecords)
                .Take(pageSize);

            var result = new PagedResult<Bike>
            {
                Data = Bikes.AsNoTracking().ToList(),
                TotalItems = BikeCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return View(result);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            BikeVM.Bike = _db.Bikes.SingleOrDefault(b=>b.Id==id);

            //Filter the models associated to the selected make
            BikeVM.Models = _db.Models.Where(m => m.MakeFK == BikeVM.Bike.MakeID);

            if (BikeVM.Bike==null)
            {
                return NotFound();
            }

            return View(BikeVM);
        }

        [HttpPost, ActionName("Edit")]

        public IActionResult EditPost()
        {

            if (!ModelState.IsValid)
            {
                BikeVM.Makes = _db.Makes.ToList();
                BikeVM.Models = _db.Models.ToList();
                return View(BikeVM);
            }

            _db.Bikes.Update(BikeVM.Bike);

            UploadImageIfAvailable();

            _db.SaveChanges();

           
            /////////////////


            return RedirectToAction(nameof(Index));
        }

        //get method
        public IActionResult Create()
        {

            return View(BikeVM);
        }

        //post method
        [HttpPost, ActionName("Create")]
        
        public IActionResult CreatePost()
        {

            if (!ModelState.IsValid)
            {
                BikeVM.Makes = _db.Makes.ToList();
                BikeVM.Models = _db.Models.ToList();
                return View(BikeVM);
            }

            _db.Bikes.Add(BikeVM.Bike);

            UploadImageIfAvailable();
            
                _db.SaveChanges();



            

            /////////////////


            return RedirectToAction(nameof(Index));
        }

        private void UploadImageIfAvailable()
        {
            //Save bike logic
            /////////////////
            ///

            //get bike id we have saved in db

            var BikeID = BikeVM.Bike.Id;

            //get wwwwrootpath to save the file on server
            string wwrootPath = _hostingEnvironment.WebRootPath;

            //get the uploaded files
            var files = HttpContext.Request.Form.Files;

            //get the references if DBSet for the bike we just have saved in database
            var SavedBike = _db.Bikes.Find(BikeID);

            //Upload the files on server and save the image path of user have uploaded any file
            if (files.Count != 0)
            {
                var ImagePath = @"images\bike\";
                var Extension = Path.GetExtension(files[0].FileName);
                var RelativeImagePath = ImagePath + BikeID + Extension;
                var AbsImagePath = Path.Combine(wwrootPath, RelativeImagePath);

                //Upload the file on server
                using (var fileStream = new FileStream(AbsImagePath, FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                //set the image path on database
                SavedBike.ImagePath = RelativeImagePath;
            }
        }

        //public IActionResult Edit(int id)
        //{

        //    ModelVM.Model = _db.Models.Include(m => m.Make).SingleOrDefault(m => m.Id == id);

        //    if (ModelVM.Model == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(ModelVM);
        //}

        //[HttpPost, ActionName("Edit")]
        //public IActionResult EditPost(int id)
        //{

        //    if (!ModelState.IsValid)
        //    {
        //        return View(ModelVM);
        //    }

        //    _db.Update(ModelVM.Model);
        //    _db.SaveChanges();
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpPost]
        public IActionResult Delete(int id)
        {

            Bike bike = _db.Bikes.Find(id);

            if (bike == null)
            {
                return NotFound();
            }

            _db.Bikes.Remove(bike);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        [AllowAnonymous]
        [HttpGet("api/models/{MakeID}")]
        public IEnumerable<Model> Models(int MakeID)
        {
            return _db.Models.ToList()
                .Where(m => m.MakeFK == MakeID);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult View(int id)
        {
            BikeVM.Bike = _db.Bikes.SingleOrDefault(b => b.Id == id);

            

            if (BikeVM.Bike == null)
            {
                return NotFound();
            }

            return View(BikeVM);
        }
    }
}