using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;
using NoteKeeper.DataLayer;
using System.Data.SqlClient;
using DataLayer;

namespace NoteKeeper.DataLayer.Sql
{
    public class UsersRepository : IUsersRepository
    {
        private static String _connectionString;
        public UsersRepository(String connectionString)
        {
            _connectionString = connectionString;
        }
        public User ChangeName(User user, string newName)
        {
            throw new NotImplementedException();
        }

        public User Create(User user)
        {
            if (user.Name == null || user.Email == null)
            {
                throw new ArgumentException("Fields Name, Email shouldn't be null");
            }
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using(var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into Users values(@Id, @Name, @Email, 1234);";

                    user.Id = Guid.NewGuid();

                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Email", user.Email);

                    command.ExecuteNonQuery();

                    return user;
                }
            }
        }

        public void Delete(User user)
        {
            if (user.Id == null)
            {
                throw new ArgumentException("Fields Id shouldn't be null");
            }
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from Users where id = @Id;";
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public User Get(Guid id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("Id shouldn't be null");
            }
            User result = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Users " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", id);
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        result = new User()
                        {
                            Id = id,
                            Name = reader["name"].ToString(),
                            Email = reader["email"].ToString()
                        };
                    }
                    return result;
                }
            }
        }

        public User Get(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("Email shouldn't be null");
            }
            User result = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Users " +
                        "where email = @Email;";
                    command.Parameters.AddWithValue("@Email", email);
                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        result = new User()
                        {
                            Id = new Guid(reader["id"].ToString()),
                            Name = reader["name"].ToString(),
                            Email = email
                        };
                    }
                    return result;
                }
            }
        }
    }
}
