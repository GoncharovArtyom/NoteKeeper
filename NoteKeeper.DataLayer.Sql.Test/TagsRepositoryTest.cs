using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteKeeper.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Configuration;

namespace NoteKeeper.DataLayer.Sql.Test
{
    [TestClass]
    public class TagsRepositoryTest
    {
        private static User _user;
        private static Note _note;
        private static readonly String _connectionString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
        private readonly List<Tag> _tagsToDelete = new List<Tag>();
        private static ITagsRepository _repository;

        [ClassInitialize]
        public static async Task InitializeData(TestContext context)
        {
            _user = new User()
            {
                Name = "Vasiliy",
                Email = (Guid.NewGuid()).ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            _user = await userRepository.CreateAsync(_user);

            _note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            var notesRepository = new NotesRepository(_connectionString);
            await notesRepository.CreateAsync(_note);

            _repository = new TagsRepository(_connectionString);
        }

        [TestMethod]
        public async Task CreateTagTest()
        {
            var tag = new Tag()
            {
                OwnerId = _user.Id,
                Name = "TestTag"
            };
            _tagsToDelete.Add(tag);

            await _repository.CreateAsync(tag);

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Tags " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", tag.Id);

                    var reader = await command.ExecuteReaderAsync();

                    Assert.IsTrue(reader.HasRows);
                }
            }

        }

        [TestMethod]
        public async Task DeleteTagTest()
        {
            //arrange
            var tag = new Tag()
            {
                OwnerId = _user.Id,
                Name = "TestTag"
            };
            tag = await _repository.CreateAsync(tag);

            //act
            await _repository.DeleteAsync(tag.Id);

            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Notes " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", tag.Id);

                    var reader = await command.ExecuteReaderAsync();

                    Assert.IsFalse(reader.HasRows);
                }
            }
        }

        [TestMethod]
        public async Task GetTagTest()
        {
            var tag = new Tag()
            {
                OwnerId = _user.Id,
                Name = "Test"
            };
            _tagsToDelete.Add(tag);

            tag = await _repository.CreateAsync(tag);
            var newTag = await _repository.GetAsync(tag.Id);

            Assert.IsNotNull(newTag);
            Assert.AreEqual(tag.Id, newTag.Id);
        }

        [TestMethod]
        public async Task ChangeTagNameTest()
        {
            //arrange
            var tag = new Tag()
            {
                OwnerId = _user.Id,
                Name = "Test"
            };
            _tagsToDelete.Add(tag);

            string newName = "Test2";

            //act
            tag = await _repository.CreateAsync(tag);
            await _repository.ChangeNameAsync(tag.Id, newName);

            //assert
            var newTag = await _repository.GetAsync(tag.Id);
            Assert.AreEqual(newTag.Name, newName);
        }

        [TestMethod]
        public async Task GetTagsByOwnerTest()
        {
            var tag1 = new Tag()
            {
                OwnerId = _user.Id,
                Name = "Test1"
            };
            var tag2 = new Tag()
            {
                OwnerId = _user.Id,
                Name = "Test2"
            };
            _tagsToDelete.Add(tag1);
            _tagsToDelete.Add(tag2);
            tag1 = await _repository.CreateAsync(tag1);
            tag2 = await _repository.CreateAsync(tag2);

            IEnumerable<Tag> result = await _repository.GetByOwnerAsync(_user.Id);

            List<Tag> resultList = new List<Tag>(result);
            Assert.AreEqual(resultList.Count, 2);
            foreach(var recievedTag in resultList)
            {
                Assert.IsTrue(recievedTag.Id.Equals(tag1.Id) || recievedTag.Id.Equals(tag2.Id));
            }
        }

        [TestMethod]
        public async Task GetTagsByNoteAndAddNoteTest()
        {
            var tag1 = new Tag()
            {
                OwnerId = _user.Id,
                Name = "Test1"
            };
            var tag2 = new Tag()
            {
                OwnerId = _user.Id,
                Name = "Test2"
            };
            _tagsToDelete.Add(tag1);
            _tagsToDelete.Add(tag2);
            tag1 = await _repository.CreateAsync(tag1);
            tag2 = await _repository.CreateAsync(tag2);
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            var noteRepository = new NotesRepository(_connectionString);
            await noteRepository.CreateAsync(note);
            await noteRepository.AddTagAsync(note.Id, tag1.Id);
            await noteRepository.AddTagAsync(note.Id, tag2.Id);

            IEnumerable<Tag> result = await _repository.GetByNoteAsync(note.Id);

            List<Tag> resultList = new List<Tag>(result);
            Assert.AreEqual(resultList.Count, 2);
            foreach (var recievedTag in resultList)
            {
                Assert.IsTrue(recievedTag.Id.Equals(tag1.Id) || recievedTag.Id.Equals(tag2.Id));
            }
            await noteRepository.DeleteAsync(note.Id);
        }

        [TestCleanup]
        public async Task CleanupData()
        {
            foreach (var tag in _tagsToDelete)
            {
                await _repository.DeleteAsync(tag.Id);
            }
        }

        [ClassCleanup]
        public static async Task CleanupClassData()
        {
            var userRepository = new UsersRepository(_connectionString);
            await userRepository.DeleteAsync(_user.Id);

            var notesRepository = new NotesRepository(_connectionString);
            await notesRepository.DeleteAsync(_note.Id);
        }
    }
}
