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

        public void ChangeName(Guid tagId, string newName)
        {
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
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "update Tags " +
                        "set name = @newName " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@newName", newName);
                    command.Parameters.AddWithValue("@Id", tagId);

                    command.ExecuteNonQuery();

                }
            }
        }

        public Tag Create(Tag tag)
        {
            if (tag.Name.Length > 255)
            {
                throw new CreateException<Tag>("Слишком длинное имя")
                {
                    Item = tag
                };
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into Tags values(@Id, @OwnerId, @Name);";

                    tag.Id = Guid.NewGuid();

                    command.Parameters.AddWithValue("@Id", tag.Id);
                    command.Parameters.AddWithValue("@OwnerId", tag.OwnerId);
                    command.Parameters.AddWithValue("@Name", tag.Name);

                    command.ExecuteNonQuery();

                    return tag;
                }
            }
        }

        public void Delete(Guid tagId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from Tags " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", tagId);

                    command.ExecuteNonQuery();

                }
            }
        }

        public Tag Get(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                Tag resultTag = null;

                using (var getNoteCommand = connection.CreateCommand())
                {
                    getNoteCommand.CommandText = "select * from Tags where id = @id;";
                    getNoteCommand.Parameters.AddWithValue("@id", id);

                    var reader = getNoteCommand.ExecuteReader();
                    if (!reader.Read())
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

        public IEnumerable<Tag> GetByNote(Guid noteId)
        {
            if (noteId == null)
            {
                throw new ArgumentException("noteId shouldn't be null");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                List<Tag> result = null;

                using (var getNoteCommand = connection.CreateCommand())
                {
                    getNoteCommand.CommandText = "select * from Tags " +
                        "join NoteTags on note_id = @noteId and id = tag_id;";
                    getNoteCommand.Parameters.AddWithValue("@noteId", noteId);

                    var reader = getNoteCommand.ExecuteReader();
                    result = new List<Tag>();
                    while (reader.Read())
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

        public IEnumerable<Tag> GetByOwner(Guid ownerId)
        {
            if (ownerId == null)
            {
                throw new ArgumentException("ownerId shouldn't be null");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                List<Tag> result = null;

                using (var getNoteCommand = connection.CreateCommand())
                {
                    getNoteCommand.CommandText = "select * from Tags " +
                        "where user_id = @Id;";
                    getNoteCommand.Parameters.AddWithValue("@Id", ownerId);

                    var reader = getNoteCommand.ExecuteReader();
                    result = new List<Tag>();
                    while (reader.Read())
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
