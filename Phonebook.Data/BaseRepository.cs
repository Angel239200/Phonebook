﻿using Phonebook.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Phonebook.Data
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity, new()
    {
        private readonly string filePath = $"{typeof(T).Name}s.txt";
        private readonly string temporaryFilePath = $"temp{typeof(T).Name}s.txt";

        private readonly string typeNameToLower = typeof(T).Name.ToLower();

        public BaseRepository()
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                if (typeNameToLower == "user")
                {
                    var user = new User(1, "admin", "adminpass", "admin", "admin", true);
                    CreateNewEntity(user as T);
                }
            }
        }

        private bool IsDataValid(T entityToValidate)
        {
            foreach (var property in entityToValidate.GetType().GetProperties())
            {
                string propertyNameLower = property.Name.ToLower();
                if (propertyNameLower == "createdate" || propertyNameLower == "updatedate")
                    continue;
                else if (string.IsNullOrWhiteSpace(property.GetValue(entityToValidate)?.ToString()))
                    return false;
                else if (propertyNameLower == "isadmin" && !bool.Parse(property.GetValue(entityToValidate).ToString()))
                    return false;
            }
            return true;
        }

        public T GetEntity(uint entityId)
        {
            return GetAllEntities().SingleOrDefault(u => u.Id == entityId);
        }

        public User GetUser(string username)
        {
            return GetAllEntities().Where(e => e is User).Cast<User>().FirstOrDefault(u => u.Username == username);
        }

        public IEnumerable<T> GetAllEntities()
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string[] userData = reader.ReadLine().Split(',').Select(d => d.Trim()).ToArray();

                    T entity = new T();
                    int index = 0;
                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (index == 4 && typeNameToLower == "user")
                            property.SetValue(entity, bool.Parse(userData[index]));
                        else if (index == 5 && typeNameToLower == "user")
                            property.SetValue(entity, int.Parse(userData[index]));
                        else if (((index == 6 || index == 7) && typeNameToLower == "user") || ((index == 5 || index == 6) && typeNameToLower == "contact") || ((index == 3 || index == 4) && typeNameToLower == "phone"))
                            property.SetValue(entity, DateTimeOffset.Parse(userData[index]));
                        else if (typeNameToLower == "contact" && (index == 0 || index == 4))
                            property.SetValue(entity, int.Parse(userData[index]));
                        else if (typeNameToLower == "phone" && (index == 0 || index == 2))
                            property.SetValue(entity, int.Parse(userData[index]));
                        else
                            property.SetValue(entity, userData[index]);
                        index++;
                    }

                    yield return entity;
                }
            }
        }

        public bool EditEntity(T entityToSave, uint entityId)
        {
            if (entityId == 0 || !IsDataValid(entityToSave))
                return false;

            entityToSave.Id = entityId;
            entityToSave.CreateDate = GetEntity(entityId).CreateDate;

            using (StreamWriter writer = new StreamWriter(temporaryFilePath, true))
            {
                foreach (T entity in GetAllEntities())
                {
                    string entityDataRow = null;

                    if (entity.Id == entityToSave.Id)
                    {
                        foreach (var property in typeof(T).GetProperties())
                        {
                            if (property.Name.ToLower() == "updatedate")
                            {
                                entityDataRow += $"{DateTimeOffset.UtcNow}, ";
                                continue;
                            }
                            entityDataRow += $"{property.GetValue(entityToSave)}, ";
                        }
                    }
                    else
                    {
                        foreach (var property in typeof(T).GetProperties())
                        {
                            entityDataRow += $"{property.GetValue(entity)}, ";
                        }
                    }
                    writer.WriteLine(entityDataRow.Remove(entityDataRow.Length - 2));
                }
            }
            File.Move(temporaryFilePath, filePath, true);

            return true;
        }

        public bool CreateNewEntity(T entityToCreate)
        {
            if (!IsDataValid(entityToCreate))
                return false;

            uint index = GetAllEntities().LastOrDefault()?.Id ?? 0;
            entityToCreate.Id = index + 1;

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                string entityDataRow = null;
                foreach (var property in typeof(T).GetProperties())
                {
                    if (property.Name.ToLower() == "createdate" || property.Name.ToLower() == "updatedate")
                    {
                        entityDataRow += $"{DateTimeOffset.UtcNow}, ";
                        continue;
                    }
                    entityDataRow += $"{property.GetValue(entityToCreate)}, ";
                }
                writer.WriteLine(entityDataRow.Remove(entityDataRow.Length - 2));
            }

            return true;
        }

        public bool DeleteEntity(uint entityId)
        {
            if (entityId == 0)
                return false;

            using (StreamWriter writer = new StreamWriter(temporaryFilePath, true))
            {
                foreach (T entity in GetAllEntities().Where(u => u.Id != entityId))
                {
                    string entityDataRow = null;

                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (property.Name.ToLower() == "id" && (int)property.GetValue(entity) > entityId)
                        {
                            property.SetValue(entity, (int)property.GetValue(entity) - 1);
                        }
                        entityDataRow += $"{property.GetValue(entity)}, ";
                    }
                    writer.WriteLine(entityDataRow.Remove(entityDataRow.Length - 2));
                }
            }
            File.Move(temporaryFilePath, filePath, true);

            return true;
        }

        public bool UserAuthentication(User userToAuthenticate)
        {
            return GetUser(userToAuthenticate.Username) == null ? false : GetUser(userToAuthenticate.Username).Password == userToAuthenticate.Password;
        }
    }
}
