using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using DemoWIUTGallery.Models;

namespace DemoWIUTGallery.DAL
{
    public class StudentRepository : IStudentRepositoy
    {

        private string ConnStr;
        public StudentRepository(string connStr)
        {
            ConnStr = connStr;
        }

        public int Delete(int id)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"[dbo].[udpDelStudent]";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("id", id);

                    var pErr = cmd.CreateParameter();
                    pErr.ParameterName = "err";
                    pErr.Direction = ParameterDirection.Output;
                    pErr.SqlDbType = SqlDbType.VarChar;
                    pErr.Size = 1000;
                    cmd.Parameters.Add(pErr);

                    var pRet = cmd.CreateParameter();
                    pRet.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(pRet);


                    conn.Open();
                    cmd.ExecuteNonQuery();

                    int retVal = (int)pRet.Value;
                    string err = (string)pErr.Value;

                    if(retVal < 0)
                    {
                        throw new Exception(err);
                    }
                    else
                    {
                        return retVal;
                    }


                }
            }
        }

        public void Edit(Student std)
        {
            using(var conn = new SqlConnection(ConnStr))
            {
                using (var cmd =conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE [dbo].[Student]
                                           SET  [FirstName] = @FirstName,
					                            [LastName] = @LastName,
					                            [Level] = @Level,
					                            [Module] = @Module,
					                            [BirthDate] = @BirthDate
                                                WHERE [StudentId] = @StudentId";
                    var pStudent = cmd.CreateParameter();
                    pStudent.ParameterName = "@FirstName";
                    pStudent.Value = std.FirstName;
                    cmd.Parameters.Add(pStudent);

                    cmd.Parameters.AddWithValue("@LastName", std.LastName);
                    cmd.Parameters.AddWithValue("@Level", std.Level);
                    cmd.Parameters.AddWithValue("@Module", std.Module);

                    cmd.Parameters.AddWithValue("@BirthDate", (object)std.BirthDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StudentId", std.StudentId);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
            }
        }

        public IList<Student> Filter(string firstName, string lastName, int? level, string module, 
            DateTime? birthDate, int page, int pageSize, out int totalCount)
        {
            IList<Student> students = new List<Student>();
            using (var conn = new SqlConnection(ConnStr))
            {
                using(var cmd = conn.CreateCommand())
                {
                    string fields = @" [StudentId],
                                                [FirstName],
                                                [LastName],
                                                [Level],
                                                [Module],
                                                [BirthDate],
                                                [Image]";
                    string sql = @"SELECT 
                                        {0}
                                        FROM [dbo].[Student] ";

                    string whereSql = "";
                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
                        whereSql += (whereSql.Length == 0 ? "" : " AND ") + " FirstName like @FirstName + '%' ";
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                    }
                    if (!string.IsNullOrWhiteSpace(lastName))
                    {
                        whereSql += (whereSql.Length == 0 ? "" : " AND ") + " LastName like @LastName + '%' ";
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                    }
                    if (level.HasValue)
                    {
                        whereSql += (whereSql.Length == 0 ? "" : " AND ") + " Level = @Level ";
                        cmd.Parameters.AddWithValue("@Level", level);
                    }
                    if (!string.IsNullOrWhiteSpace(module))
                    {
                        whereSql += (whereSql.Length == 0 ? "" : " AND ") + " Module like @Module + '%' ";
                        cmd.Parameters.AddWithValue("@Module", module);
                    }
                    if (birthDate.HasValue)
                    {
                        whereSql += (whereSql.Length == 0 ? "" : " AND ") + " BirthDate <= @BirthDate ";
                        cmd.Parameters.AddWithValue("@BirthDate", birthDate);
                    }

                    if (!string.IsNullOrWhiteSpace(whereSql))
                    {
                        whereSql = " WHERE " + whereSql;
                    }

                    conn.Open();
                    cmd.CommandText =string.Format(sql, " count(*) ") + whereSql;
                    totalCount = (int)cmd.ExecuteScalar();

                    string pageSql = " OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY ";
                    cmd.Parameters.AddWithValue("@offset", (page-1)*pageSize);
                    cmd.Parameters.AddWithValue("@pageSize", pageSize);

                    cmd.CommandText = string.Format(sql, fields) + whereSql + " ORDER BY StudentId " +pageSql;
                    

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var student = new Student();
                            student.StudentId = rdr.GetInt32(rdr.GetOrdinal("StudentId"));
                            student.FirstName = rdr.GetString(rdr.GetOrdinal("FirstName"));
                            student.LastName = rdr.GetString(rdr.GetOrdinal("LastName"));
                            student.Level = rdr.GetInt32(rdr.GetOrdinal("Level"));
                            student.Module = rdr.GetString(rdr.GetOrdinal("Module"));
                            if (!rdr.IsDBNull(rdr.GetOrdinal("BirthDate")))
                                student.BirthDate = rdr.GetDateTime(rdr.GetOrdinal("BirthDate"));

                            students.Add(student);
                        }
                    }
                }
            }
            return students;
        }


        public IList<Student> GetAll()
        {
            var result = new List<Student>();
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"SELECT * FROM [dbo].[Student]";

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var student = new Student();
                            student.StudentId = rdr.GetInt32(rdr.GetOrdinal("StudentId"));
                            student.FirstName = rdr.GetString(rdr.GetOrdinal("FirstName"));
                            student.LastName = rdr.GetString(rdr.GetOrdinal("LastName"));
                            student.Level = rdr.GetInt32(rdr.GetOrdinal("Level"));
                            student.Module = rdr.GetString(rdr.GetOrdinal("Module"));
                            if (!rdr.IsDBNull(rdr.GetOrdinal("BirthDate")))
                                student.BirthDate = rdr.GetDateTime(rdr.GetOrdinal("BirthDate"));

                            result.Add(student);
                        }
                    }
                }
                return result;
            }
        }

        public Student GetById(int id)
        {
            Student student = null;
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT  [StudentId],
                                                [FirstName],
                                                [LastName],
                                                [Level],
                                                [Module],
                                                [BirthDate],
                                                [Image]
                                        FROM [dbo].[Student] 
                                        WHERE  [StudentId]= @StudentId";
                    cmd.Parameters.AddWithValue("@StudentId", id);

                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            student = new Student()
                            {
                                StudentId = id,
                                FirstName = rdr.GetString(rdr.GetOrdinal("FirstName")),
                                LastName = rdr.GetString(rdr.GetOrdinal("LastName")),
                                Level = rdr.GetInt32(rdr.GetOrdinal("Level")),
                                Module = rdr.GetString(rdr.GetOrdinal("Module")),
                                BirthDate = !rdr.IsDBNull(rdr.GetOrdinal("BirthDate")) 
                                    ? rdr.GetDateTime(rdr.GetOrdinal("BirthDate")) : (DateTime?)null,
                                Image = !rdr.IsDBNull(rdr.GetOrdinal("Image"))
                                    ? (byte[])rdr["Image"] : null
                            };                        }
                    }
                }
            }
            return student;
        }

        public void Insert(Student std)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO [dbo].[Student](
					                        [FirstName],
					                        [LastName],
					                        [Level],
					                        [Module],
					                        [BirthDate],
                                            [Image])
                                    VALUES(	@FirstName,
		                                    @LastName,
		                                    @Level,
		                                    @Module,
		                                    @BirthDate,
                                            @Image)";

                    var pStudent = cmd.CreateParameter();
                    pStudent.ParameterName = "@FirstName";
                    pStudent.Value = std.FirstName;
                    cmd.Parameters.Add(pStudent);

                    cmd.Parameters.AddWithValue("@LastName", std.LastName);
                    cmd.Parameters.AddWithValue("@Level", std.Level);
                    cmd.Parameters.AddWithValue("@Module", std.Module);

                    cmd.Parameters.AddWithValue("@BirthDate", (object)std.BirthDate ?? DBNull.Value );
                    cmd.Parameters.AddWithValue("@Image", std.Image ?? SqlBinary.Null );

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
            }
        }
    }
}
