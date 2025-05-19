namespace UserManagement.API.Infrastructure.Data.Models
{
    public abstract  class BaseEntity<T>
    {
        public T Id { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
