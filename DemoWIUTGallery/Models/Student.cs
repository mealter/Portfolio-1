using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace DemoWIUTGallery.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Level { get; set; }

        public string Module { get; set; }

        public DateTime? BirthDate { get; set; }

        public byte[] Image { get; set; }

        public IPagedList<Student> Students { get; set; }
    }
}
