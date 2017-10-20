using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteKeeper.Model;
using System.Collections.Generic;
using DataLayer;
using NoteKeeper.DataLayer.Sql;
using System.Data.SqlClient;

namespace NoteKeeper.DataLayer.Sql.Test
{
    [TestClass]
    public class NotesRepositoryTest
    {
        private static User _user;
        private const String _connectionString = @"Server=localhost\SQLEXPRESS;Trusted_Connection=yes;Database=NoteKeeper;";
        private readonly List<Note> _notesToDelete = new List<Note>();
        private static INotesRepository _repository;

        [ClassInitialize]
        public static void CreateUser(TestContext context)
        {
            _user = new User()
            {
                Name = "Vasiliy",
                Email = (Guid.NewGuid()).ToString()
            };

            var userRepository = new UsersRepository(_connectionString);
            _user = userRepository.Create(_user);

            _repository = new NotesRepository(_connectionString);
        }

        [TestMethod]
        public void CreateNoteTest()
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
            note = _repository.Create(note);

            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Notes " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", note.Id);

                    var reader = command.ExecuteReader();

                    Assert.IsTrue(reader.Read());
                }
            }

        }

        [TestMethod]
        public void DeleteNoteTest()
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

            note = _repository.Create(note);

            //act
            _repository.Delete(note);

            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Notes " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", note.Id);

                    var reader = command.ExecuteReader();

                    Assert.IsFalse(reader.Read());
                }
            }
        }

        [TestMethod]
        public void GetNoteTest()
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

            note = _repository.Create(note);
            var newNote = _repository.Get(note.Id);

            Assert.IsNotNull(newNote);

        }

        [TestMethod]
        public void ChangeNoteHeadingTest()
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

            string newHeading = "Test2";

            //act
            note = _repository.Create(note);
            note = _repository.ChangeHeading(note, newHeading);

            //assert
            var newNote = _repository.Get(note.Id);
            Assert.AreEqual(note.Heading, newNote.Heading);
        }

        [TestMethod]
        public void GetNoteByOwnerTest()
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

            note1 = _repository.Create(note1);
            note2 = _repository.Create(note2);
            IEnumerable<Note> result = _repository.GetByOwner(_user);

            Assert.IsNotNull(result);
            List<Note> resList = new List<Note>(result);
            Assert.AreEqual(resList.Count, 2);
            foreach (var note in resList)
            {
                Assert.IsTrue(note.Id == note1.Id || note.Id == note2.Id);
            }
        }

        [TestMethod]
        public void ShareTest1()
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
            _repository.Create(note);
            _notesToDelete.Add(note);
            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            userRepository.Create(anotherUser);

            //act
            _repository.ShareTo(note, anotherUser);

            //assert
            IEnumerable<SharedNote> result = _repository.GetByPartner(anotherUser);
            List<SharedNote> resList = new List<SharedNote>(result);
            Assert.AreEqual(resList.Count, 1);
            Assert.AreEqual(resList[0].Id, note.Id);
        }

        [TestMethod]
        public void ShareTest2()
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
            _repository.Create(note);
            _notesToDelete.Add(note);
            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            userRepository.Create(anotherUser);

            //act
            _repository.ShareTo(note, anotherUser);
            var resNote = _repository.Get(note.Id);

            //assert
            List<User> resList = new List<User>(resNote.Partners);
            Assert.AreEqual(resList.Count, 1);
            Assert.AreEqual(resList[0].Id, anotherUser.Id);
        }

        [TestMethod]
        public void ShareTest3()
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
            _repository.Create(note);
            _notesToDelete.Add(note);
            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            userRepository.Create(anotherUser);

            //act
            _repository.ShareTo(note, anotherUser);
            var result = _repository.GetByOwner(_user);

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
        }

        [TestMethod]
        public void GetByPartnerTest()
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
            _repository.Create(note1);
            _repository.Create(note2);
            _notesToDelete.Add(note1);
            _notesToDelete.Add(note2);

            var anotherUser = new User()
            {
                Id = Guid.NewGuid(),
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            userRepository.Create(anotherUser);
            _repository.ShareTo(note1, anotherUser);
            _repository.ShareTo(note2, anotherUser);

            //act
            IEnumerable<SharedNote> result = _repository.GetByPartner(anotherUser);

            //assert

            List<SharedNote> resList = new List<SharedNote>(result);
            Assert.AreEqual(resList.Count, 2);
            foreach(var note in resList)
            {
                Assert.IsTrue(note.Id.Equals(note1.Id) || note.Id.Equals(note2.Id));
            }
        }

        [TestCleanup]
        public void CleanupData()
        {
            foreach (var note in _notesToDelete)
            {
                _repository.Delete(note);
            }
        }

        [ClassCleanup]
        public static void DeleteUser()
        {
            var userRepository = new UsersRepository(_connectionString);
            userRepository.Delete(_user);
        }
    }
}
