using System.ComponentModel.DataAnnotations;

namespace GLONASSsoftTestTask.Infrastructure.Models.BaseEntity
{
    public class IEntity<T> where T : new()
    {
        [Key]
        public T Id { get; set; }
    }
}
