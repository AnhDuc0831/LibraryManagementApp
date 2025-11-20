using BussinessObject;
using DataAccsess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public interface AuthorRepository
    {
        Task<List<Author>> GetAllAsync();
    }

    public class AuthorRepositoryImpl : AuthorRepository
    {
        public async Task<List<Author>> GetAllAsync()
        {
            using var db = new DBLibraryManagementContext();
            return await db.Authors.ToListAsync();
        }
    }
}
