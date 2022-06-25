using GLONASSsoftTestTask.Infrastructure.Models.BaseEntity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GLONASSsoftTestTask.Infrastructure.Services
{
    public interface IEfRepository<T> where T : IEntity<Guid>
    {
        List<T> GetAll();
        Task<T> GetById(Guid id);
        Task<Guid> Add(T entity);
    }
}
