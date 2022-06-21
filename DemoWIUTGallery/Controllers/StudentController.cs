using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DemoWIUTGallery.DAL;
using DemoWIUTGallery.Models;
using System.IO;
//using PagedList;
using X.PagedList;

namespace DemoWIUTGallery.Controllers
{
    public class StudentController : Controller
    {

        private IStudentRepositoy _repository;

        public StudentController(IStudentRepositoy repository)
        {
            _repository = repository;
        }

        // GET: StudentController
        public ActionResult Index(string firstName, string lastName, int? level, string module, 
            DateTime? birthDate, int? page)
        {
            int pageNumber = page ?? 1 ;
            int totalCount;
            int pageSize = 3;
            var students = _repository.Filter(firstName,lastName,level,module,birthDate, pageNumber, pageSize, out totalCount);
            var pagedList = new StaticPagedList<Student>(students, pageNumber, pageSize, totalCount);
            //ViewBag.
            return View(pagedList);
        }

        // GET: StudentController/Details/5
        public ActionResult Details(int id)
        {
            var std = _repository.GetById(id);
            return View(std);
        }

        // GET: StudentController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StudentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Student student, IFormFile? imageFile)
        {

            
            try
            {
                if(imageFile?.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        imageFile.CopyTo(stream);
                        student.Image = stream.ToArray();
                    }
                        
                }
                _repository.Insert(student);

                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: StudentController/Edit/5
        public ActionResult Edit(int id)
        {
            var std = _repository.GetById(id);
            return View(std);
        }

        // POST: StudentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Student student)
        {
            try
            {
               _repository.Edit(student);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        // GET: StudentController/Delete/5
        public ActionResult Delete(int id)
        {
            var std = _repository.GetById(id);
            return View(std);
        }

        // POST: StudentController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                _repository.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        public FileResult DisplayImage(int id)
        {
            var student = _repository.GetById(id);
            if(student != null && student.Image?.Length > 0)
            {
                return File(student.Image, "image/jpeg", student.LastName + ".jpg");
            }
            else
            {
                return null;
            }
        }
    }
}
