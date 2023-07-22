using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using CRUD_Task.Models;


namespace Task7.Controllers;
[ApiController]
public class UserController : ControllerBase
{
  private readonly IConfiguration _connection;
  public UserController(IConfiguration connection) 
  {
    _connection = connection;
  }

  [HttpGet]
  [Route("api/tasks/GetUserWithTask")]
  public List<OutputModels> GetUsers(string? name)
  {

    List<OutputModels> users = new List<OutputModels>();

    using (SqlConnection connection = new SqlConnection(_connection.GetConnectionString("DefaultConnection")))
    {
      connection.Open();
      string queryUsers = "select * from users";
      if (!String.IsNullOrEmpty(name)) {
        queryUsers = $"select * from users where name like '%{name}%'";
      }
      SqlCommand cmdUsers = new SqlCommand(queryUsers, connection);

      SqlDataAdapter adapterUsers = new SqlDataAdapter(cmdUsers);
      DataTable dtUsers = new DataTable();
      adapterUsers.Fill(dtUsers);

      for (int i=0; i<dtUsers.Rows.Count; i++)
      {
                OutputModels userModel = new OutputModels();
        
        int user_id = Convert.ToInt32(dtUsers.Rows[i]["pk_users_id"].ToString());

        userModel.pk_users_id = user_id;
        userModel.name = dtUsers.Rows[i]["name"].ToString();

        string getTasksUser = $"select * from tasks where fk_users_id = {user_id}";
        SqlCommand cmdTaskUser = new SqlCommand(getTasksUser, connection);
        SqlDataAdapter adapterTaskUser = new SqlDataAdapter(cmdTaskUser);
        DataTable dtTaskUsers = new DataTable();
        adapterTaskUser.Fill(dtTaskUsers);
        
        List<TasksModel> listTaskUser = new List<TasksModel>();

        for (int j=0; j<dtTaskUsers.Rows.Count; j++)
        {
                    TasksModel taskModel = new TasksModel();
          taskModel.pk_task_id = Convert.ToInt32(dtTaskUsers.Rows[j]["pk_tasks_id"].ToString());
          taskModel.task_detail = dtTaskUsers.Rows[j]["task_detail"].ToString();
          listTaskUser.Add(taskModel);
        }
        userModel.tasks = listTaskUser;
        users.Add(userModel);
      }

      connection.Close();

      return users;
    }
  }

  [HttpPost]
  [Route("api/tasks/AddUserWithTask")]
  public IActionResult AddUser(InputModels user) 
  {
    using(SqlConnection connection = new SqlConnection(_connection.GetConnectionString("DefaultConnection")))
    {
      if (user == null) {
        return BadRequest();
      }

      connection.Open();
      SqlCommand cmdAddUser = new SqlCommand("Insert into users (name) values(@name)", connection);
      cmdAddUser.Parameters.AddWithValue("@name", user.name);
      

      int rowsAffected = cmdAddUser.ExecuteNonQuery();
      if (rowsAffected > 0) {
        string query = "select IDENT_CURRENT('users') AS pk_users_id from users";
        SqlCommand cmdLastUserID = new SqlCommand(query, connection);
        var lastID = 0;
        using(var reader = cmdLastUserID.ExecuteReader())
        {
          if (reader.Read())
          {
            lastID = Convert.ToInt32(reader["pk_users_id"].ToString());
          }
        }

        if (lastID > 0) {
          var tasks = user.tasks;
          foreach(var taskItem in tasks) {
            string queryInsertTasks = "insert into tasks (task_detail, fk_users_id) values(@task_detail, @fk_users_id)";
            SqlCommand cmdInsertTasks = new SqlCommand(queryInsertTasks, connection);
            cmdInsertTasks.Parameters.AddWithValue("@task_detail", taskItem.task_detail);
            cmdInsertTasks.Parameters.AddWithValue("@fk_users_id", lastID);

            cmdInsertTasks.ExecuteNonQuery();
          }
        }

        
        object response = new {message = "Add Users Successfully", code = 200};
        return new JsonResult(response) {StatusCode = 200};
      } else {
        object response = new {message = "Failed to Add Users", code = 500};
        return new JsonResult(response) {StatusCode = 500};
      }
    }
  }
}
