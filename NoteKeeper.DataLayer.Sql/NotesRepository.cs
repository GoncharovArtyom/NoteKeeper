using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoteKeeper.Model;
using System.Data.SqlClient;
using NoteKeeper.DataLayer;
using NoteKeeper.DataLayer.Exceptions;

namespace NoteKeeper.DataLayer.Sql
{
    public class NotesRepository : INotesRepository
    {
        private readonly String _connectionString;
        public NotesRepository(String connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<Note> ChangeHeadingAsync(Guid noteId, string newHeading)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "update Notes " +
                        "set heading = @newHeading, last_update_date = @now " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@newHeading", newHeading);
                    command.Parameters.AddWithValue("@Id", noteId);
                    command.Parameters.AddWithValue("@now", DateTime.Now);

                    await command.ExecuteNonQueryAsync();

                }
            }

            return await GetAsync(noteId);
        }

        public async Task<Note> ChangeTextAsync(Guid noteId, string newText)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "update Notes " +
                        "set text = @newText, last_update_date = @now " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@newText", newText);
                    command.Parameters.AddWithValue("@Id", noteId);
                    command.Parameters.AddWithValue("@now", DateTime.Now);

                    await command.ExecuteNonQueryAsync();

                }
            }
            return await GetAsync(noteId);
        }

        public async Task<Note> CreateAsync(Note newNote)
        {
            newNote.CreationDate = DateTime.Now;
            newNote.LastUpdateDate = DateTime.Now;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                //Проверка нарушения целостности
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from users " +
                        "where id = @OwnerID;";
                    command.Parameters.AddWithValue("@OwnerID", newNote.OwnerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            throw new CreateException<Note>("Пользователя, которому принадлежит эта заметка, не существует")
                            {
                                Item = newNote
                            };
                        }
                    }
                }

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

                    await command.ExecuteNonQueryAsync();

                    return newNote;
                }
            }
        }

        public async Task DeleteAsync(Guid noteId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from Notes " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", noteId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Note> GetAsync(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                Note resultNote = null;
                var tagNames = new List<string>();
                using (var getNoteCommand = connection.CreateCommand())
                {
                    getNoteCommand.CommandText =
                        "select * from Notes " +
                        "where id = @id;";
                    getNoteCommand.Parameters.AddWithValue("@id", id);

                    var reader = await getNoteCommand.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
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

                    var reader = await getTagsComand.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
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
                }
                resultNote.Partners = partners;

                return resultNote;
            }
        }

        public async Task<IEnumerable<Note>> GetByOwnerAsync(Guid ownerId)
        {
            List<Note> result = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Notes " +
                        "where user_id = @Id";
                    command.Parameters.AddWithValue("@Id", ownerId);

                    var reader = await command.ExecuteReaderAsync();
                    result = new List<Note>();

                    while (await reader.ReadAsync())
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
                        var tagReader = await tagCommand.ExecuteReaderAsync();

                        var tagNames = new List<string>();
                        while (await tagReader.ReadAsync())
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

                        var reader = await getPartnersCommend.ExecuteReaderAsync();
                        var partners = new List<User>();
                        while (await reader.ReadAsync())
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

        public async Task<IEnumerable<SharedNote>> GetByPartnerAsync(Guid partnerId)
        {
            List<SharedNote> result = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select Notes.user_id, name, email, Notes.id, heading, text, creation_date, last_update_date from Notes " +
                        "join Users on Notes.user_id = Users.id " +
                        "join SharedNotes on SharedNotes.user_id = @Id and SharedNotes.note_id = Notes.id";
                    command.Parameters.AddWithValue("@Id", partnerId);

                    var reader = await command.ExecuteReaderAsync();
                    result = new List<SharedNote>();

                    while (await reader.ReadAsync())
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

        public async Task<IEnumerable<Note>> GetByTagAsync(Guid tagId)
        {
            List<Note> result = null;
            var tagNamesList = new List<List<String>>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Notes " +
                        "join NotesTag on id = note_id and tag_id = @Id";
                    command.Parameters.AddWithValue("@Id", tagId);

                    var reader = await command.ExecuteReaderAsync();
                    result = new List<Note>();

                    while (await reader.ReadAsync())
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
                        var tagReader = await tagCommand.ExecuteReaderAsync();

                        while (await tagReader.ReadAsync())
                        {
                            tagNamesList[i].Add(tagReader["name"].ToString());
                        }
                        tagReader.Close();
                    }
                }

                return result;

            }
        }

        public async Task ShareToAsync(Guid noteId, Guid partnerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                //Проверка того, что данный пользователь уже имеет доступ к заметке
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from SharedNotes " +
                        "where note_id = @NoteId and user_id = @PartnerId";
                    command.Parameters.AddWithValue("@NoteId", noteId);
                    command.Parameters.AddWithValue("@PartnerId", partnerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            return;
                        }
                    }
                }
                //Проверка целостности
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Users " +
                        "where id = @PartnerId";
                    command.Parameters.AddWithValue("@PartnerId", partnerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            throw new CreateRelationException("Такого пользователя не существует")
                            {
                                RelationName = "SharedNotes",
                                FirstItemId = noteId,
                                FirstItemTypeName = typeof(Note).ToString(),
                                SecondItemId = partnerId,
                                SecondItemTypeName = typeof(User).ToString()
                            };
                        }
                    }
                }
                //Проверка целостности
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Notes " +
                        "where id = @NoteId";
                    command.Parameters.AddWithValue("@NoteId", noteId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            throw new CreateRelationException("Такой заметки не существует")
                            {
                                RelationName = "SharedNotes",
                                FirstItemId = noteId,
                                FirstItemTypeName = typeof(Note).ToString(),
                                SecondItemId = partnerId,
                                SecondItemTypeName = typeof(User).ToString()
                            };
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into SharedNotes values(@userId, @noteId);";

                    command.Parameters.AddWithValue("@userId", partnerId);
                    command.Parameters.AddWithValue("@noteId", noteId);

                    await command.ExecuteNonQueryAsync();

                }
            }
        }

        public async Task AddTagAsync(Guid noteId, Guid tagId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                //Проверка того, что данная заметка уже помечена таким тегом
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from NoteTags " +
                        "where note_id = @NoteId and tag_id = @TagId";
                    command.Parameters.AddWithValue("@NoteId", noteId);
                    command.Parameters.AddWithValue("@TagId", tagId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            return;
                        }
                    }
                }
                //Проверка целостности
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Tags " +
                        "where id = @TagId";
                    command.Parameters.AddWithValue("@TagId", tagId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            throw new CreateRelationException("Такого тега не существует")
                            {
                                RelationName = "NoteTags",
                                FirstItemId = noteId,
                                FirstItemTypeName = typeof(Note).ToString(),
                                SecondItemId = tagId,
                                SecondItemTypeName = typeof(Tag).ToString()
                            };
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select * from Notes " +
                        "where id = @NoteId";
                    command.Parameters.AddWithValue("@NoteId", noteId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            throw new CreateRelationException("Такой заметки не существует")
                            {
                                RelationName = "SharedNotes",
                                FirstItemId = noteId,
                                FirstItemTypeName = typeof(Note).ToString(),
                                SecondItemId = tagId,
                                SecondItemTypeName = typeof(Tag).ToString()
                            };
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "insert into NoteTags values(@noteId, @tagId);";

                    command.Parameters.AddWithValue("@noteId", noteId);
                    command.Parameters.AddWithValue("@tagId", tagId);

                    await command.ExecuteNonQueryAsync();

                }
            }
        }

        public async Task RemoveAccessAsync(Guid noteId, Guid partnerId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from SharedNotes " +
                        "where user_id = @userId and note_id = @noteId;";

                    command.Parameters.AddWithValue("@userId", partnerId);
                    command.Parameters.AddWithValue("@noteId", noteId);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RemoveTagAsync(Guid noteId, Guid tagId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "delete from NoteTags " +
                        "where note_id = @noteId and tag_id = @tagId;";

                    command.Parameters.AddWithValue("@noteId", noteId);
                    command.Parameters.AddWithValue("@tagId", tagId);

                    await command.ExecuteNonQueryAsync();

                }
            }
        }
    }
}
