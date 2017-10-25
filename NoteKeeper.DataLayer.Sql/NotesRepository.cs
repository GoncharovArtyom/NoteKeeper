using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;
using System.Data.SqlClient;
using NoteKeeper.DataLayer;

namespace NoteKeeper.DataLayer.Sql
{
    public class NotesRepository : INotesRepository
    {
        private readonly String _connectionString;
        public NotesRepository(String connectionString)
        {
            _connectionString = connectionString;
        }
        public Note ChangeHeading(Guid noteId, string newHeading)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "update Notes " +
                        "set heading = @newHeading, last_update_date = @now " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@newHeading", newHeading);
                    command.Parameters.AddWithValue("@Id", noteId);
                    command.Parameters.AddWithValue("@now", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }

            return Get(noteId);
        }

        public Note ChangeText(Guid noteId, string newText)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "update Notes " +
                        "set text = @newText, last_update_date = @now " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@newText", newText);
                    command.Parameters.AddWithValue("@Id", noteId);
                    command.Parameters.AddWithValue("@now", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
            return Get(noteId);
        }

        public Note Create(Note newNote)
        {
            if (newNote.OwnerId == null || newNote.Heading == null || newNote.Text == null)
            {
                throw new ArgumentException("Fields Owner, Heading, Text, CreationDate and LastUpdateDate shouldn't be null");
            }

            newNote.CreationDate = DateTime.Now;
            newNote.LastUpdateDate = DateTime.Now;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into Notes values(@Id, @OwnerId, @Heading, @Text, @CreationDate, @LastUpdateDate);";

                    newNote.Id = Guid.NewGuid();

                    command.Parameters.AddWithValue("@Id", newNote.Id);
                    command.Parameters.AddWithValue("@OwnerId", newNote.OwnerId);
                    command.Parameters.AddWithValue("@Heading", newNote.Heading);
                    command.Parameters.AddWithValue("@Text", newNote.Text);
                    command.Parameters.AddWithValue("@CreationDate", newNote.CreationDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@LastUpdateDate", newNote.LastUpdateDate.ToString("yyyy-MM-dd"));

                    command.ExecuteNonQuery();

                    return newNote;
                }
            }
        }

        public void Delete(Guid noteId)
        {
            if (noteId == null)
            {
                throw new ArgumentException("Field Id shouldn't be null");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from Notes " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", noteId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Note Get(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                Note resultNote = null;
                var tagNames = new List<string>();
                using (var getNoteCommand = connection.CreateCommand())
                {
                    getNoteCommand.CommandText =
                        "select * from Notes " +
                        "where id = @id;";
                    getNoteCommand.Parameters.AddWithValue("@id", id);

                    var reader = getNoteCommand.ExecuteReader();
                    if (!reader.Read())
                    {
                        return resultNote;
                    }

                    resultNote = new Note()
                    {
                        OwnerId = new Guid(reader["user_id"].ToString()),
                        Id = id,
                        Heading = reader["heading"].ToString(),
                        Text = reader["text"].ToString(),
                        CreationDate = DateTime.Parse(reader["creation_date"].ToString()),
                        LastUpdateDate = DateTime.Parse(reader["last_update_date"].ToString()),
                        TagNames = tagNames
                    };

                    reader.Close();
                }

                using (var getTagsComand = connection.CreateCommand())
                {
                    getTagsComand.CommandText =
                        "select Tags.name from Tags " +
                        "join NoteTags on NoteTags.note_id = @id";
                    getTagsComand.Parameters.AddWithValue("@id", id);

                    var reader = getTagsComand.ExecuteReader();

                    while (reader.Read())
                    {
                        tagNames.Add(reader["name"].ToString());
                    }

                    reader.Close();
                }

                var partners = new List<User>();
                using (var getPartnersCommend = connection.CreateCommand())
                {
                    getPartnersCommend.CommandText =
                        "select * from Users " +
                        "join SharedNotes on note_id = @id and user_id = id";
                    getPartnersCommend.Parameters.AddWithValue("@id", id);

                    var reader = getPartnersCommend.ExecuteReader();

                    while (reader.Read())
                    {
                        partners.Add(new User()
                        {
                            Id = new Guid(reader["Id"].ToString()),
                            Email = reader["email"].ToString(),
                            Name = reader["name"].ToString()
                        });
                    }

                    reader.Close();
                }
                resultNote.Partners = partners;

                return resultNote;
            }
        }

        public IEnumerable<Note> GetByOwner(Guid ownerId)
        {
            if (ownerId == null)
            {
                throw new ArgumentException("Field Id shouldn't be null");
            }

            List<Note> result = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Notes " +
                        "where user_id = @Id";
                    command.Parameters.AddWithValue("@Id", ownerId);

                    var reader = command.ExecuteReader();
                    result = new List<Note>();

                    while (reader.Read())
                    {
                        var currentNote = new Note()
                        {
                            OwnerId = new Guid(reader["user_id"].ToString()),
                            Id = new Guid(reader["id"].ToString()),
                            Heading = reader["heading"].ToString(),
                            Text = reader["text"].ToString(),
                            CreationDate = DateTime.Parse(reader["creation_date"].ToString()),
                            LastUpdateDate = DateTime.Parse(reader["last_update_date"].ToString())
                        };

                        result.Add(currentNote);
                    }
                    reader.Close();
                }

                foreach (var note in result)
                {
                    using (var tagCommand = connection.CreateCommand())
                    {
                        tagCommand.CommandText =
                            "select name from Tags " +
                            "join NoteTags on note_id = @id and tag_id=id;";
                        tagCommand.Parameters.AddWithValue("@id", note.Id.ToString());
                        var tagReader = tagCommand.ExecuteReader();

                        var tagNames = new List<string>();
                        while (tagReader.Read())
                        {
                            tagNames.Add(tagReader["name"].ToString());
                        }
                        tagReader.Close();
                        note.TagNames = tagNames;
                    }

                    using (var getPartnersCommend = connection.CreateCommand())
                    {
                        getPartnersCommend.CommandText =
                            "select * from Users " +
                            "join SharedNotes on note_id = @id and user_id = id";
                        getPartnersCommend.Parameters.AddWithValue("@id", note.Id);

                        var reader = getPartnersCommend.ExecuteReader();
                        var partners = new List<User>();
                        while (reader.Read())
                        {
                            partners.Add(new User()
                            {
                                Id = new Guid(reader["id"].ToString()),
                                Email = reader["email"].ToString(),
                                Name = reader["name"].ToString()
                            });
                        }

                        reader.Close();
                        note.Partners = partners;
                    }
                }

                return result;

            }
        }

        public IEnumerable<SharedNote> GetByPartner(Guid partnerId)
        {
            if (partnerId == null)
            {
                throw new ArgumentException("Field Id shouldn't be null");
            }

            List<SharedNote> result = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select Notes.user_id, name, email, Notes.id, heading, text, creation_date, last_update_date from Notes " +
                        "join Users on Notes.user_id = Users.id " +
                        "join SharedNotes on SharedNotes.user_id = @Id and SharedNotes.note_id = Notes.id";
                    command.Parameters.AddWithValue("@Id", partnerId);

                    var reader = command.ExecuteReader();
                    result = new List<SharedNote>();

                    while (reader.Read())
                    {
                        var currentNote = new SharedNote()
                        {
                            Owner = new User
                            {
                                Id = new Guid(reader["user_id"].ToString()),
                                Email = reader["email"].ToString(),
                                Name = reader["name"].ToString()
                            },
                            Id = new Guid(reader["id"].ToString()),
                            Heading = reader["heading"].ToString(),
                            Text = reader["text"].ToString(),
                            CreationDate = DateTime.Parse(reader["creation_date"].ToString()),
                            LastUpdateDate = DateTime.Parse(reader["last_update_date"].ToString())
                        };

                        result.Add(currentNote);
                    }
                    reader.Close();
                }
                return result;

            }
        }

        public IEnumerable<Note> GetByTag(Guid tagId)
        {
            if (tagId == null)
            {
                throw new ArgumentException("Field Id shouldn't be null");
            }

            List<Note> result = null;
            var tagNamesList = new List<List<String>>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Notes " +
                        "join NotesTag on id = note_id and tag_id = @Id";
                    command.Parameters.AddWithValue("@Id", tagId);

                    var reader = command.ExecuteReader();
                    result = new List<Note>();

                    while (reader.Read())
                    {
                        var tagNames = new List<String>();
                        tagNamesList.Add(tagNames);

                        var currentNote = new Note()
                        {
                            OwnerId = new Guid(reader["user_id"].ToString()),
                            Id = new Guid(reader["id"].ToString()),
                            Heading = reader["heading"].ToString(),
                            Text = reader["text"].ToString(),
                            CreationDate = DateTime.Parse(reader["creation_date"].ToString()),
                            LastUpdateDate = DateTime.Parse(reader["last_update_date"].ToString()),
                            TagNames = tagNames
                        };

                        result.Add(currentNote);
                    }
                    reader.Close();
                }

                for (int i = 0; i < result.Count; ++i)
                {
                    using (var tagCommand = connection.CreateCommand())
                    {
                        tagCommand.CommandText =
                            "select name from Tags " +
                            "join NoteTags on note_id = @id and tag_id=id;";
                        tagCommand.Parameters.AddWithValue("@id", result[i].Id.ToString());
                        var tagReader = tagCommand.ExecuteReader();

                        while (tagReader.Read())
                        {
                            tagNamesList[i].Add(tagReader["name"].ToString());
                        }
                        tagReader.Close();
                    }
                }

                return result;

            }
        }

        public void ShareTo(Guid noteId, Guid partnerId)
        {
            if (noteId == null || partnerId == null)
            {
                throw new ArgumentException("Fields Id shouldn't be null");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into SharedNotes values(@userId, @noteId);";

                    command.Parameters.AddWithValue("@userId", partnerId);
                    command.Parameters.AddWithValue("@noteId", noteId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddTag(Guid noteId, Guid tagId)
        {

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into NoteTags values(@noteId, @tagId);";

                    command.Parameters.AddWithValue("@noteId", noteId);
                    command.Parameters.AddWithValue("@tagId", tagId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void RemoveAccess(Guid noteId, Guid partnerId)
        {
            if (noteId == null || partnerId == null)
            {
                throw new ArgumentException("Fields Id shouldn't be null");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from SharedNotes " +
                        "where user_id = @userId and note_id = @noteId;";

                    command.Parameters.AddWithValue("@userId", partnerId);
                    command.Parameters.AddWithValue("@noteId", noteId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void RemoveTag(Guid noteId, Guid tagId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from NoteTags " +
                        "where note_id = @noteId and tag_id = @tagId;";

                    command.Parameters.AddWithValue("@noteId", noteId);
                    command.Parameters.AddWithValue("@tagId", tagId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
