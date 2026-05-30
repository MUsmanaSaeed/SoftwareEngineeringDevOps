using SoftwareEngineeringDevOps.App.Users.Persistence;
using SoftwareEngineeringDevOps.App.Users.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Users.Repository
{
    /// <summary>
    /// Sits between the mediator and the DB layer.
    /// Caches users by id so repeated lookups (e.g. hydrating received records)
    /// never hit the database more than once per id per application lifetime.
    /// </summary>
    public class UsersRepository : IUsersRepository
    {
        IUsersDB Db { get; }
        readonly Dictionary<long, IUser> _cacheById = [];

        public UsersRepository(IUsersDB db)
        {
            Db = db;
        }

        public IEnumerable<IUser> ListAll()
        {
            IEnumerable<User> users = Db.ListAll().Select(User.FromDBO).ToList();
            foreach (User user in users)
            {
                _cacheById[user.Id] = user;
            }
            return users;
        }

        public IUser? GetById(long id)
        {
            if (_cacheById.TryGetValue(id, out var cached)) return cached;
            UserDBO? dbo = Db.GetById(id);
            if (dbo == null) return null;
            User user = User.FromDBO(dbo);
            _cacheById[user.Id] = user;
            return user;
        }

        public IUser? GetByUsername(string username)
        {
            IUser? existing = _cacheById.Values.FirstOrDefault(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
            if (existing != null) return existing;

            UserDBO? dbo = Db.GetByUsername(username);
            if (dbo == null) return null;
            User user = User.FromDBO(dbo);
            _cacheById[user.Id] = user;
            return user;
        }

        public IUser Insert(NewUser user)
        {
            UserDBO dbo = Db.Insert(user);
            User created = User.FromDBO(dbo);
            _cacheById[created.Id] = created;
            return created;
        }

        public IUser Update(EditUser user)
        {
            UserDBO dbo = Db.Update(user);
            User updated = User.FromDBO(dbo);
            _cacheById[updated.Id] = updated;
            return updated;
        }

        public void Delete(long id)
        {
            Db.Delete(id);
            _cacheById.Remove(id);
        }
    }
}
