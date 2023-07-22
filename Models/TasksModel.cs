namespace CRUD_Task.Models
{
    public class TasksModel
    {
        public string? task_detail { get; set; }

        public int pk_task_id { get; set; }

        public int fk_users_id { get; set;}
    }
}
