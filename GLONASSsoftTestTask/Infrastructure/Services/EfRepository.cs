using GLONASSsoftTestTask.Infrastructure.Models;
using GLONASSsoftTestTask.Infrastructure.Models.BaseEntity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GLONASSsoftTestTask.Infrastructure.Services
{
    public class EfRepository<T> : IEfRepository<T> where T : IEntity<Guid>
    {
        private readonly ApplicationDbContext _context;

        public EfRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

        public async Task<T> GetById(Guid id)
        {
            var result = await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

            if (result == null)
            {
                //todo: need to add logger
                return null;
            }

            return result;
        }

        public async Task<Guid> Add(T entity)
        {
            var result = await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity.Id;
        }
    }
}
