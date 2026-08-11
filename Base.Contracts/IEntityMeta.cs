namespace Base.Contracts;

public interface IEntityMeta
{
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        
        public DateTime? ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
}