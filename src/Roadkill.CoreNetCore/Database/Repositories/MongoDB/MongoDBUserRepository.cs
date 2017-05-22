﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDBUserRepository : IUserRepository
	{
		internal readonly string ConnectionString;

		public IQueryable<User> Users => Queryable<User>();

		public MongoDBUserRepository(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public void Wipe()
		{
			string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
			MongoClient client = new MongoClient(ConnectionString);
			IMongoDatabase database = client.GetDatabase(databaseName, new MongoDatabaseSettings());

			database.DropCollection(typeof(PageContent).Name);
			database.DropCollection(typeof(Page).Name);
			database.DropCollection(typeof(User).Name);
			database.DropCollection(typeof(SiteConfigurationEntity).Name);
		}

		private IMongoCollection<T> GetCollection<T>()
		{
			string databaseName = MongoUrl.Create(ConnectionString).DatabaseName;
			MongoClient client = new MongoClient(ConnectionString);
			IMongoDatabase database = client.GetDatabase(databaseName, new MongoDatabaseSettings());

			return database.GetCollection<T>(typeof(T).Name);
		}

		public void Delete<T>(T obj) where T : IDataStoreEntity
		{
			IMongoCollection<T> collection = GetCollection<T>();
			collection.DeleteOne(x => x.ObjectId == obj.ObjectId);
		}

		public void DeleteUser(User user)
		{
			Delete<User>(user);
		}

		public void DeleteAll<T>() where T : IDataStoreEntity
		{
			IMongoCollection<T> collection = GetCollection<T>();
			collection.DeleteMany(x => x.ObjectId != Guid.Empty);
		}

		public IQueryable<T> Queryable<T>() where T : IDataStoreEntity
		{
			return GetCollection<T>().AsQueryable();
		}

		public void SaveOrUpdate<T>(T obj) where T : IDataStoreEntity
		{
			IMongoCollection<T> collection = GetCollection<T>();
			collection.FindOneAndReplace(t => t.ObjectId == obj.ObjectId, obj);
		}

		public User GetAdminById(Guid id)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsAdmin);
		}

		public User GetUserByActivationKey(string key)
		{
			return Users.FirstOrDefault(x => x.ActivationKey == key && x.IsActivated == false);
		}

		public User GetEditorById(Guid id)
		{
			return Users.FirstOrDefault(x => x.Id == id && x.IsEditor);
		}

		public User GetUserByEmail(string email, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated.HasValue);
			else
				return Users.FirstOrDefault(x => x.Email == email);
		}

		public User GetUserById(Guid id, bool? isActivated = null)
		{
			if (isActivated.HasValue)
				return Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated.Value);
			else
				return Users.FirstOrDefault(x => x.Id == id);
		}

		public User GetUserByPasswordResetKey(string key)
		{
			return Users.FirstOrDefault(x => x.PasswordResetKey == key);
		}

		public User GetUserByUsername(string username)
		{
			return Users.FirstOrDefault(x => x.Username == username);
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			return Users.FirstOrDefault(x => x.Username == username || x.Email == email);
		}

		public IEnumerable<User> FindAllEditors()
		{
			return Users.Where(x => x.IsEditor);
		}

		public IEnumerable<User> FindAllAdmins()
		{
			return Users.Where(x => x.IsAdmin);
		}

		public void DeleteAllUsers()
		{
			DeleteAll<User>();
		}

		public User SaveOrUpdateUser(User user)
		{
			SaveOrUpdate<User>(user);
			return user;
		}


		public void Dispose()
		{

		}
	}
}
