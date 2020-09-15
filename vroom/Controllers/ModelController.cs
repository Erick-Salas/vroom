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
using vroom.Controllers.Resources;
using AutoMapper;

namespace vroom.Controllers
{
    [Authorize(Roles = Roles.Admin+","+Roles.Executive)]
    public class ModelController : Controller
    {
        private readonly VroomDbContext _db;
        private readonly IMapper _mapper;

        [BindProperty]
        public ModelViewModel ModelVM { get; set; }

        public ModelController(VroomDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
            ModelVM = new ModelViewModel()
            {
                Makes = _db.Makes.ToList(),
                Model = new Models.Model()
            };
        }

        public IActionResult Index()
        {
            var model = _db.Models.Include(m=>m.Make);
            return View(model);
        }

        public IActionResult Create()
        {
            
            return View(ModelVM);
        }

        [HttpPost, ActionName("Create")]
        public IActionResult CreatePost()
        {

            if (!ModelState.IsValid)
            {
                return View(ModelVM);
            }

            _db.Models.Add(ModelVM.Model);
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Edit(int id)
        {

            ModelVM.Model = _db.Models.Include(m => m.Make).SingleOrDefault(m => m.Id == id);

            if (ModelVM.Model==null)
            {
                return NotFound();
            }

            return View(ModelVM);
        }

        [HttpPost, ActionName("Edit")]
        public IActionResult EditPost()
        {

            if (!ModelState.IsValid)
            {
                return View(ModelVM);
            }

            _db.Update(ModelVM.Model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            
            Model model = _db.Models.Find(id);
           List<Bike> model1 = _db.Bikes.Where(m=>m.ModelID==id).ToList();

            if (model==null)
            {
                return NotFound();
            }
            _db.Bikes.RemoveRange(model1);
            _db.Models.Remove(model);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));

        }

        [AllowAnonymous]
        [HttpGet("api/models")]
        public IEnumerable<ModelResources> Models()
        {
            //return _db.Models.ToList();
            var models = _db.Models.ToList();

            //create mapper configuration
            //var config = new MapperConfiguration(mc => mc.CreateMap<Model, ModelResources>());

            //map the objects
            //var mapper = new Mapper(config);
            return _mapper.Map<List<Model>, List<ModelResources>>(models);

            //var modelResources = models
            //    .Select(m => new ModelResources
            //    {
            //        Id = m.Id,
            //        Name = m.Name

            //    }).ToList();

            //return modelResources;
                
        }



    }
}