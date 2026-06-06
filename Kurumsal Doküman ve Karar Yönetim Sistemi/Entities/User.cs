namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true; // Soft Delete için
        
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        
        public List<TaskItem>? Tasks { get; set; }

    }
}
