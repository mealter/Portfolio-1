using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoWIUTGallery.Models;

namespace DemoWIUTGallery.DAL
{
    public interface IStudentRepositoy
    {
        IList<Student> GetAll();

        Student GetById(int id);

        void Insert(Student std);

        void Edit(Student std);

        int Delete(int id);

        IList<Student> Filter(string firstName, string lastName, int? level, string module, 
            DateTime? birthDate, int page, int pageSize, out int totalCount);
    }
}
