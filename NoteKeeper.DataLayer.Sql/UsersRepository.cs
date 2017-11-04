using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;
using NoteKeeper.DataLayer.Exceptions;
using System.Data.SqlClient;

namespace NoteKeeper.DataLayer.Sql
{
    public class UsersRepository : IUsersRepository
    {
        private readonly String _connectionString;
        public UsersRepository(String connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task ChangeNameAsync(Guid id, string newName)
        {
            //Проверка нарушений целостности
            if (newName.Length > 255)
            {
                throw new ChangeException<string>("Имя слишком длинное")
                {
                    Id = id,
                    TypeName = typeof(User).ToString(),
                    FieldName = "Name",
                    NewValue = newName
                };
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "update Users " +
                        "set name = @newName " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@newName", newName);
                    command.Parameters.AddWithValue("@Id", id);

                    await command.ExecuteNonQueryAsync();

                }
            }
        }

        public async Task<User> CreateAsync(User user)
        {
            //Проверка нарушений целостности
            if (user.Name.Length > 255)
            {
                throw new CreateException<User>("Имя слишком длинное")
                {
                    Item = user
                };
            }
            //Проверка нарушений целостности
            if (user.Email.Length > 255)
            {
                throw new CreateException<User>("Email слишком длинный")
                {
                    Item = user
                };
            }
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                //Проверка нарушений целостности
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Users " +
                        "where email = @Email";
                    command.Parameters.AddWithValue("@Email", user.Email);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        
                        if (reader.HasRows)
                        {
                            throw new CreateException<User>("Email уже существует")
                            {
                                Item = user
                            };
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into Users values(@Id, @Name, @Email, 1234);";

                    user.Id = Guid.NewGuid();

                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@Name", user.Name);
                    command.Parameters.AddWithValue("@Email", user.Email);

                    await command.ExecuteNonQueryAsync();

                    return user;
                }
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from Users where id = @Id;";
                    command.Parameters.AddWithValue("@Id", id);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<User> GetAsync(Guid id)
        {
            User result = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Users " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", id);
                    var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
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

        public async Task<User> GetAsync(string email)
        {
            User result = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Users " +
                        "where email = @Email;";
                    command.Parameters.AddWithValue("@Email", email);
                    var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
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

        public async Task<IEnumerable<User>> GetPartnersByNoteAsync(Guid noteId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var partners = new List<User>();
                using (var getPartnersCommend = connection.CreateCommand())
                {
                    getPartnersCommend.CommandText =
                        "select * from Users " +
                        "join SharedNotes on note_id = @id and user_id = id";
                    getPartnersCommend.Parameters.AddWithValue("@id", noteId);

                    var reader = await getPartnersCommend.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        partners.Add(new User()
                        {
                            Id = new Guid(reader["Id"].ToString()),
                            Email = reader["email"].ToString(),
                            Name = reader["name"].ToString()
                        });
                    }

                    reader.Close();

                    return partners;
                }
            }
        }
    }
}
