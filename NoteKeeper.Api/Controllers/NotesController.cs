﻿using NoteKeeper.DataLayer;
using NoteKeeper.DataLayer.Sql;
using NoteKeeper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NoteKeeper.Api.Filters;
using NoteKeeper.Logger;
using System.Threading.Tasks;
using System.Web.Configuration;
using NoteKeeper.Api.NotificationService;

namespace NoteKeeper.Api.Controllers
{
    public class NotesController : ApiController
    {
        private readonly string _connectionString = WebConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString;
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
        /// Получение заметки
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <returns>Заметка</returns>
        [HttpGet]
        [Route("api/notes/{note_id}")]
        [HandleExceptionFilter]
        [ValidateModelFilter]
        public async Task<Note> GetNote(Guid note_id)
        {
            Log.Instance.Info("Полуение заметки: Id = {0}", note_id);

            return await _notesRepository.GetAsync(note_id);
        }

        /// <summary>
        /// Получение чужой заметки
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <returns>Заметка</returns>
        [HttpGet]
        [Route("api/notes/shared/{note_id}")]
        [HandleExceptionFilter]
        [ValidateModelFilter]
        public async Task<SharedNote> GetSharedNote(Guid note_id)
        {
            Log.Instance.Info("Полуение чужой заметки: Id = {0}", note_id);

            Note note = await _notesRepository.GetAsync(note_id);
            if (note != null)
            {
                User owner = await _usersRepository.GetAsync(note.OwnerId);
                return new SharedNote()
                {
                    Text = note.Text,
                    Heading = note.Heading,
                    CreationDate = note.CreationDate,
                    LastUpdateDate = note.LastUpdateDate,
                    Owner = owner,
                    Id = note.Id
                };
            } else
            {
                return null;
            }
        }

        /// <summary>
        /// Создание заметки
        /// </summary>
        /// <param name="newNote">Заметка, которую нужно создать</param>
        /// <returns>Новая заметка</returns>
        [HttpPost]
        [Route("api/notes")]
        [HandleExceptionFilter]
        [ValidateModelFilter]
        public async Task<Note> CreateNote(Note newNote)
        {
            Log.Instance.Info("Создание заметки: OwnerId = {0}", newNote.OwnerId);

            return await _notesRepository.CreateAsync(newNote);
        }

        /// <summary>
        /// Удаление заметки
        /// </summary>
        /// <param name="id">идентификатор заметки</param>
        [HttpDelete]
        [Route("api/notes/{id}")]
        [HandleExceptionFilter]
        public async Task DeleteNote(Guid id, [FromUri] Guid requestOwnerId)
        {
            Log.Instance.Info("Удаление заметки: Id = {0}", id);

            if (requestOwnerId == null)
            {
                throw new ArgumentNullException("requestOwnerId");
            }

            Note changedNote = await _notesRepository.GetAsync(id);

            if (changedNote != null)
            {
                await _notesRepository.DeleteAsync(id);

                if (requestOwnerId != changedNote.OwnerId)
                {
                    NotificationTicker.Instance.NotifyAboutNoteChanged(changedNote, changedNote.OwnerId);
                }
                foreach (User partner in changedNote.Partners)
                {
                    if (requestOwnerId != partner.Id)
                    {
                        NotificationTicker.Instance.NotifyAboutNoteChanged(changedNote, partner.Id);
                    }
                }
            }
        }

        /// <summary>
        /// Изменение заголовка заметки 
        /// </summary>
        /// <param name="id">Идентификатор заметки</param>
        /// <param name="newHeading">Новое название</param>
        /// <returns>Измененная заметка</returns>
        [HttpPut]
        [Route("api/notes/{id}/heading")]
        [HandleExceptionFilter]
        public async Task<Note> ChangeNoteHeading(Guid id, [FromBody] string newHeading, [FromUri] Guid requestOwnerId)
        {
            Log.Instance.Info("Изменение названия заметки: Id = {0}", id);

            if (requestOwnerId == null)
            {
                throw new ArgumentNullException("requestOwnerId");
            }

            Note changedNote = await _notesRepository.ChangeHeadingAsync(id, newHeading);

            if (requestOwnerId != changedNote.OwnerId)
            {
                NotificationTicker.Instance.NotifyAboutNoteChanged(changedNote, changedNote.OwnerId);
            }
            foreach (User partner in changedNote.Partners)
            {
                if (requestOwnerId != partner.Id)
                {
                    NotificationTicker.Instance.NotifyAboutNoteChanged(changedNote, partner.Id);
                }
            }

            return changedNote;
        }

