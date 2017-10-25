﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using NoteKeeper.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

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
            repository.Delete(user.Id);

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

        [TestMethod]
        public void GetPartnersByNoteTest()
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
            user = repository.Create(user);
            user2 = repository.Create(user2);
            user3 = repository.Create(user3);

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
            notesRepository.Create(note);

            //act
            notesRepository.ShareTo(note.Id, user2.Id);
            notesRepository.ShareTo(note.Id, user3.Id);
            var result = new List<User>(repository.GetPartnersByNote(note.Id));

            //assert
            Assert.AreEqual(result.Count, 2);
            foreach(var partner in result)
            {
                Assert.IsTrue(partner.Id == user2.Id || partner.Id == user3.Id);
            }

            notesRepository.Delete(note.Id);
        }

        [TestCleanup]
        public void CleanData()
        {
            var repository = new UsersRepository(_connectionString);
            foreach(var user in _usersToDelete)
            {
                repository.Delete(user.Id);
            }
        }
    }
}