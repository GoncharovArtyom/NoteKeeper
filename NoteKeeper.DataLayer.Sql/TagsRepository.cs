using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.DataLayer;
using NoteKeeper.Model;
using System.Data.SqlClient;

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
            if (tag.OwnerId == null || tag.Name == null)
            {
                throw new ArgumentException("Fields OwnerId, Heading, Text, CreationDate and LastUpdateDate shouldn't be null");
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
            if (tagId == null)
            {
                throw new ArgumentException("Field Id shouldn't be null");
            }

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
