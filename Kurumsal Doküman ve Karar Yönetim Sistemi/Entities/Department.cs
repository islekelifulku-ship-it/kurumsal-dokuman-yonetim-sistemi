using System.Collections.Generic;

namespace Kurumsal_Doküman_ve_Karar_Yönetim_Sistemi.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<User>? Users { get; set; }
    }
}
