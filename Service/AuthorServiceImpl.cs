using BussinessObject;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface AuthorService
    {
        Task<List<Author>> GetAuthorsAsync();
    }

    public class AuthorServiceImpl : AuthorService
    {
        private readonly AuthorRepository _repo;

        public AuthorServiceImpl()
        {
            _repo = new AuthorRepositoryImpl();
        }

        public Task<List<Author>> GetAuthorsAsync() => _repo.GetAllAsync();
    }
}
