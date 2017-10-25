using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoteKeeper.Model;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace NoteKeeper.DataLayer.Sql.Test
{
    [TestClass]
    public class TagsRepositoryTest
    {
        private static User _user;
        private static Note _note;
        private const String _connectionString = @"Server=localhost\SQLEXPRESS;Trusted_Connection=yes;Database=NoteKeeper;";
        private readonly List<Tag> _tagsToDelete = new List<Tag>();
        private static ITagsRepository _repository;

        [ClassInitialize]
        public static void InitializeData(TestContext context)
        {
            _user = new User()
            {
                Name = "Vasiliy",
                Email = (Guid.NewGuid()).ToString()
            };
            var userRepository = new UsersRepository(_connectionString);
            _user = userRepository.Create(_user);

            _note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            var notesRepository = new NotesRepository(_connectionString);
            notesRepository.Create(_note);

            _repository = new TagsRepository(_connectionString);
        }

        [TestMethod]
        public void CreateTagTest()
        {
            var tag = new Tag()
            {
                OwnerId = _user.Id,
                Name = "TestTag"
            };
            _tagsToDelete.Add(tag);

            _repository.Create(tag);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Tags " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", tag.Id);

                    var reader = command.ExecuteReader();

                    Assert.IsTrue(reader.Read());
                }
            }

        }

        [TestMethod]
        public void DeleteTagTest()
        {
            //arrange
            var tag = new Tag()
            {
                OwnerId = _user.Id,
                Name = "TestTag"
            };
            tag = _repository.Create(tag);

            //act
            _repository.Delete(tag.Id);

            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Notes " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", tag.Id);

                    var reader = command.ExecuteReader();

                    Assert.IsFalse(reader.Read());
                }
            }
        }

        [TestMethod]
        public void GetTagTest()
        {
            var tag = new Tag()
            {
                OwnerId = _user.Id,
                Name = "Test"
            };
            _tagsToDelete.Add(tag);

            tag = _repository.Create(tag);
            var newTag = _repository.Get(tag.Id);

            Assert.IsNotNull(newTag);
            Assert.AreEqual(tag.Id, newTag.Id);
        }

        [TestMethod]
        public void ChangeTagNameTest()
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
            tag = _repository.Create(tag);
            _repository.ChangeName(tag.Id, newName);

            //assert
            var newTag = _repository.Get(tag.Id);
            Assert.AreEqual(newTag.Name, newName);
        }

        [TestMethod]
        public void GetTagsByOwnerTest()
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
            tag1 = _repository.Create(tag1);
            tag2 = _repository.Create(tag2);

            IEnumerable<Tag> result = _repository.GetByOwner(_user.Id);

            List<Tag> resultList = new List<Tag>(result);
            Assert.AreEqual(resultList.Count, 2);
            foreach(var recievedTag in resultList)
            {
                Assert.IsTrue(recievedTag.Id.Equals(tag1.Id) || recievedTag.Id.Equals(tag2.Id));
            }
        }

        [TestMethod]
        public void GetTagsByNoteAndAddNoteTest()
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
            tag1 = _repository.Create(tag1);
            tag2 = _repository.Create(tag2);
            var note = new Note()
            {
                OwnerId = _user.Id,
                Heading = "Test",
                Text = "Test",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            var noteRepository = new NotesRepository(_connectionString);
            noteRepository.Create(note);
            noteRepository.AddTag(note.Id, tag1.Id);
            noteRepository.AddTag(note.Id, tag2.Id);

            IEnumerable<Tag> result = _repository.GetByNote(note.Id);

            List<Tag> resultList = new List<Tag>(result);
            Assert.AreEqual(resultList.Count, 2);
            foreach (var recievedTag in resultList)
            {
                Assert.IsTrue(recievedTag.Id.Equals(tag1.Id) || recievedTag.Id.Equals(tag2.Id));
            }
            noteRepository.Delete(note.Id);
        }

        [TestCleanup]
        public void CleanupData()
        {
            foreach (var tag in _tagsToDelete)
            {
                _repository.Delete(tag.Id);
            }
        }

        [ClassCleanup]
        public static void CleanupClassData()
        {
            var userRepository = new UsersRepository(_connectionString);
            userRepository.Delete(_user.Id);

            var notesRepository = new NotesRepository(_connectionString);
            notesRepository.Delete(_note.Id);
        }
    }
}
