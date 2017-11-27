using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteKeeper.Model;
using System.Collections.Generic;
using NoteKeeper.DataLayer.Sql;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Configuration;

namespace NoteKeeper.DataLayer.Sql.Test
{
    [TestClass]
    public class NotesRepositoryTest
    {
        private static User _user;
        private static readonly String _connectionString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
        private readonly List<Note> _notesToDelete = new List<Note>();
        private static INotesRepository _repository;

        [ClassInitialize]
        public static async Task CreateUser(TestContext context)
        {
            _user = new User()
            {
                Name = "Vasiliy",
                Email = (Guid.NewGuid()).ToString()
            };

            var userRepository = new UsersRepository(_connectionString);
            _user = await userRepository.CreateAsync(_user);

            _repository = new NotesRepository(_connectionString);
        }

        [TestMethod]
        public async Task CreateNoteTest()
        {
            //arrange
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };

            _notesToDelete.Add(note);

            //act
            note = await _repository.CreateAsync(note);

            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Notes " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", note.Id);

                    var reader = await command.ExecuteReaderAsync();

                    Assert.IsTrue(reader.HasRows);
                }
            }

        }

        [TestMethod]
        public async Task DeleteNoteTest()
        {
            //arrange
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };

            note = await _repository.CreateAsync(note);

            //act
            await _repository.DeleteAsync(note.Id);

            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Notes " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", note.Id);

                    var reader = await command.ExecuteReaderAsync();

                    Assert.IsFalse(reader.HasRows);
                }
            }
        }

        [TestMethod]
        public async Task GetNoteTest()
        {
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            _notesToDelete.Add(note);

            note = await _repository.CreateAsync(note);
            var newNote = await _repository.GetAsync(note.Id);

            Assert.IsNotNull(newNote);

        }

        [TestMethod]
        public async Task ChangeNoteHeadingTest()
        {
            //arrange
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test"
            };

            _notesToDelete.Add(note);

            string newHeading = "Test2";

            //act
            note = await _repository.CreateAsync(note);
            note = await _repository.ChangeHeadingAsync(note.Id, newHeading);

            //assert
            var newNote = await _repository.GetAsync(note.Id);
            Assert.AreEqual(note.Heading, newNote.Heading, newHeading);
            Assert.AreEqual(note.LastUpdateDate, newNote.LastUpdateDate);
        }

        [TestMethod]
        public async Task GetNoteByOwnerTest()
        {
            var note1 = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            var note2 = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            _notesToDelete.Add(note1);
            _notesToDelete.Add(note2);

            note1 = await _repository.CreateAsync(note1);
            note2 = await _repository.CreateAsync(note2);
            IEnumerable<Note> result = await _repository.GetByOwnerAsync(_user.Id);

            Assert.IsNotNull(result);
            List<Note> resList = new List<Note>(result);
            Assert.AreEqual(resList.Count, 2);
            foreach (var note in resList)
            {
                Assert.IsTrue(note.Id == note1.Id || note.Id == note2.Id);
            }
        }

        [TestMethod]
        public async Task ShareTest1()
        {
            //arrange
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            await _repository.CreateAsync(note);
            _notesToDelete.Add(note);
            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            await userRepository.CreateAsync(anotherUser);

            //act
            await _repository.ShareToAsync(note.Id, anotherUser.Id);

            //assert
            IEnumerable<SharedNote> result = await _repository.GetByPartnerAsync(anotherUser.Id);
            List<SharedNote> resList = new List<SharedNote>(result);
            Assert.AreEqual(resList.Count, 1);
            Assert.AreEqual(resList[0].Id, note.Id);

            await userRepository.DeleteAsync(anotherUser.Id);
        }

        [TestMethod]
        public async Task ShareTest2()
        {
            //arrange
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            await _repository.CreateAsync(note);
            _notesToDelete.Add(note);
            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            await userRepository.CreateAsync(anotherUser);

            //act
            await _repository.ShareToAsync(note.Id, anotherUser.Id);
            var resNote = await _repository.GetAsync(note.Id);

            //assert
            List<User> resList = new List<User>(resNote.Partners);
            Assert.AreEqual(resList.Count, 1);
            Assert.AreEqual(resList[0].Id, anotherUser.Id);

            await userRepository.DeleteAsync(anotherUser.Id);
        }

        [TestMethod]
        public async Task ShareTest3()
        {
            //arrange
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            await _repository.CreateAsync(note);
            _notesToDelete.Add(note);
            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            await userRepository.CreateAsync(anotherUser);

            //act
            await _repository.ShareToAsync(note.Id, anotherUser.Id);
            var result = await _repository.GetByOwnerAsync(_user.Id);

            //assert
            bool isPresent = false;
            foreach(var resNote in result)
            {
                foreach(var partner in resNote.Partners)
                {
                    if (partner.Id == anotherUser.Id && resNote.Id == note.Id)
                    {
                        isPresent = true;
                    }
                }
            }
            Assert.IsTrue(isPresent);

            await userRepository.DeleteAsync(anotherUser.Id);
        }

        [TestMethod]
        public async Task GetByPartnerTest()
        {
            //arrange
            var note1 = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            var note2 = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test2",
                Text = "Test2",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            await _repository.CreateAsync(note1);
            await _repository.CreateAsync(note2);
            _notesToDelete.Add(note1);
            _notesToDelete.Add(note2);

            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            await userRepository.CreateAsync(anotherUser);
            await _repository.ShareToAsync(note1.Id, anotherUser.Id);
            await _repository.ShareToAsync(note2.Id, anotherUser.Id);

            //act
            IEnumerable<SharedNote> result = await _repository.GetByPartnerAsync(anotherUser.Id);

            //assert

            List<SharedNote> resList = new List<SharedNote>(result);
            Assert.AreEqual(resList.Count, 2);
            foreach(var note in resList)
            {
                Assert.IsTrue(note.Id.Equals(note1.Id) || note.Id.Equals(note2.Id));
            }

            await userRepository.DeleteAsync(anotherUser.Id);
        }

        [TestMethod]
        public async Task RemoveAccessTest()
        {
            //arrange
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            await _repository.CreateAsync(note);
            _notesToDelete.Add(note);
            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            await userRepository.CreateAsync(anotherUser);

            //act
            await _repository.ShareToAsync(note.Id, anotherUser.Id);
            await _repository.RemoveAccessAsync(note.Id, anotherUser.Id);

            //assert
            IEnumerable<SharedNote> result = await _repository.GetByPartnerAsync(anotherUser.Id);
            List<SharedNote> resList = new List<SharedNote>(result);
            Assert.AreEqual(resList.Count, 0);

            await userRepository.DeleteAsync(anotherUser.Id);
        } 

        [TestMethod]
        public async Task RemoveTagTest()
        {
            //arrange
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
            var tagsRepository = new TagsRepository(_connectionString);
            tag1 = await tagsRepository.CreateAsync(tag1);
            tag2 = await tagsRepository.CreateAsync(tag2);
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            
            await _repository.CreateAsync(note);
            await _repository.AddTagAsync(note.Id, tag1.Id);
            await _repository.AddTagAsync(note.Id, tag2.Id);

            _notesToDelete.Add(note);

            //act
            await _repository.RemoveTagAsync(note.Id, tag1.Id);
            IEnumerable<Tag> result = await tagsRepository.GetByNoteAsync(note.Id);

            List<Tag> resultList = new List<Tag>(result);
            Assert.AreEqual(resultList.Count, 1);
            Assert.AreEqual(resultList[0].Id, tag2.Id);

            await tagsRepository.DeleteAsync(tag1.Id);
            await tagsRepository.DeleteAsync(tag2.Id);
        }

        [TestCleanup]
        public async Task CleanupData()
        {
            foreach (var note in _notesToDelete)
            {
                await _repository.DeleteAsync(note.Id);
            }
        }

        [ClassCleanup]
        public static async Task DeleteUser()
        {
            var userRepository = new UsersRepository(_connectionString);
            await userRepository.DeleteAsync(_user.Id);
        }
    }
}
