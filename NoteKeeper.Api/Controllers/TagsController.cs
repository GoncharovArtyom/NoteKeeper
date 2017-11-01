using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NoteKeeper.Model;
using NoteKeeper.DataLayer;
using NoteKeeper.DataLayer.Sql;
using NoteKeeper.Api.Filters;
using NoteKeeper.Logger;

namespace NoteKeeper.Api.Controllers
{
    public class TagsController : ApiController
    {
        private const string _connectionString = @"Server=localhost\SQLEXPRESS;Trusted_Connection=yes;Database=NoteKeeper;";
        private readonly IUsersRepository _usersRepository;
        private readonly INotesRepository _notesRepository;
        private readonly ITagsRepository _tagsRepository;

        public TagsController()
        {
            _usersRepository = new UsersRepository(_connectionString);
            _notesRepository = new NotesRepository(_connectionString);
            _tagsRepository = new TagsRepository(_connectionString);
        }

        /// <summary>
        /// Удаление тега по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор тега</param>
        [HttpDelete]
        [Route("api/tags/{id}")]
        [HandleExceptionFilter]
        public void DeleteTag(Guid id)
        {
            Log.Instance.Info("Удаление тега: Id = {0}", id);

            _tagsRepository.Delete(id);
        }

        /// <summary>
        /// Создание тега
        /// </summary>
        /// <param name="newTag">Тег, который нужно создать</param>
        /// <returns>Созданный тег</returns>
        [HttpPost]
        [Route("api/tags")]
        [HandleExceptionFilter]
        [ValidateModelFilter]
        public Tag CreateTag([FromBody] Tag newTag)
        {
            Log.Instance.Info("Создание тега: OwnerId = {0}", newTag.OwnerId);

            return _tagsRepository.Create(newTag);
        }

        /// <summary>
        /// Изменение имени тега по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор тега</param>
        /// <param name="newName">Новое имя</param>
        [HttpPut]
        [Route("api/tags/{id}/name")]
        [HandleExceptionFilter]
        public void ChangeTagName(Guid id,[FromBody] string newName)
        {
            Log.Instance.Info("Изменение названия тега: Id = {0}", id);

            _tagsRepository.ChangeName(id, newName);
        }

        /// <summary>
        /// Получение всех заметок с данным тегом
        /// </summary>
        /// <param name="id">Идентификатор тега</param>
        /// <returns>Коллекция заметок</returns>
        [HttpGet]
        [Route("api/tags/{id}/notes")]
        [HandleExceptionFilter]
        public IEnumerable<Note> GetTagNotes(Guid id)
        {
            Log.Instance.Info("Получение заметок с тегом: Id = {0}", id);

            return _notesRepository.GetByTag(id);
        }
    }
}
