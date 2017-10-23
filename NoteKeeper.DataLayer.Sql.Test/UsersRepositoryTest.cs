using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using NoteKeeper.Model;
using System.Collections.Generic;
using System.Diagnostics;

namespace NoteKeeper.DataLayer.Sql.Test
{
    [TestClass]
    public class UsersRepositoryTest
    {
        private const String _connectionString = @"Server=localhost\SQLEXPRESS;Trusted_Connection=yes;Database=NoteKeeper;";
        private readonly List<User> _usersToDelete = new List<User>();

        [TestMethod]
        public void CreateConnectionTest()
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            conn.Close();
        }

        [TestMethod]
        public void CreateUserTest()
        {
            //arrange
            var user = new User
            {
                Name = "Vasiliy",
                Email = Guid.NewGuid().ToString()
            };

            var repository = new UsersRepository(_connectionString);
            _usersToDelete.Add(user);

            //act
            user = repository.Create(user);
            
            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using(var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Users " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", user.Id);

                    var reader = command.ExecuteReader();

                    Assert.IsTrue(reader.Read());
                }
            }

        }

        public void ChangeUserNameTest()
        {
            //arrange
            var user = new User
            {
                Name = "Vasiliy",
                Email = Guid.NewGuid().ToString()
            };
            _usersToDelete.Add(user);

            string newName = "Ivan";

            //act
            note = _repository.Create(note);
            note = _repository.ChangeHeading(note, newHeading);

            //assert
            var newNote = _repository.Get(note.Id);
            Assert.AreEqual(note.Heading, newNote.Heading);
        }

        [TestMethod]
        public void DeleteUserTest()
        {
            //arrange
            var user = new User
            {
                Name = "Vasiliy",
                Email = Guid.NewGuid().ToString()
            };

            var repository = new UsersRepository(_connectionString);
            user = repository.Create(user);

            //act
            repository.Delete(user);

            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Users " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", user.Id);

                    var reader = command.ExecuteReader();

                    Assert.IsFalse(reader.Read());
                }
            }

        }

        [TestCleanup]
        public void CleanData()
        {
            var repository = new UsersRepository(_connectionString);
            foreach(var user in _usersToDelete)
            {
                repository.Delete(user);
            }
        }
    }
}
