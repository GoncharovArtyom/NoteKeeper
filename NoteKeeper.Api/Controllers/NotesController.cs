using NoteKeeper.DataLayer;
using NoteKeeper.DataLayer.Sql;
using NoteKeeper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace NoteKeeper.Api.Controllers
{
    public class NotesController : ApiController
    {
        private const string _connectionString = @"Server=localhost\SQLEXPRESS;Trusted_Connection=yes;Database=NoteKeeper;";
        private readonly IUsersRepository _usersRepository;
        private readonly INotesRepository _notesRepository;
        private readonly ITagsRepository _tagsRepository;

        public NotesController()
        {
            _usersRepository = new UsersRepository(_connectionString);
            _notesRepository = new NotesRepository(_connectionString);
            _tagsRepository = new TagsRepository(_connectionString);
        }

        /// <summary>
        /// Создание заметки
        /// </summary>
        /// <param name="newNote">Заметка, которую нужно создать</param>
        /// <returns>Новая заметка</returns>
        [HttpPost]
        [Route("api/notes")]
        public Note CreateNote(Note newNote)
        {
            return _notesRepository.Create(newNote);
        }

        /// <summary>
        /// Удаление заметки
        /// </summary>
        /// <param name="id">идентификатор заметки</param>
        [HttpDelete]
        [Route("api/notes/{id}")]
        public void DeleteNote(Guid id)
        {
            _notesRepository.Delete(id);
        }

        /// <summary>
        /// Изменение заголовка заметки 
        /// </summary>
        /// <param name="id">Идентификатор заметки</param>
        /// <param name="newHeading">Новое название</param>
        /// <returns>Измененная заметка</returns>
        [HttpPut]
        [Route("api/notes/{id}/heading")]
        public Note ChangeNoteHeading(Guid id, [FromBody] string newHeading)
        {
            return _notesRepository.ChangeHeading(id, newHeading);
        }

        /// <summary>
        /// Изменение текста заметки 
        /// </summary>
        /// <param name="id">Идентификатор заметки</param>
        /// <param name="newHeading">Новый текст</param>
        /// <returns>Измененная заметка</returns>
        [HttpPut]
        [Route("api/notes/{id}/text")]
        public Note ChangeNoteText(Guid id, [FromBody] string newText)
        {
            return _notesRepository.ChangeText(id, newText);
        }

        /// <summary>
        /// Получение всех тегов данной заметки
        /// </summary>
        /// <param name="id">Идентификатор заметки</param>
        /// <returns>Коллекция тегов</returns>
        [HttpGet]
        [Route("api/notes/{id}/tags")]
        public IEnumerable<Tag> GeyNoteTags(Guid id)
        {
            return _tagsRepository.GetByNote(id);
        }

        /// <summary>
        /// Добавление тега к заметке
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <param name="tag_id">Идентификатор тега</param>
        [HttpPost]
        [Route("api/notes/{note_id}/tags/{tag_id}")]
        public void AddTagToNote(Guid note_id, Guid tag_id)
        {
            _notesRepository.AddTag(note_id, tag_id);
        }

        /// <summary>
        /// Удаление тега
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <param name="tag_id">Идентификатор тега</param>
        [HttpDelete]
        [Route("api/notes/{note_id}/tags/{tag_id}")]
        public void DeleteTagFromNote(Guid note_id, Guid tag_id)
        {
            _notesRepository.RemoveTag(note_id, tag_id);
        }

        /// <summary>
        /// Получение всех пользователей, которые имеют доступ к заметке
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        [HttpGet]
        [Route("api/notes/{note_id}/shared-to-users")]
        public IEnumerable<User> ShareNoteToUser(Guid note_id)
        {
            return _usersRepository.GetPartnersByNote(note_id);
        }

        /// <summary>
        /// Открытие пользователю доступа к заметке
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <param name="user_id">Идентификатор пользователя</param>
        [HttpPost]
        [Route("api/notes/{note_id}/shared-to-users/{user_id}")]
        public void ShareNoteToUser(Guid note_id, Guid user_id)
        {
            _notesRepository.ShareTo(note_id, user_id);
        }

        /// <summary>
        /// Удаление доступа к заметке для пользователя
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <param name="user_id">Идентификатор пользователя</param>
        [HttpDelete]
        [Route("api/notes/{note_id}/shared-to-users/{user_id}")]
        public void RemoveAccessToNoteFromUser(Guid note_id, Guid user_id)
        {
            _notesRepository.RemoveAccess(note_id, user_id);
        }
    }
}
