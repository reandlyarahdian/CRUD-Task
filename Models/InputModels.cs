namespace CRUD_Task.Models
{
    public class InputModels
    {
        public int pk_users_id {  get; set; }

        public string name {  get; set; }

        public List<TasksModel>? tasks { get; set; }
    }
}
