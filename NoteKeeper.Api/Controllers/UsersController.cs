using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NoteKeeper.DataLayer;
using NoteKeeper.DataLayer.Sql;
using NoteKeeper.Model;

namespace NoteKeeper.Api.Controllers
{
    public class UsersController : ApiController
    {
        private const string _connectionString = @"Server=localhost\SQLEXPRESS;Trusted_Connection=yes;Database=NoteKeeper;";
        private readonly IUsersRepository _usersRepository;
        private readonly INotesRepository _notesRepository;
        private readonly ITagsRepository _tagsRepository;

        public UsersController()
        {
            _usersRepository = new UsersRepository(_connectionString);
            _notesRepository = new NotesRepository(_connectionString);
            _tagsRepository = new TagsRepository(_connectionString);
        }

        /// <summary>
        /// Получение пользователя по id
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Пользователь</returns>
        [HttpGet]
        [Route("api/users/{id}")]
        public User Get(Guid id)
        {
            return _usersRepository.Get(id);
        }

        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="newUser">Объект - новый пользователь</param>
        /// <returns>Созданный пользователь</returns>
        [HttpPost]
        [Route("api/users")]
        public User CreateUser([FromBody] User newUser)
        {
           return _usersRepository.Create(newUser);
        }

        /// <summary>
        /// Удаление пользователя по идентификатору
        /// </summary>
        /// <param name="id">идентификатор</param>
        [HttpDelete]
        [Route("api/users/{id}")]
        public void DeleteUser(Guid id)
        {
            _usersRepository.Delete(id);
        }

        /// <summary>
        /// Изменение имени пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="newName">Новое имя</param>
        [HttpPut]
        [Route("api/users/{id}/name")]
        public void ChangeUserName(Guid id, [FromBody] string newName)
        {
            _usersRepository.ChangeName(id, newName);
        }

        /// <summary>
        /// Получение всех заметок пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Коллекция заметок пользователя</returns>
        [HttpGet]
        [Route("api/users/{id}/notes")]
        public IEnumerable<Note> GetUserNotes(Guid id)
        {
            return _notesRepository.GetByOwner(id);
        }

        /// <summary>
        /// Получение всех тегов пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Коллекция тегов пользователя</returns>
        [HttpGet]
        [Route("api/users/{id}/tags")]
        public IEnumerable<Tag> GetUserTags(Guid id)
        {
            return _tagsRepository.GetByOwner(id);
        }

        /// <summary>
        /// Получение всех заметок, которыми поделились с пользователем
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Коллекция заметок</returns>
        [HttpGet]
        [Route("api/users/{id}/shared-notes")]
        public IEnumerable<SharedNote> GetNotesSharedToUser(Guid userId)
        {
            return _notesRepository.GetByPartner(userId);
        }
    }
}
