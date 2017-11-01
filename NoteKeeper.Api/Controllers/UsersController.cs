using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NoteKeeper.DataLayer;
using NoteKeeper.DataLayer.Sql;
using NoteKeeper.Model;
using NoteKeeper.Api.Filters;
using NoteKeeper.Logger;

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
        [HandleExceptionFilter]
        public User Get(Guid id)
        {
            Log.Instance.Info("Получение пользователя: Id = {0}", id);

            return _usersRepository.Get(id);
        }

        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="newUser">Объект - новый пользователь</param>
        /// <returns>Созданный пользователь</returns>
        [HttpPost]
        [Route("api/users")]
        [ValidateModelFilter]
        [HandleExceptionFilter]
        public User CreateUser([FromBody] User newUser)
        {
            Log.Instance.Info("Создание пользователя: Email = {0}", newUser.Email);

            return _usersRepository.Create(newUser);
        }

        /// <summary>
        /// Удаление пользователя по идентификатору
        /// </summary>
        /// <param name="id">идентификатор</param>
        [HttpDelete]
        [Route("api/users/{id}")]
        [HandleExceptionFilter]
        public void DeleteUser(Guid id)
        {
            Log.Instance.Info("Удаление пользователя: Id = {0}", id);

            _usersRepository.Delete(id);
        }

        /// <summary>
        /// Изменение имени пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="newName">Новое имя</param>
        [HttpPut]
        [Route("api/users/{id}/name")]
        [HandleExceptionFilter]
        public void ChangeUserName(Guid id, [FromBody] string newName)
        {
            Log.Instance.Info("Изменение имени пользователя: Id = {0}", id);

            _usersRepository.ChangeName(id, newName);
        }

        /// <summary>
        /// Получение всех заметок пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Коллекция заметок пользователя</returns>
        [HttpGet]
        [Route("api/users/{id}/notes")]
        [HandleExceptionFilter]
        public IEnumerable<Note> GetUserNotes(Guid id)
        {
            Log.Instance.Info("Получение заметок пользователя: Id = {0}", id);

            return _notesRepository.GetByOwner(id);
        }

        /// <summary>
        /// Получение всех тегов пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Коллекция тегов пользователя</returns>
        [HttpGet]
        [Route("api/users/{id}/tags")]
        [HandleExceptionFilter]
        public IEnumerable<Tag> GetUserTags(Guid id)
        {
            Log.Instance.Info("Получение тегов пользователя: Id = {0}", id);

            return _tagsRepository.GetByOwner(id);
        }

        /// <summary>
        /// Получение всех заметок, которыми поделились с пользователем
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Коллекция заметок</returns>
        [HttpGet]
        [Route("api/users/{id}/shared-notes")]
        [HandleExceptionFilter]
        public IEnumerable<SharedNote> GetNotesSharedToUser(Guid userId)
        {
            Log.Instance.Info("Получение заметок, которыми поделились с пользователем: Id = {0}", userId);

            return _notesRepository.GetByPartner(userId);
        }
    }
}
