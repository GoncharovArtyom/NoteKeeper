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
using System.Threading.Tasks;

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
        public async Task<User> Get(Guid id)
        {
            Log.Instance.Info("Получение пользователя: Id = {0}", id);

            return await _usersRepository.GetAsync(id);
        }

        /// <summary>
        /// Получение пользователя по Email
        /// </summary>
        /// <param name="email">Email пользователя</param>
        /// <returns>Пользователь</returns>
        [HttpGet]
        [Route("api/users/have-email")]
        [HandleExceptionFilter]
        public async Task<User> Get([FromUri] string email)
        {
            Log.Instance.Info("Получение пользователя: Email = {0}", email);

            return await _usersRepository.GetAsync(email);
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
        public async Task<User> CreateUser([FromBody] User newUser)
        {
            Log.Instance.Info("Создание пользователя: Email = {0}", newUser.Email);

            return await _usersRepository.CreateAsync(newUser);
        }

        /// <summary>
        /// Удаление пользователя по идентификатору
        /// </summary>
        /// <param name="id">идентификатор</param>
        [HttpDelete]
        [Route("api/users/{id}")]
        [HandleExceptionFilter]
        public async Task DeleteUser(Guid id)
        {
            Log.Instance.Info("Удаление пользователя: Id = {0}", id);

            await _usersRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Изменение имени пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="newName">Новое имя</param>
        [HttpPut]
        [Route("api/users/{id}/name")]
        [HandleExceptionFilter]
        public async Task ChangeUserName(Guid id, [FromBody] string newName)
        {
            Log.Instance.Info("Изменение имени пользователя: Id = {0}", id);

            await _usersRepository.ChangeNameAsync(id, newName);
        }

        /// <summary>
        /// Получение всех заметок пользователя по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Коллекция заметок пользователя</returns>
        [HttpGet]
        [Route("api/users/{id}/notes")]
        [HandleExceptionFilter]
        public async Task<IEnumerable<Note>> GetUserNotes(Guid id)
        {
            Log.Instance.Info("Получение заметок пользователя: Id = {0}", id);

            return await _notesRepository.GetByOwnerAsync(id);
        }

        /// <summary>
        /// Получение всех тегов пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns>Коллекция тегов пользователя</returns>
        [HttpGet]
        [Route("api/users/{id}/tags")]
        [HandleExceptionFilter]
        public async Task<IEnumerable<Tag>> GetUserTags(Guid id)
        {
            Log.Instance.Info("Получение тегов пользователя: Id = {0}", id);

            return await _tagsRepository.GetByOwnerAsync(id);
        }

        /// <summary>
        /// Получение всех заметок, которыми поделились с пользователем
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Коллекция заметок</returns>
        [HttpGet]
        [Route("api/users/{id}/shared-notes")]
        [HandleExceptionFilter]
        public async Task<IEnumerable<SharedNote>> GetNotesSharedToUser(Guid id)
        {
            Log.Instance.Info("Получение заметок, которыми поделились с пользователем: Id = {0}", id);

            return await _notesRepository.GetByPartnerAsync(id);
        }

        /// <summary>
        /// Удаление доступа к заметке
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="note_id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/users/{user_id}/shared-notes/{note_id}")]
        [HandleExceptionFilter]
        public async Task RemoveAcceessToNote(Guid user_id, Guid note_id)
        {
            Log.Instance.Info("Удаление доступа к заметке: UserId = {0}, NoteId = {1}", user_id, note_id);

            await _notesRepository.RemoveAccessAsync(note_id, user_id);
        }
    }
}
