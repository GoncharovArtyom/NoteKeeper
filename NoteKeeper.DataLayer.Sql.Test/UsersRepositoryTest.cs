using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using NoteKeeper.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Threading.Tasks;

namespace NoteKeeper.DataLayer.Sql.Test
{
    [TestClass]
    public class UsersRepositoryTest
    {
        private const String _connectionString = @"Server=localhost\SQLEXPRESS;Trusted_Connection=yes;Database=NoteKeeper;";
        private readonly List<User> _usersToDelete = new List<User>();

        [TestMethod]
        public async Task CreateConnectionTest()
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            conn.Close();
        }

        [TestMethod]
        public async Task CreateUserTest()
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
            user = await repository.CreateAsync(user);
            
            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using(var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Users " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", user.Id);

                    var reader = await command.ExecuteReaderAsync();

                    Assert.IsTrue(reader.HasRows);
                }
            }

        }

        [TestMethod]
        public async Task DeleteUserTest()
        {
            //arrange
            var user = new User
            {
                Name = "Vasiliy",
                Email = Guid.NewGuid().ToString()
            };

            var repository = new UsersRepository(_connectionString);
            user = await repository.CreateAsync(user);

            //act
            await repository.DeleteAsync(user.Id);

            //assert
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "select id from Users " +
                        "where id = @Id;";
                    command.Parameters.AddWithValue("@Id", user.Id);

                    var reader = await command.ExecuteReaderAsync();

                    Assert.IsFalse(reader.HasRows);
                }
            }

        }

        [TestMethod]
        public async Task GetPartnersByNoteTest()
        {
            //arrange
            var user = new User
            {
                Name = "Vasiliy",
                Email = Guid.NewGuid().ToString()
            };
            var user2 = new User
            {
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };
            var user3 = new User
            {
                Name = "Ivan",
                Email = Guid.NewGuid().ToString()
            };

            var repository = new UsersRepository(_connectionString);
            user = await repository.CreateAsync(user);
            user2 = await repository.CreateAsync(user2);
            user3 = await repository.CreateAsync(user3);

            _usersToDelete.Add(user);
            _usersToDelete.Add(user2);
            _usersToDelete.Add(user3);

            var note = new Note()
            {
                OwnerId = user.Id,
                Heading="",
                Text = "",
                CreationDate = new DateTime(),
                LastUpdateDate = new DateTime()
            };
            var notesRepository = new NotesRepository(_connectionString);
            await notesRepository.CreateAsync(note);

            //act
            await notesRepository.ShareToAsync(note.Id, user2.Id);
            await notesRepository.ShareToAsync(note.Id, user3.Id);
            var result = new List<User>(await repository.GetPartnersByNoteAsync(note.Id));

            //assert
            Assert.AreEqual(result.Count, 2);
            foreach(var partner in result)
            {
                Assert.IsTrue(partner.Id == user2.Id || partner.Id == user3.Id);
            }

            await notesRepository.DeleteAsync(note.Id);
        }

        [TestCleanup]
        public async Task CleanData()
        {
            var repository = new UsersRepository(_connectionString);
            foreach(var user in _usersToDelete)
            {
                await repository.DeleteAsync(user.Id);
            }
        }
    }
}