        /// <summary>
        /// Изменение текста заметки 
        /// </summary>
        /// <param name="id">Идентификатор заметки</param>
        /// <param name="newText">Новый текст</param>
        /// <returns>Измененная заметка</returns>
        [HttpPut]
        [Route("api/notes/{id}/text")]
        [HandleExceptionFilter]
        public async Task<Note> ChangeNoteText(Guid id, [FromBody] string newText, [FromUri] Guid requestOwnerId)
        {
            Log.Instance.Info("Изменение текста заметки: Id = {0}", id);

            if (requestOwnerId == null)
            {
                throw new ArgumentNullException("requestOwnerId");
            }

            Note changedNote = await _notesRepository.ChangeTextAsync(id, newText);

            if (requestOwnerId != changedNote.OwnerId)
            {
                NotificationTicker.Instance.NotifyAboutNoteChanged(changedNote, changedNote.OwnerId);
            }
            foreach (User partner in changedNote.Partners)
            {
                if (requestOwnerId != partner.Id)
                {
                    NotificationTicker.Instance.NotifyAboutNoteChanged(changedNote, partner.Id);
                }
            }

            return changedNote;
        }

        /// <summary>
        /// Получение всех тегов данной заметки
        /// </summary>
        /// <param name="id">Идентификатор заметки</param>
        /// <returns>Коллекция тегов</returns>
        [HttpGet]
        [Route("api/notes/{id}/tags")]
        [HandleExceptionFilter]
        public async Task<IEnumerable<Tag>> GetNoteTags(Guid id)
        {
            Log.Instance.Info("Получение всех тегов заметки: Id = {0}", id);

            return await _tagsRepository.GetByNoteAsync(id);
        }

        /// <summary>
        /// Добавление тега к заметке
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <param name="tag_id">Идентификатор тега</param>
        [HttpPost]
        [Route("api/notes/{note_id}/tags/{tag_id}")]
        [HandleExceptionFilter]
        public async Task AddTagToNote(Guid note_id, Guid tag_id)
        {
            Log.Instance.Info("Добавление тега к заметке: NoteId = {0}, TagId = {1}", note_id, tag_id);

            await _notesRepository.AddTagAsync(note_id, tag_id);
        }

        /// <summary>
        /// Удаление тега
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <param name="tag_id">Идентификатор тега</param>
        [HttpDelete]
        [Route("api/notes/{note_id}/tags/{tag_id}")]
        [HandleExceptionFilter]
        public async Task DeleteTagFromNote(Guid note_id, Guid tag_id)
        {
            Log.Instance.Info("Удаление тега у заметки: NoteId = {0}, TagId = {1}", note_id, tag_id);

            await _notesRepository.RemoveTagAsync(note_id, tag_id);
        }

        /// <summary>
        /// Получение всех пользователей, которые имеют доступ к заметке
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        [HttpGet]
        [Route("api/notes/{note_id}/shared-to-users")]
        [HandleExceptionFilter]
        public async Task<IEnumerable<User>> GetPartnersByNote(Guid note_id)
        {
            Log.Instance.Info("Получение всех пользователей, которые имеют доступ к заметке: NoteId = {0}", note_id);

            return await _usersRepository.GetPartnersByNoteAsync(note_id);
        }

        /// <summary>
        /// Открытие пользователю доступа к заметке
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <param name="user_id">Идентификатор пользователя</param>
        [HttpPost]
        [Route("api/notes/{note_id}/shared-to-users/{user_id}")]
        [HandleExceptionFilter]
        public async Task ShareNoteToUser(Guid note_id, Guid user_id)
        {
            Log.Instance.Info("Открытие пользователю доступа к заметке: NoteId = {0}, UserId = {1}", note_id, user_id);

            await _notesRepository.ShareToAsync(note_id, user_id);
        }

        /// <summary>
        /// Удаление доступа к заметке для пользователя
        /// </summary>
        /// <param name="note_id">Идентификатор заметки</param>
        /// <param name="user_id">Идентификатор пользователя</param>
        [HttpDelete]
        [Route("api/notes/{note_id}/shared-to-users/{user_id}")]
        [HandleExceptionFilter]
        public async Task RemoveAccessToNoteFromUser(Guid note_id, Guid user_id, [FromUri] Guid requestOwnerId)
        {
            if(requestOwnerId == null)
            {
                throw new ArgumentNullException("requestOwnerId");
            }

            Log.Instance.Info("Удаление доступа к заметке для пользователя: NoteId = {0}, UserId = {1}", note_id, user_id);

            await _notesRepository.RemoveAccessAsync(note_id, user_id);

            if(requestOwnerId != user_id)
            {
                NotificationTicker.Instance.NotifyAboutDeniedAccess(note_id, user_id);
            }
        }
    }
}
