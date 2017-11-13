using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.DataLayer;
using NoteKeeper.Model;
using System.Data.SqlClient;
using NoteKeeper.DataLayer.Exceptions;

namespace NoteKeeper.DataLayer.Sql
{
    public class TagsRepository : ITagsRepository
    {
        private readonly String _connectionString;

        public TagsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ChangeNameAsync(Guid tagId, string newName)
        {
            //Проверка целостности
            if (newName.Length > 255)
            {
                throw new ChangeException<string>("Слишком длинное имя")
                {
                    Id = tagId,
                    TypeName = typeof(Tag).ToString(),
                    FieldName = "Name",
                    NewValue = newName
                };
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Tags " +
                        "where name = @Name and user_id in " +
                            "(select distinct user_id from Tags " +
                            "where id = @Id);";
                    command.Parameters.AddWithValue("@Id", tagId);
                    command.Parameters.AddWithValue("@Name", newName);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            throw new ChangeException<string>("Такой тег уже существует")
                            {
                                TypeName = typeof(Tag).ToString(),
                                FieldName = "Name",
                                NewValue = newName
                            };
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "update Tags " +
                        "set name = @newName " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@newName", newName);
                    command.Parameters.AddWithValue("@Id", tagId);

                    await command.ExecuteNonQueryAsync();

                }
            }
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            //Проверка целостности
            if (tag.Name.Length > 255)
            {
                throw new CreateException<Tag>("Слишком длинное имя")
                {
                    Item = tag
                };
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                //Проверка нарушения целостности
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from users " +
                        "where id = @OwnerID;";
                    command.Parameters.AddWithValue("@OwnerID", tag.OwnerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            throw new CreateException<Tag>("Пользователя, которому принадлежит этот тег, не существует")
                            {
                                Item = tag
                            };
                        }
                    }
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from tags " +
                        "where name = @Name and user_id = @OwnerId;";
                    command.Parameters.AddWithValue("@OwnerId", tag.OwnerId);
                    command.Parameters.AddWithValue("@Name", tag.Name);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            throw new CreateException<Tag>("Такой тег уже существует")
                            {
                                Item = tag
                            };
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into Tags values(@Id, @OwnerId, @Name);";

                    tag.Id = Guid.NewGuid();

                    command.Parameters.AddWithValue("@Id", tag.Id);
                    command.Parameters.AddWithValue("@OwnerId", tag.OwnerId);
                    command.Parameters.AddWithValue("@Name", tag.Name);

                    await command.ExecuteNonQueryAsync();

                    return tag;
                }
            }
        }

        public async Task DeleteAsync(Guid tagId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from Tags " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", tagId);

                    await command.ExecuteNonQueryAsync();

                }
            }
        }

        public async Task<Tag> GetAsync(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Tag resultTag = null;

                using (var getNoteCommand = connection.CreateCommand())
                {
                    getNoteCommand.CommandText = "select * from Tags where id = @id;";
                    getNoteCommand.Parameters.AddWithValue("@id", id);

                    var reader = await getNoteCommand.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        return resultTag;
                    }

                    resultTag = new Tag()
                    {
                        OwnerId = new Guid(reader["user_id"].ToString()),
                        Id = id,
                        Name = reader["Name"].ToString()
                    };

                    reader.Close();
                }
                return resultTag;
            }
        }

        public async Task<IEnumerable<Tag>> GetByNoteAsync(Guid noteId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                List<Tag> result = null;

                using (var getNoteCommand = connection.CreateCommand())
                {
                    getNoteCommand.CommandText = "select * from Tags " +
                        "join NoteTags on note_id = @noteId and id = tag_id;";
                    getNoteCommand.Parameters.AddWithValue("@noteId", noteId);

                    var reader = await getNoteCommand.ExecuteReaderAsync();
                    result = new List<Tag>();
                    while (await reader.ReadAsync())
                    {
                        var currentTag = new Tag()
                        {
                            OwnerId = new Guid(reader["user_id"].ToString()),
                            Id = new Guid(reader["id"].ToString()),
                            Name = reader["Name"].ToString()
                        };

                        result.Add(currentTag);
                    }

                    reader.Close();
                }
                return result;
            }
        }

        public async Task<IEnumerable<Tag>> GetByOwnerAsync(Guid ownerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                List<Tag> result = null;

                using (var getNoteCommand = connection.CreateCommand())
                {
                    getNoteCommand.CommandText = "select * from Tags " +
                        "where user_id = @Id;";
                    getNoteCommand.Parameters.AddWithValue("@Id", ownerId);

                    var reader = await getNoteCommand.ExecuteReaderAsync();
                    result = new List<Tag>();
                    while (await reader.ReadAsync())
                    {
                        var currentTag = new Tag()
                        {
                            OwnerId = new Guid(reader["user_id"].ToString()),
                            Id = new Guid(reader["id"].ToString()),
                            Name = reader["Name"].ToString()
                        };

                        result.Add(currentTag);
                    }

                    reader.Close();
                }
                return result;
            }
        }
    }
}
